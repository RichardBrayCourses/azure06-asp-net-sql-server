using System.Security.Claims;
using AllChecksOut.Cases.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AllChecksOut.Cases.Api.CurrentUser;

public sealed class ApplicationUserResolver(
    IHttpContextAccessor httpContextAccessor,
    AllChecksOutDbContext db)
{
    public async Task<CurrentUserProfile> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No active HTTP context is available.");
        var identity = EntraIdentity.FromClaims(principal);
        var applicationUser = await db.UserAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.EntraObjectId == identity.ObjectId, cancellationToken);

        if (applicationUser is null)
        {
            return new CurrentUserProfile(identity, null, []);
        }

        var memberships = await ResolveMembershipsAsync(applicationUser.Id, cancellationToken);

        return new CurrentUserProfile(
            identity,
            new ApplicationUser(
                applicationUser.Id,
                applicationUser.Email,
                applicationUser.DisplayName,
                applicationUser.Status),
            memberships);
    }

    public async Task<ApplicationUserContext?> ResolveApplicationUserAsync(CancellationToken cancellationToken = default)
    {
        var profile = await ResolveAsync(cancellationToken);
        return profile.ApplicationUser is null
            ? null
            : new ApplicationUserContext(profile.ApplicationUser, profile.Memberships);
    }

    private async Task<ApplicationMembership[]> ResolveMembershipsAsync(
        string userAccountId,
        CancellationToken cancellationToken)
    {
        var systemOwnerMemberships = await (
            from membership in db.SystemOwnerUsers.AsNoTracking()
            join systemOwner in db.SystemOwners.AsNoTracking() on membership.EntityId equals systemOwner.Id
            where membership.UserAccountId == userAccountId
            select new ApplicationMembership(
                "system-owner",
                systemOwner.Id,
                systemOwner.Name,
                null,
                null))
            .ToListAsync(cancellationToken);

        var authorityMemberships = await (
            from membership in db.AuthorityUsers.AsNoTracking()
            join authority in db.Authorities.AsNoTracking() on membership.EntityId equals authority.Id
            where membership.UserAccountId == userAccountId
            select new ApplicationMembership(
                "authority",
                authority.Id,
                authority.Name,
                authority.Id,
                authority.Name))
            .ToListAsync(cancellationToken);

        var participantMemberships = await (
            from membership in db.ParticipantUsers.AsNoTracking()
            join participant in db.Participants.AsNoTracking() on membership.EntityId equals participant.Id
            join authority in db.Authorities.AsNoTracking() on participant.AuthorityId equals authority.Id
            where membership.UserAccountId == userAccountId
            select new ApplicationMembership(
                "participant",
                participant.Id,
                participant.DisplayName,
                authority.Id,
                authority.Name))
            .ToListAsync(cancellationToken);

        var stakeholderMemberships = await (
            from membership in db.StakeholderUsers.AsNoTracking()
            join stakeholder in db.Stakeholders.AsNoTracking() on membership.EntityId equals stakeholder.Id
            join authority in db.Authorities.AsNoTracking() on stakeholder.AuthorityId equals authority.Id
            where membership.UserAccountId == userAccountId
            select new ApplicationMembership(
                "stakeholder",
                stakeholder.Id,
                stakeholder.DisplayName,
                authority.Id,
                authority.Name))
            .ToListAsync(cancellationToken);

        var agentMemberships = await (
            from membership in db.AgentUsers.AsNoTracking()
            join agent in db.Agents.AsNoTracking() on membership.EntityId equals agent.Id
            join authority in db.Authorities.AsNoTracking() on agent.AuthorityId equals authority.Id
            where membership.UserAccountId == userAccountId
            select new ApplicationMembership(
                "agent",
                agent.Id,
                agent.DisplayName,
                authority.Id,
                authority.Name))
            .ToListAsync(cancellationToken);

        return systemOwnerMemberships
            .Concat(authorityMemberships)
            .Concat(participantMemberships)
            .Concat(stakeholderMemberships)
            .Concat(agentMemberships)
            .OrderBy(membership => membership.Type, StringComparer.Ordinal)
            .ThenBy(membership => membership.EntityName, StringComparer.Ordinal)
            .ToArray();
    }
}

public sealed record CurrentUserProfile(
    EntraIdentity Identity,
    ApplicationUser? ApplicationUser,
    IReadOnlyList<ApplicationMembership> Memberships);

public sealed record EntraIdentity(
    string ObjectId,
    string? TenantId,
    string? Subject,
    string? Email,
    string? DisplayName)
{
    public static EntraIdentity FromClaims(ClaimsPrincipal principal)
    {
        var objectId = ClaimValue(principal, "oid")
            ?? ClaimValue(principal, "http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? throw new InvalidOperationException("The authenticated token did not include an oid claim.");

        return new EntraIdentity(
            objectId,
            ClaimValue(principal, "tid") ?? ClaimValue(principal, "http://schemas.microsoft.com/identity/claims/tenantid"),
            ClaimValue(principal, "sub") ?? ClaimValue(principal, ClaimTypes.NameIdentifier),
            ClaimValue(principal, "email") ?? ClaimValue(principal, "preferred_username") ?? ClaimValue(principal, ClaimTypes.Email),
            ClaimValue(principal, "name") ?? ClaimValue(principal, ClaimTypes.Name));
    }

    private static string? ClaimValue(ClaimsPrincipal principal, string type)
    {
        var value = principal.FindFirst(type)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}

public sealed record ApplicationUser(
    string Id,
    string Email,
    string DisplayName,
    string Status);

public sealed record ApplicationMembership(
    string Type,
    string EntityId,
    string EntityName,
    string? AuthorityId,
    string? AuthorityName);

public sealed record ApplicationUserContext(
    ApplicationUser User,
    IReadOnlyList<ApplicationMembership> Memberships)
{
    public bool IsAuthorityMember(string authorityId) =>
        HasMembership("authority", authorityId);

    public bool IsParticipantMember(string participantId) =>
        HasMembership("participant", participantId);

    public bool IsStakeholderMember(string stakeholderId) =>
        HasMembership("stakeholder", stakeholderId);

    public bool IsAgentMember(string agentId) =>
        HasMembership("agent", agentId);

    private bool HasMembership(string type, string entityId) =>
        Memberships.Any(membership =>
            string.Equals(membership.Type, type, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(membership.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
}
