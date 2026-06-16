using System.Text.Json;
using AllChecksOut.Cases.Api.Entities;

namespace AllChecksOut.Cases.Api.Domain;

public sealed record EntityUserCommand(string DisplayName, string Email);
public sealed record UpdateUserEmailCommand(string Email);
public sealed record RegisterUserAccountWithEntraCommand(string EntraObjectId);
public sealed record UpdateAuthorityTerminologyCommand(JsonElement Labels);
public sealed record CreateParticipantCommand(string AuthorityId, string DisplayName, EntityUserCommand? InitialUser);
public sealed record CreateEntityCommand(string AuthorityId, string DisplayName);
public sealed record GrantStakeholderAccessCommand(string StakeholderId, string ParticipantId, string ApprovedByUserId);
public sealed record CreateAccessGrantCommand(
    string AuthorityId,
    string ParticipantId,
    string GranteeType,
    string? GranteeStakeholderId,
    string? GranteeAgentId,
    string? GranteeUserId,
    string PermissionLevel,
    string? DataScopeType,
    string? DataScopeId,
    string? Status,
    string CreatedByUserId,
    DateTimeOffset? ExpiresAt);
public sealed record UpdateAccessGrantStatusCommand(string Status);
public sealed record CreateParticipantSupplierCommand(
    string AuthorityId,
    string ParticipantId,
    string SupplierName,
    string RelationshipType,
    string ServicesProvided,
    string DataExposure);
public sealed record LinkParticipantSupplierCommand(string ParticipantSupplierId);
public sealed record UpsertStakeholderReviewCommand(string StakeholderId, string CaseId, string Note, string ReviewedByUserId);
public sealed record CreateRequestForInformationCommand(string StakeholderId, string CaseId, string? TaskId, string RequestText, string RequestedByUserId);
public sealed record RespondToRequestForInformationCommand(string ResponseText, string RespondedByUserId, string? Status);
public sealed record UpdateRequestForInformationStatusCommand(string Status, string UpdatedByUserId, string? Note);
public sealed record CreateCaseTemplateCommand(string AuthorityId, string Name, string Description);
public sealed record AddTaskToTemplateCommand(string TaskTypeId, string Title, string Description, JsonElement? ParametersJson);
public sealed record AssignParticipantToTemplateCommand(string ParticipantId, string? ExemptionReason, string? DecidedByUserId);
public sealed record CompleteTaskCommand(JsonElement ResponseJson);
public sealed record UploadEvidenceCommand(JsonElement EvidenceJson);
public sealed record ReviewTaskCommand(string Decision);
public sealed record WithdrawTemplateTaskCommand(string WithdrawnByUserId, string WithdrawnReason);
public sealed record WithdrawCaseCommand(string WithdrawnByUserId, string WithdrawnReason);

public sealed record UserMembershipDto(Membership Membership, UserAccount UserAccount);

public static class DomainDto
{
    public static object AuthorityTerminology(AuthorityTerminology entity) => new
    {
        entity.Id,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.AuthorityId,
        Labels = Json(entity.LabelsJson),
    };

    public static object TaskType(TaskType entity) => new
    {
        entity.Id,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.Code,
        entity.Name,
        entity.Description,
        ParameterSchemaJson = Json(entity.ParameterSchemaJson),
        entity.Status,
    };

    public static object TemplateTask(TemplateTask entity) => new
    {
        entity.Id,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.CaseTemplateId,
        entity.TaskTypeId,
        entity.Title,
        entity.Description,
        ParametersJson = Json(entity.ParametersJson),
        entity.SortOrder,
        entity.Status,
        entity.WithdrawnReason,
        entity.WithdrawnAt,
        entity.WithdrawnByUserId,
    };

    public static object Task(Entities.Task entity) => new
    {
        entity.Id,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.CaseId,
        entity.TemplateTaskId,
        entity.Status,
        ResponseJson = Json(entity.ResponseJson),
        EvidenceJson = Json(entity.EvidenceJson),
        entity.WithdrawnAt,
    };

    public static object RequestForInformation(RequestForInformation entity) => new
    {
        entity.Id,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.AuthorityId,
        entity.ParticipantId,
        entity.StakeholderId,
        entity.CaseId,
        entity.TaskId,
        entity.RequestText,
        entity.ResponseText,
        entity.Status,
        entity.RequestedByUserId,
        entity.AssignedToUserId,
        entity.RespondedByUserId,
        entity.RequestedAt,
        entity.RespondedAt,
        StatusHistory = Json(entity.StatusHistoryJson),
    };

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.Clone();
    }

    public static string JsonString(JsonElement? json, string fallback = "{}") =>
        json is null ? fallback : json.Value.GetRawText();
}
