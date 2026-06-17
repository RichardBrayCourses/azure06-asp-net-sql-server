import {
  AccessGrantEntity,
  AgentEntity,
  AgentId,
  AuthorityEntity,
  AuthorityId,
  AuthorityTerminologyEntity,
  CaseEntity,
  CaseRecordId,
  CaseTemplateEntity,
  CaseTemplateId,
  CaseTemplateParticipantEntity,
  AddTaskToTemplateCommand,
  CreateAccessGrantCommand,
  CreateAgentCommand,
  CreateCaseTemplateCommand,
  CreateEntityUserCommand,
  CreateParticipantCommand,
  CreateParticipantSupplierCommand,
  CreateRequestForInformationCommand,
  CreateStakeholderCommand,
  InMemoryAllChecksOutDatabase,
  ParticipantEntity,
  ParticipantId,
  ParticipantSupplierEntity,
  ParticipantSupplierId,
  RequestForInformationEntity,
  RespondToRequestForInformationCommand,
  StakeholderEntity,
  StakeholderId,
  StakeholderParticipantAccessEntity,
  StakeholderReviewEntity,
  TaskEntity,
  TaskId,
  TaskTypeEntity,
  TemplateTaskEntity,
  TemplateTaskId,
  UpdateAuthorityTerminologyCommand,
  UpdateRequestForInformationStatusCommand,
  UploadEvidenceCommand,
  CompleteTaskCommand,
  AssignParticipantToTemplateCommand,
  GrantStakeholderAccessCommand,
  UpsertStakeholderReviewCommand,
  AccessGrantId,
  AccessGrantStatus,
  UserAccountEntity,
  UserAccountId,
  AuthorityUserEntity,
  ParticipantUserEntity,
  StakeholderUserEntity,
  AgentUserEntity,
  type AccessGrantDto,
  type AgentDto,
  type AuthorityDto,
  type AuthorityTerminologyDto,
  type CaseDto,
  type CaseTemplateDto,
  type CaseTemplateParticipantDto,
  type MembershipDto,
  type ParticipantDto,
  type ParticipantSupplierDto,
  type RequestForInformationDto,
  type StakeholderDto,
  type StakeholderParticipantAccessDto,
  type StakeholderReviewDto,
  type TaskDto,
  type TaskTypeDto,
  type TemplateTaskDto,
  type UserAccountDto,
} from "./console";
import { apiRequest, ApiError } from "@/lib/api/client";

type MembershipUserDto = {
  membership: MembershipDto;
  userAccount: UserAccountDto;
};

type CurrentUserProfileDto = {
  applicationUser: UserAccountDto | null;
  memberships: Array<{
    type: string;
    entityId: string;
    authorityId: string | null;
  }>;
};

function query(params: Record<string, string | null | undefined>) {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value) search.set(key, value);
  });
  const value = search.toString();
  return value ? `?${value}` : "";
}

export class ApiBackedAllChecksOutDatabase extends InMemoryAllChecksOutDatabase {
  private isHydrating = false;

  async hydrateFromApi() {
    if (this.isHydrating) return;
    this.isHydrating = true;
    try {
      const profile = await apiRequest<CurrentUserProfileDto>("/api/me");
      if (!profile.applicationUser) return;

      const authorityIds = unique([
        ...profile.memberships.map((membership) => membership.authorityId),
        ...profile.memberships
          .filter((membership) => membership.type === "authority")
          .map((membership) => membership.entityId),
      ]);
      const participantIds = unique(profile.memberships.filter((membership) => membership.type === "participant").map((membership) => membership.entityId));
      const stakeholderIds = unique(profile.memberships.filter((membership) => membership.type === "stakeholder").map((membership) => membership.entityId));
      const agentIds = unique(profile.memberships.filter((membership) => membership.type === "agent").map((membership) => membership.entityId));

      await Promise.all([
        this.replaceEntities(this.authorities, AuthorityEntity, apiRequest<AuthorityDto[]>("/api/authorities")),
        this.replaceEntities(this.taskTypes, TaskTypeEntity, apiRequest<TaskTypeDto[]>("/api/task-types")),
        this.replaceEntities(this.participants, ParticipantEntity, apiRequest<ParticipantDto[]>("/api/participants")),
        this.replaceEntities(this.cases, CaseEntity, apiRequest<CaseDto[]>("/api/cases")),
      ]);
      await this.ignoreForbidden(this.replaceEntities(this.userAccounts, UserAccountEntity, apiRequest<UserAccountDto[]>("/api/users")));

      await Promise.all(authorityIds.map((authorityId) => this.hydrateAuthority(authorityId)));
      await Promise.all([...this.participants.map((participant) => participant.id), ...participantIds].map((participantId) => this.hydrateParticipant(participantId)));
      await Promise.all(stakeholderIds.map((stakeholderId) => this.hydrateStakeholder(stakeholderId)));
      await Promise.all(agentIds.map((agentId) => this.hydrateAgent(agentId)));
      await Promise.all(this.cases.map((caseRecord) => this.hydrateCase(caseRecord.id)));
    } finally {
      this.isHydrating = false;
    }
  }

  async updateUserAccountEmail(userAccountId: UserAccountId, email: string) {
    const account = await apiRequest<UserAccountDto>(`/api/users/${userAccountId}/email`, {
      method: "PATCH",
      body: { email },
    });
    this.upsertEntity(this.userAccounts, UserAccountEntity, account);
    return account;
  }

  async registerUserAccountWithEntra(userAccountId: UserAccountId, entraObjectId: string) {
    const account = await apiRequest<UserAccountDto>(`/api/users/${userAccountId}/entra`, {
      method: "PATCH",
      body: { entraObjectId },
    });
    this.upsertEntity(this.userAccounts, UserAccountEntity, account);
    return account;
  }

  async updateAuthorityTerminology(command: UpdateAuthorityTerminologyCommand) {
    const terminology = await apiRequest<AuthorityTerminologyDto>(`/api/authorities/${command.authorityId}/terminology`, {
      method: "PUT",
      body: { labels: command.labels },
    });
    this.upsertEntity(this.authorityTerminology, AuthorityTerminologyEntity, terminology);
    return terminology;
  }

  async createParticipant(command: CreateParticipantCommand) {
    const participant = await apiRequest<ParticipantDto>("/api/participants", { method: "POST", body: command });
    await this.hydrateFromApi();
    return participant;
  }

  async createParticipantUser(participantId: ParticipantId, command: CreateEntityUserCommand) {
    const result = await apiRequest<MembershipUserDto>(`/api/participants/${participantId}/users`, { method: "POST", body: command });
    await this.hydrateFromApi();
    return { userAccount: result.userAccount, participantUser: result.membership };
  }

  async createAuthorityUser(authorityId: AuthorityId, command: CreateEntityUserCommand) {
    const result = await apiRequest<MembershipUserDto>(`/api/authorities/${authorityId}/users`, { method: "POST", body: command });
    await this.hydrateFromApi();
    return { userAccount: result.userAccount, authorityUser: result.membership };
  }

  async createStakeholder(command: CreateStakeholderCommand) {
    const stakeholder = await apiRequest<StakeholderDto>("/api/stakeholders", { method: "POST", body: command });
    await this.hydrateFromApi();
    return stakeholder;
  }

  async createStakeholderUser(stakeholderId: StakeholderId, command: CreateEntityUserCommand) {
    const result = await apiRequest<MembershipUserDto>(`/api/stakeholders/${stakeholderId}/users`, { method: "POST", body: command });
    await this.hydrateFromApi();
    return { userAccount: result.userAccount, stakeholderUser: result.membership };
  }

  async createAgent(command: CreateAgentCommand) {
    const agent = await apiRequest<AgentDto>("/api/agents", { method: "POST", body: command });
    await this.hydrateFromApi();
    return agent;
  }

  async createAgentUser(agentId: AgentId, command: CreateEntityUserCommand) {
    const result = await apiRequest<MembershipUserDto>(`/api/agents/${agentId}/users`, { method: "POST", body: command });
    await this.hydrateFromApi();
    return { userAccount: result.userAccount, agentUser: result.membership };
  }

  async grantStakeholderAccess(command: GrantStakeholderAccessCommand) {
    const access = await apiRequest<StakeholderParticipantAccessDto>("/api/stakeholder-access", { method: "POST", body: command });
    await this.hydrateFromApi();
    return access;
  }

  async createAccessGrant(command: CreateAccessGrantCommand) {
    const grant = await apiRequest<AccessGrantDto>("/api/access-grants", { method: "POST", body: command });
    await this.hydrateFromApi();
    return grant;
  }

  async updateAccessGrantStatus(accessGrantId: AccessGrantId, status: AccessGrantStatus) {
    const grant = await apiRequest<AccessGrantDto>(`/api/access-grants/${accessGrantId}/status`, {
      method: "PATCH",
      body: { status },
    });
    await this.hydrateFromApi();
    return grant;
  }

  async createParticipantSupplier(command: CreateParticipantSupplierCommand) {
    const supplier = await apiRequest<ParticipantSupplierDto>("/api/participant-suppliers", { method: "POST", body: command });
    await this.hydrateFromApi();
    return supplier;
  }

  async linkParticipantSupplierToCase(participantSupplierId: ParticipantSupplierId, caseId: CaseRecordId) {
    const caseRecord = await apiRequest<CaseDto>(`/api/cases/${caseId}/participant-supplier`, {
      method: "PUT",
      body: { participantSupplierId },
    });
    await this.hydrateFromApi();
    return caseRecord;
  }

  async unlinkParticipantSupplierFromCase(caseId: CaseRecordId) {
    const caseRecord = await apiRequest<CaseDto>(`/api/cases/${caseId}/participant-supplier`, { method: "DELETE" });
    await this.hydrateFromApi();
    return caseRecord;
  }

  async upsertStakeholderReview(command: UpsertStakeholderReviewCommand) {
    const review = await apiRequest<StakeholderReviewDto>("/api/stakeholder-reviews", { method: "POST", body: command });
    await this.hydrateFromApi();
    return review;
  }

  async createRequestForInformation(command: CreateRequestForInformationCommand) {
    const request = await apiRequest<RequestForInformationDto>("/api/requests-for-information", { method: "POST", body: command });
    await this.hydrateFromApi();
    return request;
  }

  async respondToRequestForInformation(command: RespondToRequestForInformationCommand) {
    const request = await apiRequest<RequestForInformationDto>(`/api/requests-for-information/${command.requestId}/response`, {
      method: "POST",
      body: {
        responseText: command.responseText,
        respondedByUserId: command.respondedByUserId,
        status: command.status,
      },
    });
    await this.hydrateFromApi();
    return request;
  }

  async updateRequestForInformationStatus(command: UpdateRequestForInformationStatusCommand) {
    const request = await apiRequest<RequestForInformationDto>(`/api/requests-for-information/${command.requestId}/status`, {
      method: "PATCH",
      body: {
        status: command.status,
        updatedByUserId: command.updatedByUserId,
        note: command.note,
      },
    });
    await this.hydrateFromApi();
    return request;
  }

  async createCaseTemplate(command: CreateCaseTemplateCommand) {
    const template = await apiRequest<CaseTemplateDto>("/api/case-templates", { method: "POST", body: command });
    await this.hydrateFromApi();
    return template;
  }

  async addTaskToTemplate(command: AddTaskToTemplateCommand) {
    const task = await apiRequest<TemplateTaskDto>(`/api/case-templates/${command.caseTemplateId}/tasks`, {
      method: "POST",
      body: {
        taskTypeId: command.taskTypeId,
        title: command.title,
        description: command.description,
        parametersJson: command.parametersJson,
      },
    });
    await this.hydrateFromApi();
    return task;
  }

  async finalizeCaseTemplate(caseTemplateId: CaseTemplateId) {
    const template = await apiRequest<CaseTemplateDto>(`/api/case-templates/${caseTemplateId}/finalize`, { method: "POST" });
    await this.hydrateFromApi();
    return template;
  }

  async assignParticipantToTemplate(command: AssignParticipantToTemplateCommand) {
    const assignment = await apiRequest<CaseTemplateParticipantDto>(`/api/case-templates/${command.caseTemplateId}/participants`, {
      method: "POST",
      body: {
        participantId: command.participantId,
        exemptionReason: command.exemptionReason,
        decidedByUserId: command.decidedByUserId,
      },
    });
    await this.hydrateFromApi();
    return assignment;
  }

  async completeTask(command: CompleteTaskCommand) {
    const task = await apiRequest<TaskDto>(`/api/tasks/${command.taskId}/complete`, {
      method: "POST",
      body: { responseJson: command.responseJson },
    });
    await this.hydrateFromApi();
    return task;
  }

  async uploadEvidence(command: UploadEvidenceCommand) {
    const task = await apiRequest<TaskDto>(`/api/tasks/${command.taskId}/evidence`, {
      method: "POST",
      body: { evidenceJson: command.evidenceJson },
    });
    await this.hydrateFromApi();
    return task;
  }

  async submitTask(taskId: TaskId) {
    const task = await apiRequest<TaskDto>(`/api/tasks/${taskId}/submit`, { method: "POST" });
    await this.hydrateFromApi();
    return task;
  }

  async submitCase(caseId: CaseRecordId) {
    const caseRecord = await apiRequest<CaseDto>(`/api/cases/${caseId}/submit`, { method: "POST" });
    await this.hydrateFromApi();
    return caseRecord;
  }

  async reviewTask(taskId: TaskId, decision: "PASSED" | "FAILED") {
    const task = await apiRequest<TaskDto>(`/api/tasks/${taskId}/review`, { method: "POST", body: { decision } });
    await this.hydrateFromApi();
    return task;
  }

  async withdrawTemplateTask(templateTaskId: TemplateTaskId, withdrawnByUserId: UserAccountId, withdrawnReason: string) {
    const task = await apiRequest<TemplateTaskDto>(`/api/template-tasks/${templateTaskId}/withdraw`, {
      method: "POST",
      body: { withdrawnByUserId, withdrawnReason },
    });
    await this.hydrateFromApi();
    return task;
  }

  async withdrawCase(caseId: CaseRecordId, withdrawnByUserId: UserAccountId, withdrawnReason: string) {
    const caseRecord = await apiRequest<CaseDto>(`/api/cases/${caseId}/withdraw`, {
      method: "POST",
      body: { withdrawnByUserId, withdrawnReason },
    });
    await this.hydrateFromApi();
    return caseRecord;
  }

  async reinstateCase(caseId: CaseRecordId) {
    const caseRecord = await apiRequest<CaseDto>(`/api/cases/${caseId}/reinstate`, { method: "POST" });
    await this.hydrateFromApi();
    return caseRecord;
  }

  async deleteCaseTemplate(caseTemplateId: CaseTemplateId) {
    const template = await apiRequest<CaseTemplateDto>(`/api/case-templates/${caseTemplateId}`, { method: "DELETE" });
    await this.hydrateFromApi();
    return template;
  }

  private async hydrateAuthority(authorityId: AuthorityId) {
    await this.ignoreForbidden(this.replaceEntities(this.authorityTerminology, AuthorityTerminologyEntity, apiRequest<AuthorityTerminologyDto>(`/api/authorities/${authorityId}/terminology`).then((item) => [item])));
    await this.ignoreForbidden(this.replaceEntities(this.stakeholders, StakeholderEntity, apiRequest<StakeholderDto[]>(`/api/stakeholders${query({ authorityId })}`)));
    await this.ignoreForbidden(this.replaceEntities(this.agents, AgentEntity, apiRequest<AgentDto[]>(`/api/agents${query({ authorityId })}`)));
    await this.ignoreForbidden(this.replaceEntities(this.caseTemplates, CaseTemplateEntity, apiRequest<CaseTemplateDto[]>(`/api/case-templates${query({ authorityId })}`)));
    await this.ignoreForbidden(this.replaceMembershipUsers("authority", authorityId, apiRequest<MembershipUserDto[]>(`/api/authorities/${authorityId}/users`)));
    await Promise.all(this.caseTemplates.filter((template) => template.toDto().authorityId === authorityId).map((template) => this.hydrateTemplate(template.id)));
  }

  private async hydrateParticipant(participantId: ParticipantId) {
    await this.ignoreForbidden(this.replaceEntities(this.participantSuppliers, ParticipantSupplierEntity, apiRequest<ParticipantSupplierDto[]>(`/api/participants/${participantId}/suppliers`)));
    await this.ignoreForbidden(this.replaceEntities(this.accessGrants, AccessGrantEntity, apiRequest<AccessGrantDto[]>(`/api/access-grants${query({ participantId })}`)));
    await this.ignoreForbidden(this.replaceMembershipUsers("participant", participantId, apiRequest<MembershipUserDto[]>(`/api/participants/${participantId}/users`)));
    await this.ignoreForbidden(this.replaceEntities(this.requestsForInformation, RequestForInformationEntity, apiRequest<RequestForInformationDto[]>(`/api/requests-for-information${query({ participantId })}`)));
  }

  private async hydrateStakeholder(stakeholderId: StakeholderId) {
    await this.ignoreForbidden(this.replaceEntities(this.stakeholderParticipantAccess, StakeholderParticipantAccessEntity, apiRequest<StakeholderParticipantAccessDto[]>(`/api/stakeholders/${stakeholderId}/access`)));
    await this.ignoreForbidden(this.replaceEntities(this.accessGrants, AccessGrantEntity, apiRequest<AccessGrantDto[]>(`/api/access-grants${query({ stakeholderId })}`)));
    await this.ignoreForbidden(this.replaceMembershipUsers("stakeholder", stakeholderId, apiRequest<MembershipUserDto[]>(`/api/stakeholders/${stakeholderId}/users`)));
  }

  private async hydrateAgent(agentId: AgentId) {
    await this.ignoreForbidden(this.replaceEntities(this.accessGrants, AccessGrantEntity, apiRequest<AccessGrantDto[]>(`/api/access-grants${query({ agentId })}`)));
    await this.ignoreForbidden(this.replaceMembershipUsers("agent", agentId, apiRequest<MembershipUserDto[]>(`/api/agents/${agentId}/users`)));
  }

  private async hydrateTemplate(caseTemplateId: CaseTemplateId) {
    await this.ignoreForbidden(this.replaceEntities(this.templateTasks, TemplateTaskEntity, apiRequest<TemplateTaskDto[]>(`/api/case-templates/${caseTemplateId}/tasks`)));
    await this.ignoreForbidden(this.replaceEntities(this.caseTemplateParticipants, CaseTemplateParticipantEntity, apiRequest<CaseTemplateParticipantDto[]>(`/api/case-templates/${caseTemplateId}/participants`)));
  }

  private async hydrateCase(caseId: CaseRecordId) {
    await this.ignoreForbidden(this.replaceEntities(this.tasks, TaskEntity, apiRequest<TaskDto[]>(`/api/cases/${caseId}/tasks`)));
    await this.ignoreForbidden(this.replaceEntities(this.stakeholderReviews, StakeholderReviewEntity, apiRequest<StakeholderReviewDto[]>(`/api/cases/${caseId}/stakeholder-reviews`)));
    await this.ignoreForbidden(this.replaceEntities(this.requestsForInformation, RequestForInformationEntity, apiRequest<RequestForInformationDto[]>(`/api/requests-for-information${query({ caseId })}`)));
  }

  private async replaceEntities<TDto extends { id: string }, TEntity>(
    target: TEntity[],
    Entity: new (dto: TDto) => TEntity,
    source: Promise<TDto[]>,
  ) {
    const items = await source;
    items.forEach((item) => this.upsertEntity(target, Entity, item));
  }

  private upsertEntity<TDto extends { id: string }, TEntity>(
    target: TEntity[],
    Entity: new (dto: TDto) => TEntity,
    item: TDto,
  ) {
    const existingIndex = target.findIndex((candidate) => entityId(candidate) === item.id);
    const next = new Entity(item);
    if (existingIndex >= 0) {
      target.splice(existingIndex, 1, next);
    } else {
      target.push(next);
    }
  }

  private async replaceMembershipUsers(entityType: "authority" | "participant" | "stakeholder" | "agent", entityId: string, source: Promise<MembershipUserDto[]>) {
    const items = await source;
    items.forEach((item) => {
      this.upsertEntity(this.userAccounts, UserAccountEntity, item.userAccount);
      if (entityType === "authority") this.upsertEntity(this.authorityUsers, AuthorityUserEntity, item.membership);
      if (entityType === "participant") this.upsertEntity(this.participantUsers, ParticipantUserEntity, item.membership);
      if (entityType === "stakeholder") this.upsertEntity(this.stakeholderUsers, StakeholderUserEntity, item.membership);
      if (entityType === "agent") this.upsertEntity(this.agentUsers, AgentUserEntity, item.membership);
    });
    this.removeMissingMemberships(entityType, entityId, items.map((item) => item.membership.id));
  }

  private removeMissingMemberships(entityType: "authority" | "participant" | "stakeholder" | "agent", entityId: string, ids: string[]) {
    const keep = new Set(ids);
    const target =
      entityType === "authority" ? this.authorityUsers :
        entityType === "participant" ? this.participantUsers :
          entityType === "stakeholder" ? this.stakeholderUsers :
            this.agentUsers;
    for (let index = target.length - 1; index >= 0; index -= 1) {
      const dto = target[index].toDto();
      if (dto.entityId === entityId && !keep.has(dto.id)) target.splice(index, 1);
    }
  }

  private async ignoreForbidden(promise: Promise<unknown>) {
    try {
      await promise;
    } catch (error) {
      if (error instanceof ApiError && (error.status === 401 || error.status === 403 || error.status === 404)) {
        return;
      }
      throw error;
    }
  }
}

function unique(values: Array<string | null | undefined>) {
  return [...new Set(values.filter((value): value is string => Boolean(value)))];
}

function entityId(value: unknown) {
  return typeof value === "object" && value !== null && "id" in value
    ? String((value as { id: string }).id)
    : "";
}
