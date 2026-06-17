using System.Security.Claims;
using AllChecksOut.FunctionsApi.CurrentUser;
using AllChecksOut.FunctionsApi.Data;
using AllChecksOut.FunctionsApi.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AllChecksOut.FunctionsApi.Tests;

public sealed class FunctionsDomainTests
{
    [Fact]
    public async Task Resolver_ReturnsAuthenticatedIdentityAndResolvedMemberships()
    {
        await using var app = await TestBackend.CreateAsync("entra-user-aisha-khan");

        var profile = await app.Resolver.ResolveAsync();

        Assert.Equal("entra-user-aisha-khan", profile.Identity.ObjectId);
        Assert.Equal("user-aisha-khan", profile.ApplicationUser?.Id);
        Assert.Contains(profile.Memberships, membership =>
            membership.Type == "participant" &&
            membership.EntityId == "northstar-cloud" &&
            membership.AuthorityId == "northstar-association");
    }

    [Fact]
    public async Task Stakeholder_CannotReadParticipantOutsideActiveGrantScope()
    {
        await using var app = await TestBackend.CreateAsync("entra-user-rachel-morgan");

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            app.Domain.ListCasesAsync("pinebridge-data", null, CancellationToken.None));

        Assert.Equal(403, exception.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotSubmitCaseWithInProgressTasks()
    {
        await using var app = await TestBackend.CreateAsync("entra-user-lewis-green");

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            app.Domain.SubmitCaseAsync("case-2026-cobalt", CancellationToken.None));

        Assert.Equal(400, exception.StatusCode);
    }

    private sealed class TestBackend : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private TestBackend(
            SqliteConnection connection,
            ApplicationUserResolver resolver,
            CasesDomainService domain)
        {
            this.connection = connection;
            Resolver = resolver;
            Domain = domain;
        }

        public ApplicationUserResolver Resolver { get; }
        public CasesDomainService Domain { get; }

        public static async Task<TestBackend> CreateAsync(string objectId)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AllChecksOutDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new AllChecksOutDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var currentUser = new CurrentUserAccessor
            {
                Principal = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("oid", objectId),
                    new Claim("tid", "tenant-test"),
                    new Claim("sub", "subject-test"),
                    new Claim("email", $"{objectId}@example.test"),
                    new Claim("name", objectId),
                ], "Test")),
            };

            var resolver = new ApplicationUserResolver(currentUser, db);
            var domain = new CasesDomainService(db, resolver);

            return new TestBackend(connection, resolver, domain);
        }

        public async ValueTask DisposeAsync() =>
            await connection.DisposeAsync();
    }
}
