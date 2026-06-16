using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using AllChecksOut.Cases.Api.CurrentUser;
using AllChecksOut.Cases.Api.Data;
using AllChecksOut.Cases.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AllChecksOut.Cases.Api.Tests;

public sealed class MeEndpointTests
{
    [Fact]
    public async Task Health_IsPublic()
    {
        await using var app = await CasesApiTestApp.CreateAsync();
        using var client = app.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Me_RequiresAuthentication()
    {
        await using var app = await CasesApiTestApp.CreateAsync();
        using var client = app.CreateClient();

        var response = await client.GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsAuthenticatedIdentityAndResolvedMemberships()
    {
        await using var app = await CasesApiTestApp.CreateAsync();
        using var client = app.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            TestAuthHandler.SchemeName,
            "entra-user-aisha-khan");

        var response = await client.GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var body = await JsonDocument.ParseAsync(stream);

        Assert.Equal("entra-user-aisha-khan", body.RootElement.GetProperty("identity").GetProperty("objectId").GetString());
        Assert.Equal("user-aisha-khan", body.RootElement.GetProperty("applicationUser").GetProperty("id").GetString());
        Assert.Contains(body.RootElement.GetProperty("memberships").EnumerateArray(), membership =>
            membership.GetProperty("type").GetString() == "participant" &&
            membership.GetProperty("entityId").GetString() == "northstar-cloud" &&
            membership.GetProperty("authorityId").GetString() == "northstar-association");
    }

    private sealed class CasesApiTestApp : IAsyncDisposable
    {
        private readonly WebApplication app;
        private readonly SqliteConnection connection;

        private CasesApiTestApp(WebApplication app, SqliteConnection connection)
        {
            this.app = app;
            this.connection = connection;
        }

        public static async Task<CasesApiTestApp> CreateAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();

            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            builder.Services.AddDbContext<AllChecksOutDbContext>(options => options.UseSqlite(connection));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ApplicationUserResolver>();
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
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

            return new CasesApiTestApp(app, connection);
        }

        public HttpClient CreateClient() => app.GetTestClient();

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
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!AuthenticationHeaderValue.TryParse(authorizationValues, out var authorization) ||
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
                new Claim("email", "aisha.khan@northstar-cloud.example"),
                new Claim("name", "Aisha Khan"),
            };
            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
