using AllChecksOut.FunctionsApi.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AllChecksOut.FunctionsApi.Tests;

public sealed class AllChecksOutDbContextTests
{
    [Fact]
    public async Task DbContext_CreatesSchema_AndQueriesSeededScenario()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AllChecksOutDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AllChecksOutDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var authority = await db.Authorities.SingleAsync();
        var participants = await db.Participants
            .Where(participant => participant.AuthorityId == authority.Id)
            .OrderBy(participant => participant.Id)
            .ToListAsync();
        var northstarTasks = await db.Tasks
            .Where(task => task.CaseId == "case-2026-northstar")
            .ToListAsync();
        var activeStakeholderGrants = await db.AccessGrants
            .Where(grant => grant.GranteeStakeholderId == "mercury-retail" && grant.Status == "ACTIVE")
            .ToListAsync();

        Assert.Equal("Digital Platform Assurance Association", authority.Name);
        Assert.Contains(participants, participant => participant.Id == "northstar-cloud");
        Assert.Contains(northstarTasks, task => task.Id == "task-northstar-subprocessors");
        Assert.Contains(activeStakeholderGrants, grant => grant.Id == "grant-mercury-northstar-stratuspay");
        Assert.Equal(3, await db.RequestsForInformation.CountAsync());
    }
}
