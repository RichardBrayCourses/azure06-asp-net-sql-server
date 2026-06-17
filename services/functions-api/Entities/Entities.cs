namespace AllChecksOut.FunctionsApi.Entities;

public abstract class Entity
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class SystemOwner : Entity
{
    public required string Name { get; set; }
}

public class Authority : Entity
{
    public required string SystemOwnerId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}

public class AuthorityTerminology : Entity
{
    public required string AuthorityId { get; set; }
    public required string LabelsJson { get; set; }
}

public class Participant : Entity
{
    public required string AuthorityId { get; set; }
    public required string DisplayName { get; set; }
}

public class Stakeholder : Entity
{
    public required string AuthorityId { get; set; }
    public required string DisplayName { get; set; }
}

public class Agent : Entity
{
    public required string AuthorityId { get; set; }
    public required string DisplayName { get; set; }
}

public class UserAccount : Entity
{
    public required string EntraObjectId { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string Status { get; set; }
}

public abstract class Membership : Entity
{
    public string EntityId { get; set; } = string.Empty;
    public string UserAccountId { get; set; } = string.Empty;
}

public class SystemOwnerUser : Membership;
public class AuthorityUser : Membership;
public class ParticipantUser : Membership;
public class StakeholderUser : Membership;
public class AgentUser : Membership;

public class StakeholderParticipantAccess : Entity
{
    public required string StakeholderId { get; set; }
    public required string ParticipantId { get; set; }
    public required string ApprovedByUserId { get; set; }
    public DateTimeOffset ApprovedAt { get; set; }
}

public class AccessGrant : Entity
{
    public required string AuthorityId { get; set; }
    public required string ParticipantId { get; set; }
    public required string GranteeType { get; set; }
    public string? GranteeStakeholderId { get; set; }
    public string? GranteeAgentId { get; set; }
    public string? GranteeUserId { get; set; }
    public required string PermissionLevel { get; set; }
    public required string DataScopeType { get; set; }
    public string? DataScopeId { get; set; }
    public required string Status { get; set; }
    public required string CreatedByUserId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public class TaskType : Entity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string ParameterSchemaJson { get; set; }
    public required string Status { get; set; }
}

public class CaseTemplate : Entity
{
    public required string AuthorityId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
}

public class TemplateTask : Entity
{
    public required string CaseTemplateId { get; set; }
    public required string TaskTypeId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string ParametersJson { get; set; }
    public int SortOrder { get; set; }
    public required string Status { get; set; }
    public string? WithdrawnReason { get; set; }
    public DateTimeOffset? WithdrawnAt { get; set; }
    public string? WithdrawnByUserId { get; set; }
}

public class CaseTemplateParticipant : Entity
{
    public required string CaseTemplateId { get; set; }
    public required string ParticipantId { get; set; }
    public string? CaseId { get; set; }
    public string? ExemptionReason { get; set; }
    public string? DecidedByUserId { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
}

public class Case : Entity
{
    public required string AuthorityId { get; set; }
    public required string CaseTemplateId { get; set; }
    public required string ParticipantId { get; set; }
    public string? ParticipantSupplierId { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset? WithdrawnAt { get; set; }
    public string? WithdrawnByUserId { get; set; }
    public string? WithdrawnReason { get; set; }
}

public class Task : Entity
{
    public required string CaseId { get; set; }
    public required string TemplateTaskId { get; set; }
    public required string Status { get; set; }
    public required string ResponseJson { get; set; }
    public required string EvidenceJson { get; set; }
    public DateTimeOffset? WithdrawnAt { get; set; }
}

public class StakeholderReview : Entity
{
    public required string StakeholderId { get; set; }
    public required string CaseId { get; set; }
    public required string Note { get; set; }
    public required string ReviewedByUserId { get; set; }
    public DateTimeOffset ReviewedAt { get; set; }
}

public class RequestForInformation : Entity
{
    public required string AuthorityId { get; set; }
    public required string ParticipantId { get; set; }
    public required string StakeholderId { get; set; }
    public string? CaseId { get; set; }
    public string? TaskId { get; set; }
    public required string RequestText { get; set; }
    public required string ResponseText { get; set; }
    public required string Status { get; set; }
    public required string RequestedByUserId { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? RespondedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
    public required string StatusHistoryJson { get; set; }
}

public class ParticipantSupplier : Entity
{
    public required string AuthorityId { get; set; }
    public required string ParticipantId { get; set; }
    public required string SupplierName { get; set; }
    public required string RelationshipType { get; set; }
    public required string ServicesProvided { get; set; }
    public required string DataExposure { get; set; }
}
