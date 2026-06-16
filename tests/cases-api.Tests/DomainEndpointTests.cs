using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AllChecksOut.Cases.Api.CurrentUser;
using AllChecksOut.Cases.Api.Data;
using AllChecksOut.Cases.Api.Domain;
using AllChecksOut.Cases.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AllChecksOut.Cases.Api.Tests;

public sealed class DomainEndpointTests
{
    [Fact]
    public async Task DomainEndpoints_RequireAuthentication()
    {
        await using var app = await TestCasesApi.CreateAsync();
        using var client = app.CreateClient();

        var response = await client.GetAsync("/api/participants?authorityId=northstar-association");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Stakeholder_CannotReadParticipantOutsideActiveGrantScope()
    {
        await using var app = await TestCasesApi.CreateAsync();
        using var client = app.CreateClient("entra-user-rachel-morgan");

        var response = await client.GetAsync("/api/cases?participantId=pinebridge-data");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotSubmitCaseWithInProgressTasks()
    {
        await using var app = await TestCasesApi.CreateAsync();
        using var client = app.CreateClient("entra-user-lewis-green");

        var response = await client.PostAsync("/api/cases/case-2026-cobalt/submit", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Authority_CanAssignFinalizedTemplateButCannotAssignDuplicate()
    {
        await using var app = await TestCasesApi.CreateAsync();
        using var client = app.CreateClient("entra-user-jonathan-price");

        var createTemplateResponse = await client.PostAsJsonAsync("/api/case-templates", new
        {
            authorityId = "northstar-association",
            name = "Focused reassessment",
            description = "Small test template",
        });
        Assert.Equal(HttpStatusCode.OK, createTemplateResponse.StatusCode);
        var template = await createTemplateResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var templateId = template!["id"].ToString();

        var addTaskResponse = await client.PostAsJsonAsync($"/api/case-templates/{templateId}/tasks", new
        {
            taskTypeId = "task-type-questionnaire",
            title = "Confirm scope",
            description = "Confirm the reassessment scope.",
            parametersJson = new { due = "No due date" },
        });
        Assert.Equal(HttpStatusCode.OK, addTaskResponse.StatusCode);

        var finalizeResponse = await client.PostAsync($"/api/case-templates/{templateId}/finalize", null);
        Assert.Equal(HttpStatusCode.OK, finalizeResponse.StatusCode);

        var assignResponse = await client.PostAsJsonAsync($"/api/case-templates/{templateId}/participants", new
        {
            participantId = "asteria-identity",
            exemptionReason = (string?)null,
            decidedByUserId = "user-jonathan-price",
        });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync($"/api/case-templates/{templateId}/participants", new
        {
            participantId = "asteria-identity",
            exemptionReason = (string?)null,
            decidedByUserId = "user-jonathan-price",
        });
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Stakeholder_WithReadOnlyGrantCannotCreateRequestForInformation()
    {
        await using var app = await TestCasesApi.CreateAsync();
        await app.AddReadOnlyStakeholderGrantAsync();
        using var client = app.CreateClient("entra-user-rachel-morgan");

        var response = await client.PostAsJsonAsync("/api/requests-for-information", new
        {
            stakeholderId = "harrington-financial",
            caseId = "case-2026-pinebridge",
            taskId = (string?)null,
            requestText = "Please provide more detail.",
            requestedByUserId = "user-rachel-morgan",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed class TestCasesApi : IAsyncDisposable
    {
        private readonly WebApplication app;
        private readonly SqliteConnection connection;

        private TestCasesApi(WebApplication app, SqliteConnection connection)
        {
            this.app = app;
            this.connection = connection;
        }

        public static async Task<TestCasesApi> CreateAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();

            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            builder.Services.AddDbContext<AllChecksOutDbContext>(options => options.UseSqlite(connection));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ApplicationUserResolver>();
            builder.Services.AddScoped<CasesDomainService>();
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapCasesApiEndpoints();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AllChecksOutDbContext>();
                await db.Database.EnsureCreatedAsync();
            }

            await app.StartAsync();
            return new TestCasesApi(app, connection);
        }

        public HttpClient CreateClient(string? entraObjectId = null)
        {
            var client = app.GetTestClient();
            if (entraObjectId is not null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName, entraObjectId);
            }
            return client;
        }

        public async Task AddReadOnlyStakeholderGrantAsync()
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AllChecksOutDbContext>();
            var now = DateTimeOffset.UtcNow;
            db.AccessGrants.Add(new()
            {
                Id = "grant-harrington-pinebridge-read-only",
                AuthorityId = "northstar-association",
                ParticipantId = "pinebridge-data",
                GranteeType = "STAKEHOLDER",
                GranteeStakeholderId = "harrington-financial",
                PermissionLevel = "READ_ONLY",
                DataScopeType = "PARTICIPANT",
                Status = "ACTIVE",
                CreatedByUserId = "user-maya-patel",
                CreatedAt = now,
                UpdatedAt = now,
            });
            await db.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await app.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationValues) ||
                !AuthenticationHeaderValue.TryParse(authorizationValues, out var authorization) ||
                !string.Equals(authorization.Scheme, SchemeName, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(authorization.Parameter))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim("oid", authorization.Parameter),
                new Claim("tid", "tenant-test"),
                new Claim("sub", "subject-test"),
                new Claim("email", $"{authorization.Parameter}@example.test"),
                new Claim("name", authorization.Parameter),
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
        }
    }
}
