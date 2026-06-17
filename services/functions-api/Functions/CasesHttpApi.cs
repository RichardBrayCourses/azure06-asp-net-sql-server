using System.Net;
using System.Security.Claims;
using System.Text.Json;
using AllChecksOut.FunctionsApi.Authentication;
using AllChecksOut.FunctionsApi.CurrentUser;
using AllChecksOut.FunctionsApi.Data;
using AllChecksOut.FunctionsApi.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AllChecksOut.FunctionsApi.Functions;

public sealed class CasesHttpApi(
    IServiceScopeFactory scopeFactory,
    BearerTokenValidator tokenValidator,
    IOptions<CorsOptions> corsOptions)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function(nameof(CasesHttpApi))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", "get", "options", "patch", "post", "put", Route = "{*path}")]
        HttpRequestData request,
        string? path,
        FunctionContext context)
    {
        if (IsOptions(request))
        {
            return Empty(request, HttpStatusCode.NoContent);
        }

        try
        {
            var normalizedPath = NormalizePath(path, request.Url.AbsolutePath);
            var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var method = request.Method.ToUpperInvariant();

            if (method == "GET" && segments.Length == 0)
            {
                var response = request.CreateResponse(HttpStatusCode.Redirect);
                ApplyCors(response, request);
                response.Headers.Add("Location", "/health");
                return response;
            }

            if (method == "GET" && Matches(segments, "health"))
            {
                return await Json(request, new { status = "ok" });
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var services = scope.ServiceProvider;

            if (method == "GET" && Matches(segments, "api", "demo", "sign-in-options"))
            {
                return await DemoSignInOptions(request, services, context.CancellationToken);
            }

            var principal = await tokenValidator.ValidateAsync(Header(request, "Authorization"), context.CancellationToken);
            if (principal is null)
            {
                return Problem(request, HttpStatusCode.Unauthorized, "Authentication is required.");
            }

            services.GetRequiredService<CurrentUserAccessor>().Principal = principal;

            if (method == "GET" && Matches(segments, "api", "me"))
            {
                var resolver = services.GetRequiredService<ApplicationUserResolver>();
                return await Json(request, await resolver.ResolveAsync(context.CancellationToken));
            }

            if (segments.Length == 0 || !string.Equals(segments[0], "api", StringComparison.OrdinalIgnoreCase))
            {
                return Problem(request, HttpStatusCode.NotFound, "Route was not found.");
            }

            var domain = services.GetRequiredService<CasesDomainService>();
            var query = Query(request.Url);
            var result = await DispatchApiRoute(request, method, segments[1..], query, domain, context.CancellationToken);
            return result is null
                ? Problem(request, HttpStatusCode.NotFound, "Route was not found.")
                : await Json(request, result);
        }
        catch (DomainException exception)
        {
            return Problem(request, (HttpStatusCode)exception.StatusCode, exception.Message);
        }
        catch (JsonException exception)
        {
            return Problem(request, HttpStatusCode.BadRequest, exception.Message);
        }
    }

    private async Task<object?> DispatchApiRoute(
        HttpRequestData request,
        string method,
        string[] segments,
        IReadOnlyDictionary<string, string?> query,
        CasesDomainService domain,
        CancellationToken cancellationToken)
    {
        switch (segments)
        {
            case ["authorities"] when method == "GET":
                return await domain.ListAuthoritiesAsync(cancellationToken);
            case ["authorities", var authorityId, "terminology"] when method == "GET":
                return DomainDto.AuthorityTerminology(await domain.GetAuthorityTerminologyAsync(authorityId, cancellationToken));
            case ["authorities", var authorityId, "terminology"] when method == "PUT":
                return DomainDto.AuthorityTerminology(await domain.UpdateAuthorityTerminologyAsync(authorityId, await Body<UpdateAuthorityTerminologyCommand>(request), cancellationToken));
            case ["authorities", var authorityId, "users"] when method == "GET":
                return await domain.ListMembershipUsersAsync("authority", authorityId, cancellationToken);
            case ["authorities", var authorityId, "users"] when method == "POST":
                return await domain.CreateMembershipUserAsync("authority", authorityId, await Body<EntityUserCommand>(request), cancellationToken);

            case ["participants"] when method == "GET":
                return await domain.ListParticipantsAsync(Value(query, "authorityId"), cancellationToken);
            case ["participants"] when method == "POST":
                return await domain.CreateParticipantAsync(await Body<CreateParticipantCommand>(request), cancellationToken);
            case ["participants", var participantId, "users"] when method == "GET":
                return await domain.ListMembershipUsersAsync("participant", participantId, cancellationToken);
            case ["participants", var participantId, "users"] when method == "POST":
                return await domain.CreateMembershipUserAsync("participant", participantId, await Body<EntityUserCommand>(request), cancellationToken);
            case ["participants", var participantId, "suppliers"] when method == "GET":
                return await domain.ListParticipantSuppliersAsync(participantId, cancellationToken);
            case ["participant-suppliers"] when method == "POST":
                return await domain.CreateParticipantSupplierAsync(await Body<CreateParticipantSupplierCommand>(request), cancellationToken);

            case ["stakeholders"] when method == "GET":
                return await domain.ListStakeholdersAsync(Required(query, "authorityId"), cancellationToken);
            case ["stakeholders"] when method == "POST":
                return await domain.CreateStakeholderAsync(await Body<CreateEntityCommand>(request), cancellationToken);
            case ["stakeholders", var stakeholderId, "users"] when method == "GET":
                return await domain.ListMembershipUsersAsync("stakeholder", stakeholderId, cancellationToken);
            case ["stakeholders", var stakeholderId, "users"] when method == "POST":
                return await domain.CreateMembershipUserAsync("stakeholder", stakeholderId, await Body<EntityUserCommand>(request), cancellationToken);
            case ["stakeholders", var stakeholderId, "access"] when method == "GET":
                return await domain.ListStakeholderAccessAsync(stakeholderId, cancellationToken);
            case ["stakeholder-access"] when method == "POST":
                return await domain.GrantStakeholderAccessAsync(await Body<GrantStakeholderAccessCommand>(request), cancellationToken);

            case ["agents"] when method == "GET":
                return await domain.ListAgentsAsync(Required(query, "authorityId"), cancellationToken);
            case ["agents"] when method == "POST":
                return await domain.CreateAgentAsync(await Body<CreateEntityCommand>(request), cancellationToken);
            case ["agents", var agentId, "users"] when method == "GET":
                return await domain.ListMembershipUsersAsync("agent", agentId, cancellationToken);
            case ["agents", var agentId, "users"] when method == "POST":
                return await domain.CreateMembershipUserAsync("agent", agentId, await Body<EntityUserCommand>(request), cancellationToken);

            case ["users"] when method == "GET":
                return await domain.ListUserAccountsAsync(cancellationToken);
            case ["users", var userAccountId, "email"] when method == "PATCH":
                return await domain.UpdateUserEmailAsync(userAccountId, await Body<UpdateUserEmailCommand>(request), cancellationToken);
            case ["users", var userAccountId, "entra"] when method == "PATCH":
                return await domain.RegisterUserAccountWithEntraAsync(userAccountId, await Body<RegisterUserAccountWithEntraCommand>(request), cancellationToken);

            case ["task-types"] when method == "GET":
                return await domain.ListTaskTypesAsync(cancellationToken);
            case ["case-templates"] when method == "GET":
                return await domain.ListCaseTemplatesAsync(Required(query, "authorityId"), cancellationToken);
            case ["case-templates"] when method == "POST":
                return await domain.CreateCaseTemplateAsync(await Body<CreateCaseTemplateCommand>(request), cancellationToken);
            case ["case-templates", var caseTemplateId] when method == "DELETE":
                await domain.DeleteCaseTemplateAsync(caseTemplateId, cancellationToken);
                return new { status = "ok" };
            case ["case-templates", var caseTemplateId, "finalize"] when method == "POST":
                return await domain.FinalizeCaseTemplateAsync(caseTemplateId, cancellationToken);
            case ["case-templates", var caseTemplateId, "tasks"] when method == "GET":
                return (await domain.ListTemplateTasksAsync(caseTemplateId, cancellationToken)).Select(DomainDto.TemplateTask);
            case ["case-templates", var caseTemplateId, "tasks"] when method == "POST":
                return DomainDto.TemplateTask(await domain.AddTaskToTemplateAsync(caseTemplateId, await Body<AddTaskToTemplateCommand>(request), cancellationToken));
            case ["case-templates", var caseTemplateId, "participants"] when method == "GET":
                return await domain.ListCaseTemplateParticipantsAsync(caseTemplateId, cancellationToken);
            case ["case-templates", var caseTemplateId, "participants"] when method == "POST":
                return await domain.AssignParticipantToTemplateAsync(caseTemplateId, await Body<AssignParticipantToTemplateCommand>(request), cancellationToken);
            case ["template-tasks", var templateTaskId, "withdraw"] when method == "POST":
                return DomainDto.TemplateTask(await domain.WithdrawTemplateTaskAsync(templateTaskId, await Body<WithdrawTemplateTaskCommand>(request), cancellationToken));

            case ["cases"] when method == "GET":
                return await domain.ListCasesAsync(Value(query, "participantId"), Value(query, "authorityId"), cancellationToken);
            case ["cases", var caseId, "submit"] when method == "POST":
                return await domain.SubmitCaseAsync(caseId, cancellationToken);
            case ["cases", var caseId, "withdraw"] when method == "POST":
                return await domain.WithdrawCaseAsync(caseId, await Body<WithdrawCaseCommand>(request), cancellationToken);
            case ["cases", var caseId, "reinstate"] when method == "POST":
                return await domain.ReinstateCaseAsync(caseId, cancellationToken);
            case ["cases", var caseId, "participant-supplier"] when method == "PUT":
                return await domain.LinkParticipantSupplierToCaseAsync(caseId, await Body<LinkParticipantSupplierCommand>(request), cancellationToken);
            case ["cases", var caseId, "participant-supplier"] when method == "DELETE":
                return await domain.UnlinkParticipantSupplierFromCaseAsync(caseId, cancellationToken);
            case ["cases", var caseId, "tasks"] when method == "GET":
                return (await domain.ListTasksAsync(caseId, cancellationToken)).Select(DomainDto.Task);
            case ["cases", var caseId, "stakeholder-reviews"] when method == "GET":
                return await domain.ListStakeholderReviewsAsync(caseId, cancellationToken);

            case ["tasks", var taskId, "complete"] when method == "POST":
                return DomainDto.Task(await domain.CompleteTaskAsync(taskId, await Body<CompleteTaskCommand>(request), cancellationToken));
            case ["tasks", var taskId, "evidence"] when method == "POST":
                return DomainDto.Task(await domain.UploadEvidenceAsync(taskId, await Body<UploadEvidenceCommand>(request), cancellationToken));
            case ["tasks", var taskId, "submit"] when method == "POST":
                return DomainDto.Task(await domain.SubmitTaskAsync(taskId, cancellationToken));
            case ["tasks", var taskId, "review"] when method == "POST":
                return DomainDto.Task(await domain.ReviewTaskAsync(taskId, await Body<ReviewTaskCommand>(request), cancellationToken));

            case ["access-grants"] when method == "GET":
                return await domain.ListAccessGrantsAsync(Value(query, "participantId"), Value(query, "stakeholderId"), Value(query, "agentId"), cancellationToken);
            case ["access-grants"] when method == "POST":
                return await domain.CreateAccessGrantAsync(await Body<CreateAccessGrantCommand>(request), cancellationToken);
            case ["access-grants", var accessGrantId, "status"] when method == "PATCH":
                return await domain.UpdateAccessGrantStatusAsync(accessGrantId, await Body<UpdateAccessGrantStatusCommand>(request), cancellationToken);

            case ["stakeholder-reviews"] when method == "POST":
                return await domain.UpsertStakeholderReviewAsync(await Body<UpsertStakeholderReviewCommand>(request), cancellationToken);
            case ["requests-for-information"] when method == "GET":
                return (await domain.ListRequestsForInformationAsync(Value(query, "caseId"), Value(query, "participantId"), cancellationToken)).Select(DomainDto.RequestForInformation);
            case ["requests-for-information"] when method == "POST":
                return DomainDto.RequestForInformation(await domain.CreateRequestForInformationAsync(await Body<CreateRequestForInformationCommand>(request), cancellationToken));
            case ["requests-for-information", var requestId, "response"] when method == "POST":
                return DomainDto.RequestForInformation(await domain.RespondToRequestForInformationAsync(requestId, await Body<RespondToRequestForInformationCommand>(request), cancellationToken));
            case ["requests-for-information", var requestId, "status"] when method == "PATCH":
                return DomainDto.RequestForInformation(await domain.UpdateRequestForInformationStatusAsync(requestId, await Body<UpdateRequestForInformationStatusCommand>(request), cancellationToken));
            default:
                return null;
        }
    }

    private async Task<HttpResponseData> DemoSignInOptions(
        HttpRequestData request,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var options = services.GetRequiredService<IOptions<DemoSignInOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.Key) ||
            !string.Equals(Header(request, "X-Demo-SignIn-Key"), options.Key, StringComparison.Ordinal))
        {
            return Problem(request, HttpStatusCode.Unauthorized, "Demo sign-in is not available.");
        }

        var db = services.GetRequiredService<AllChecksOutDbContext>();
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

        return await Json(request, new DemoSignInOptionsDto(authorities, participants, stakeholders, agents, users, memberships));
    }

    private async Task<T> Body<T>(HttpRequestData request)
    {
        var body = await JsonSerializer.DeserializeAsync<T>(request.Body, JsonOptions);
        return body ?? throw new JsonException("Request body is required.");
    }

    private async Task<HttpResponseData> Json(HttpRequestData request, object value, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = request.CreateResponse(statusCode);
        ApplyCors(response, request);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await JsonSerializer.SerializeAsync(response.Body, value, JsonOptions);
        return response;
    }

    private HttpResponseData Problem(HttpRequestData request, HttpStatusCode statusCode, string message) =>
        Json(request, new { title = message, status = (int)statusCode }, statusCode).GetAwaiter().GetResult();

    private HttpResponseData Empty(HttpRequestData request, HttpStatusCode statusCode)
    {
        var response = request.CreateResponse(statusCode);
        ApplyCors(response, request);
        return response;
    }

    private void ApplyCors(HttpResponseData response, HttpRequestData request)
    {
        var origin = Header(request, "Origin");
        var allowedOrigins = corsOptions.Value.GetAllowedOrigins();
        if (!string.IsNullOrWhiteSpace(origin) && allowedOrigins.Contains(origin.TrimEnd('/'), StringComparer.OrdinalIgnoreCase))
        {
            response.Headers.Add("Access-Control-Allow-Origin", origin);
            response.Headers.Add("Vary", "Origin");
        }

        response.Headers.Add("Access-Control-Allow-Methods", "DELETE,GET,OPTIONS,PATCH,POST,PUT");
        response.Headers.Add("Access-Control-Allow-Headers", "Authorization,Content-Type,X-Demo-SignIn-Key");
    }

    private static bool IsOptions(HttpRequestData request) =>
        string.Equals(request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string? routePath, string absolutePath) =>
        Uri.UnescapeDataString(string.IsNullOrWhiteSpace(routePath) ? absolutePath.Trim('/') : routePath.Trim('/'));

    private static bool Matches(string[] actual, params string[] expected) =>
        actual.Length == expected.Length && actual.Zip(expected).All(pair =>
            string.Equals(pair.First, pair.Second, StringComparison.OrdinalIgnoreCase));

    private static string? Header(HttpRequestData request, string name) =>
        request.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    private static string? Value(IReadOnlyDictionary<string, string?> query, string name) =>
        query.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static string Required(IReadOnlyDictionary<string, string?> query, string name) =>
        Value(query, name) ?? throw new JsonException($"Query parameter '{name}' is required.");

    private static IReadOnlyDictionary<string, string?> Query(Uri uri)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var query = uri.Query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(query))
        {
            return values;
        }

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0].Replace("+", " "));
            var value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : null;
            values[key] = value;
        }

        return values;
    }
}
