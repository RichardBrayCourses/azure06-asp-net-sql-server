using AllChecksOut.Cases.Api.CurrentUser;
using AllChecksOut.Cases.Api.Authentication;
using AllChecksOut.Cases.Api.Data;
using AllChecksOut.Cases.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AllChecksOut.Cases.Api.Endpoints;

public static class CasesApiEndpoints
{
    public static IEndpointRouteBuilder MapCasesApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Redirect("/health")).AllowAnonymous();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
        app.MapGet("/api/demo/sign-in-options", async (
            HttpContext httpContext,
            [FromServices] IOptions<DemoSignInOptions> options,
            [FromServices] AllChecksOutDbContext db,
            CancellationToken cancellationToken) =>
        {
            if (!IsDemoSignInRequestAllowed(httpContext, options.Value))
            {
                return Results.Unauthorized();
            }

            var users = await db.UserAccounts.AsNoTracking()
                .Where(user => user.Status == "ACTIVE")
                .OrderBy(user => user.DisplayName)
                .ToListAsync(cancellationToken);
            var userIds = users.Select(user => user.Id).ToArray();

            var authorityUsers = await db.AuthorityUsers.AsNoTracking()
                .Where(membership => userIds.Contains(membership.UserAccountId))
                .OrderBy(membership => membership.EntityId)
                .ThenBy(membership => membership.UserAccountId)
                .Select(membership => new DemoSignInMembershipDto(membership.Id, "authority", membership.EntityId, membership.UserAccountId))
                .ToListAsync(cancellationToken);
            var participantUsers = await db.ParticipantUsers.AsNoTracking()
                .Where(membership => userIds.Contains(membership.UserAccountId))
                .OrderBy(membership => membership.EntityId)
                .ThenBy(membership => membership.UserAccountId)
                .Select(membership => new DemoSignInMembershipDto(membership.Id, "participant", membership.EntityId, membership.UserAccountId))
                .ToListAsync(cancellationToken);
            var stakeholderUsers = await db.StakeholderUsers.AsNoTracking()
                .Where(membership => userIds.Contains(membership.UserAccountId))
                .OrderBy(membership => membership.EntityId)
                .ThenBy(membership => membership.UserAccountId)
                .Select(membership => new DemoSignInMembershipDto(membership.Id, "stakeholder", membership.EntityId, membership.UserAccountId))
                .ToListAsync(cancellationToken);
            var agentUsers = await db.AgentUsers.AsNoTracking()
                .Where(membership => userIds.Contains(membership.UserAccountId))
                .OrderBy(membership => membership.EntityId)
                .ThenBy(membership => membership.UserAccountId)
                .Select(membership => new DemoSignInMembershipDto(membership.Id, "agent", membership.EntityId, membership.UserAccountId))
                .ToListAsync(cancellationToken);

            var memberships = authorityUsers
                .Concat(participantUsers)
                .Concat(stakeholderUsers)
                .Concat(agentUsers)
                .ToArray();

            var participantIds = memberships.Where(membership => membership.Type == "participant").Select(membership => membership.EntityId).Distinct().ToArray();
            var stakeholderIds = memberships.Where(membership => membership.Type == "stakeholder").Select(membership => membership.EntityId).Distinct().ToArray();
            var agentIds = memberships.Where(membership => membership.Type == "agent").Select(membership => membership.EntityId).Distinct().ToArray();

            var participants = await db.Participants.AsNoTracking()
                .Where(participant => participantIds.Contains(participant.Id))
                .OrderBy(participant => participant.DisplayName)
                .ToListAsync(cancellationToken);
            var stakeholders = await db.Stakeholders.AsNoTracking()
                .Where(stakeholder => stakeholderIds.Contains(stakeholder.Id))
                .OrderBy(stakeholder => stakeholder.DisplayName)
                .ToListAsync(cancellationToken);
            var agents = await db.Agents.AsNoTracking()
                .Where(agent => agentIds.Contains(agent.Id))
                .OrderBy(agent => agent.DisplayName)
                .ToListAsync(cancellationToken);

            var authorityIds = memberships.Where(membership => membership.Type == "authority").Select(membership => membership.EntityId)
                .Concat(participants.Select(participant => participant.AuthorityId))
                .Concat(stakeholders.Select(stakeholder => stakeholder.AuthorityId))
                .Concat(agents.Select(agent => agent.AuthorityId))
                .Distinct()
                .ToArray();
            var authorities = await db.Authorities.AsNoTracking()
                .Where(authority => authorityIds.Contains(authority.Id))
                .OrderBy(authority => authority.Name)
                .ToListAsync(cancellationToken);

            var optionsDto = new DemoSignInOptionsDto(authorities, participants, stakeholders, agents, users, memberships);

            return Results.Ok(optionsDto);
        }).AllowAnonymous();

        app.MapGet("/api/me", async ([FromServices] ApplicationUserResolver resolver, CancellationToken cancellationToken) =>
            Results.Ok(await resolver.ResolveAsync(cancellationToken)));

        var api = app.MapGroup("/api");
        api.AddEndpointFilter(async (context, next) =>
        {
            try
            {
                return await next(context);
            }
            catch (DomainException exception)
            {
                return Results.Problem(exception.Message, statusCode: exception.StatusCode);
            }
        });

        api.MapGet("/authorities", async ([FromServices] CasesDomainService domain, CancellationToken ct) =>
            await domain.ListAuthoritiesAsync(ct));
        api.MapGet("/authorities/{authorityId}/terminology", async ([FromServices] CasesDomainService domain, string authorityId, CancellationToken ct) =>
            DomainDto.AuthorityTerminology(await domain.GetAuthorityTerminologyAsync(authorityId, ct)));
        api.MapPut("/authorities/{authorityId}/terminology", async ([FromServices] CasesDomainService domain, string authorityId, UpdateAuthorityTerminologyCommand command, CancellationToken ct) =>
            DomainDto.AuthorityTerminology(await domain.UpdateAuthorityTerminologyAsync(authorityId, command, ct)));

        api.MapGet("/participants", async ([FromServices] CasesDomainService domain, string? authorityId, CancellationToken ct) =>
            await domain.ListParticipantsAsync(authorityId, ct));
        api.MapPost("/participants", async ([FromServices] CasesDomainService domain, CreateParticipantCommand command, CancellationToken ct) =>
            await domain.CreateParticipantAsync(command, ct));
        api.MapGet("/participants/{participantId}/users", async ([FromServices] CasesDomainService domain, string participantId, CancellationToken ct) =>
            await domain.ListMembershipUsersAsync("participant", participantId, ct));
        api.MapPost("/participants/{participantId}/users", async ([FromServices] CasesDomainService domain, string participantId, EntityUserCommand command, CancellationToken ct) =>
            await domain.CreateMembershipUserAsync("participant", participantId, command, ct));
        api.MapGet("/participants/{participantId}/suppliers", async ([FromServices] CasesDomainService domain, string participantId, CancellationToken ct) =>
            await domain.ListParticipantSuppliersAsync(participantId, ct));
        api.MapPost("/participant-suppliers", async ([FromServices] CasesDomainService domain, CreateParticipantSupplierCommand command, CancellationToken ct) =>
            await domain.CreateParticipantSupplierAsync(command, ct));

        api.MapGet("/stakeholders", async ([FromServices] CasesDomainService domain, string authorityId, CancellationToken ct) =>
            await domain.ListStakeholdersAsync(authorityId, ct));
        api.MapPost("/stakeholders", async ([FromServices] CasesDomainService domain, CreateEntityCommand command, CancellationToken ct) =>
            await domain.CreateStakeholderAsync(command, ct));
        api.MapGet("/stakeholders/{stakeholderId}/users", async ([FromServices] CasesDomainService domain, string stakeholderId, CancellationToken ct) =>
            await domain.ListMembershipUsersAsync("stakeholder", stakeholderId, ct));
        api.MapPost("/stakeholders/{stakeholderId}/users", async ([FromServices] CasesDomainService domain, string stakeholderId, EntityUserCommand command, CancellationToken ct) =>
            await domain.CreateMembershipUserAsync("stakeholder", stakeholderId, command, ct));
        api.MapGet("/stakeholders/{stakeholderId}/access", async ([FromServices] CasesDomainService domain, string stakeholderId, CancellationToken ct) =>
            await domain.ListStakeholderAccessAsync(stakeholderId, ct));
        api.MapPost("/stakeholder-access", async ([FromServices] CasesDomainService domain, GrantStakeholderAccessCommand command, CancellationToken ct) =>
            await domain.GrantStakeholderAccessAsync(command, ct));

        api.MapGet("/agents", async ([FromServices] CasesDomainService domain, string authorityId, CancellationToken ct) =>
            await domain.ListAgentsAsync(authorityId, ct));
        api.MapPost("/agents", async ([FromServices] CasesDomainService domain, CreateEntityCommand command, CancellationToken ct) =>
            await domain.CreateAgentAsync(command, ct));
        api.MapGet("/agents/{agentId}/users", async ([FromServices] CasesDomainService domain, string agentId, CancellationToken ct) =>
            await domain.ListMembershipUsersAsync("agent", agentId, ct));
        api.MapPost("/agents/{agentId}/users", async ([FromServices] CasesDomainService domain, string agentId, EntityUserCommand command, CancellationToken ct) =>
            await domain.CreateMembershipUserAsync("agent", agentId, command, ct));

        api.MapGet("/users", async ([FromServices] CasesDomainService domain, CancellationToken ct) =>
            await domain.ListUserAccountsAsync(ct));
        api.MapPatch("/users/{userAccountId}/email", async ([FromServices] CasesDomainService domain, string userAccountId, UpdateUserEmailCommand command, CancellationToken ct) =>
            await domain.UpdateUserEmailAsync(userAccountId, command, ct));
        api.MapPatch("/users/{userAccountId}/entra", async ([FromServices] CasesDomainService domain, string userAccountId, RegisterUserAccountWithEntraCommand command, CancellationToken ct) =>
            await domain.RegisterUserAccountWithEntraAsync(userAccountId, command, ct));
        api.MapGet("/authorities/{authorityId}/users", async ([FromServices] CasesDomainService domain, string authorityId, CancellationToken ct) =>
            await domain.ListMembershipUsersAsync("authority", authorityId, ct));
        api.MapPost("/authorities/{authorityId}/users", async ([FromServices] CasesDomainService domain, string authorityId, EntityUserCommand command, CancellationToken ct) =>
            await domain.CreateMembershipUserAsync("authority", authorityId, command, ct));

        api.MapGet("/task-types", async ([FromServices] CasesDomainService domain, CancellationToken ct) =>
            await domain.ListTaskTypesAsync(ct));
        api.MapGet("/case-templates", async ([FromServices] CasesDomainService domain, string authorityId, CancellationToken ct) =>
            await domain.ListCaseTemplatesAsync(authorityId, ct));
        api.MapPost("/case-templates", async ([FromServices] CasesDomainService domain, CreateCaseTemplateCommand command, CancellationToken ct) =>
            await domain.CreateCaseTemplateAsync(command, ct));
        api.MapDelete("/case-templates/{caseTemplateId}", async ([FromServices] CasesDomainService domain, string caseTemplateId, CancellationToken ct) =>
            await domain.DeleteCaseTemplateAsync(caseTemplateId, ct));
        api.MapPost("/case-templates/{caseTemplateId}/finalize", async ([FromServices] CasesDomainService domain, string caseTemplateId, CancellationToken ct) =>
            await domain.FinalizeCaseTemplateAsync(caseTemplateId, ct));
        api.MapGet("/case-templates/{caseTemplateId}/tasks", async ([FromServices] CasesDomainService domain, string caseTemplateId, CancellationToken ct) =>
            (await domain.ListTemplateTasksAsync(caseTemplateId, ct)).Select(DomainDto.TemplateTask));
        api.MapPost("/case-templates/{caseTemplateId}/tasks", async ([FromServices] CasesDomainService domain, string caseTemplateId, AddTaskToTemplateCommand command, CancellationToken ct) =>
            DomainDto.TemplateTask(await domain.AddTaskToTemplateAsync(caseTemplateId, command, ct)));
        api.MapPost("/template-tasks/{templateTaskId}/withdraw", async ([FromServices] CasesDomainService domain, string templateTaskId, WithdrawTemplateTaskCommand command, CancellationToken ct) =>
            DomainDto.TemplateTask(await domain.WithdrawTemplateTaskAsync(templateTaskId, command, ct)));
        api.MapGet("/case-templates/{caseTemplateId}/participants", async ([FromServices] CasesDomainService domain, string caseTemplateId, CancellationToken ct) =>
            await domain.ListCaseTemplateParticipantsAsync(caseTemplateId, ct));
        api.MapPost("/case-templates/{caseTemplateId}/participants", async ([FromServices] CasesDomainService domain, string caseTemplateId, AssignParticipantToTemplateCommand command, CancellationToken ct) =>
            await domain.AssignParticipantToTemplateAsync(caseTemplateId, command, ct));

        api.MapGet("/cases", async ([FromServices] CasesDomainService domain, string? participantId, string? authorityId, CancellationToken ct) =>
            await domain.ListCasesAsync(participantId, authorityId, ct));
        api.MapPost("/cases/{caseId}/submit", async ([FromServices] CasesDomainService domain, string caseId, CancellationToken ct) =>
            await domain.SubmitCaseAsync(caseId, ct));
        api.MapPost("/cases/{caseId}/withdraw", async ([FromServices] CasesDomainService domain, string caseId, WithdrawCaseCommand command, CancellationToken ct) =>
            await domain.WithdrawCaseAsync(caseId, command, ct));
        api.MapPost("/cases/{caseId}/reinstate", async ([FromServices] CasesDomainService domain, string caseId, CancellationToken ct) =>
            await domain.ReinstateCaseAsync(caseId, ct));
        api.MapPut("/cases/{caseId}/participant-supplier", async ([FromServices] CasesDomainService domain, string caseId, LinkParticipantSupplierCommand command, CancellationToken ct) =>
            await domain.LinkParticipantSupplierToCaseAsync(caseId, command, ct));
        api.MapDelete("/cases/{caseId}/participant-supplier", async ([FromServices] CasesDomainService domain, string caseId, CancellationToken ct) =>
            await domain.UnlinkParticipantSupplierFromCaseAsync(caseId, ct));
        api.MapGet("/cases/{caseId}/tasks", async ([FromServices] CasesDomainService domain, string caseId, CancellationToken ct) =>
            (await domain.ListTasksAsync(caseId, ct)).Select(DomainDto.Task));
        api.MapGet("/cases/{caseId}/stakeholder-reviews", async ([FromServices] CasesDomainService domain, string caseId, CancellationToken ct) =>
            await domain.ListStakeholderReviewsAsync(caseId, ct));

        api.MapPost("/tasks/{taskId}/complete", async ([FromServices] CasesDomainService domain, string taskId, CompleteTaskCommand command, CancellationToken ct) =>
            DomainDto.Task(await domain.CompleteTaskAsync(taskId, command, ct)));
        api.MapPost("/tasks/{taskId}/evidence", async ([FromServices] CasesDomainService domain, string taskId, UploadEvidenceCommand command, CancellationToken ct) =>
            DomainDto.Task(await domain.UploadEvidenceAsync(taskId, command, ct)));
        api.MapPost("/tasks/{taskId}/submit", async ([FromServices] CasesDomainService domain, string taskId, CancellationToken ct) =>
            DomainDto.Task(await domain.SubmitTaskAsync(taskId, ct)));
        api.MapPost("/tasks/{taskId}/review", async ([FromServices] CasesDomainService domain, string taskId, ReviewTaskCommand command, CancellationToken ct) =>
            DomainDto.Task(await domain.ReviewTaskAsync(taskId, command, ct)));

        api.MapGet("/access-grants", async ([FromServices] CasesDomainService domain, string? participantId, string? stakeholderId, string? agentId, CancellationToken ct) =>
            await domain.ListAccessGrantsAsync(participantId, stakeholderId, agentId, ct));
        api.MapPost("/access-grants", async ([FromServices] CasesDomainService domain, CreateAccessGrantCommand command, CancellationToken ct) =>
            await domain.CreateAccessGrantAsync(command, ct));
        api.MapPatch("/access-grants/{accessGrantId}/status", async ([FromServices] CasesDomainService domain, string accessGrantId, UpdateAccessGrantStatusCommand command, CancellationToken ct) =>
            await domain.UpdateAccessGrantStatusAsync(accessGrantId, command, ct));

        api.MapPost("/stakeholder-reviews", async ([FromServices] CasesDomainService domain, UpsertStakeholderReviewCommand command, CancellationToken ct) =>
            await domain.UpsertStakeholderReviewAsync(command, ct));
        api.MapGet("/requests-for-information", async ([FromServices] CasesDomainService domain, string? caseId, string? participantId, CancellationToken ct) =>
            (await domain.ListRequestsForInformationAsync(caseId, participantId, ct)).Select(DomainDto.RequestForInformation));
        api.MapPost("/requests-for-information", async ([FromServices] CasesDomainService domain, CreateRequestForInformationCommand command, CancellationToken ct) =>
            DomainDto.RequestForInformation(await domain.CreateRequestForInformationAsync(command, ct)));
        api.MapPost("/requests-for-information/{requestId}/response", async ([FromServices] CasesDomainService domain, string requestId, RespondToRequestForInformationCommand command, CancellationToken ct) =>
            DomainDto.RequestForInformation(await domain.RespondToRequestForInformationAsync(requestId, command, ct)));
        api.MapPatch("/requests-for-information/{requestId}/status", async ([FromServices] CasesDomainService domain, string requestId, UpdateRequestForInformationStatusCommand command, CancellationToken ct) =>
            DomainDto.RequestForInformation(await domain.UpdateRequestForInformationStatusAsync(requestId, command, ct)));

        return app;
    }

    private static bool IsDemoSignInRequestAllowed(HttpContext httpContext, DemoSignInOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Key))
        {
            return false;
        }

        return httpContext.Request.Headers.TryGetValue("X-Demo-SignIn-Key", out var keyValues) &&
            string.Equals(keyValues.ToString(), options.Key, StringComparison.Ordinal);
    }
}
