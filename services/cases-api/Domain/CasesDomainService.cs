using System.Text.Json;
using AllChecksOut.Cases.Api.CurrentUser;
using AllChecksOut.Cases.Api.Data;
using AllChecksOut.Cases.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AllChecksOut.Cases.Api.Domain;

public sealed class CasesDomainService(
    AllChecksOutDbContext db,
    ApplicationUserResolver resolver)
{
    private const string EmptyJson = "{}";

    public async Task<IReadOnlyList<Authority>> ListAuthoritiesAsync(CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var authorityIds = user.Memberships
            .Where(membership => membership.AuthorityId is not null)
            .Select(membership => membership.AuthorityId!)
            .Distinct()
            .ToArray();
        return await db.Authorities.AsNoTracking()
            .Where(authority => authorityIds.Contains(authority.Id))
            .OrderBy(authority => authority.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<AuthorityTerminology> GetAuthorityTerminologyAsync(string authorityId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        await RequireAuthorityReadAsync(user, authorityId, cancellationToken);
        return await db.AuthorityTerminologies.AsNoTracking()
            .SingleOrDefaultAsync(item => item.AuthorityId == authorityId, cancellationToken)
            ?? throw DomainException.NotFound($"Authority terminology for {authorityId} was not found.");
    }

    public async Task<AuthorityTerminology> UpdateAuthorityTerminologyAsync(string authorityId, UpdateAuthorityTerminologyCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, authorityId);
        var now = Clock();
        var terminology = await db.AuthorityTerminologies.SingleOrDefaultAsync(item => item.AuthorityId == authorityId, cancellationToken);
        if (terminology is null)
        {
            await RequireAuthorityAsync(authorityId, cancellationToken);
            terminology = new AuthorityTerminology
            {
                Id = NextId("terminology"),
                AuthorityId = authorityId,
                LabelsJson = command.Labels.GetRawText(),
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.AuthorityTerminologies.Add(terminology);
        }
        else
        {
            terminology.LabelsJson = command.Labels.GetRawText();
            terminology.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        return terminology;
    }

    public async Task<IReadOnlyList<Participant>> ListParticipantsAsync(string? authorityId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var query = db.Participants.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(authorityId))
        {
            await RequireAuthorityReadAsync(user, authorityId, cancellationToken);
            query = query.Where(participant => participant.AuthorityId == authorityId);
        }
        else if (!user.Memberships.Any(membership => membership.Type == "system-owner"))
        {
            var participantIds = await ReadableParticipantIdsAsync(user, cancellationToken);
            query = query.Where(participant => participantIds.Contains(participant.Id));
        }

        return await query.OrderBy(participant => participant.DisplayName).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Stakeholder>> ListStakeholdersAsync(string authorityId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, authorityId);
        return await db.Stakeholders.AsNoTracking()
            .Where(stakeholder => stakeholder.AuthorityId == authorityId)
            .OrderBy(stakeholder => stakeholder.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Agent>> ListAgentsAsync(string authorityId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, authorityId);
        return await db.Agents.AsNoTracking()
            .Where(agent => agent.AuthorityId == authorityId)
            .OrderBy(agent => agent.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskType>> ListTaskTypesAsync(CancellationToken cancellationToken) =>
        await db.TaskTypes.AsNoTracking().OrderBy(taskType => taskType.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserAccount>> ListUserAccountsAsync(CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        if (!user.Memberships.Any(membership => membership.Type == "authority"))
        {
            throw DomainException.Forbidden();
        }

        return await db.UserAccounts.AsNoTracking()
            .OrderBy(account => account.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CaseTemplate>> ListCaseTemplatesAsync(string authorityId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, authorityId);
        return await db.CaseTemplates.AsNoTracking()
            .Where(template => template.AuthorityId == authorityId)
            .OrderBy(template => template.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TemplateTask>> ListTemplateTasksAsync(string caseTemplateId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var template = await RequireCaseTemplateAsync(caseTemplateId, cancellationToken);
        RequireAuthorityMember(user, template.AuthorityId);
        return await db.TemplateTasks.AsNoTracking()
            .Where(task => task.CaseTemplateId == caseTemplateId)
            .OrderBy(task => task.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CaseTemplateParticipant>> ListCaseTemplateParticipantsAsync(string caseTemplateId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var template = await RequireCaseTemplateAsync(caseTemplateId, cancellationToken);
        RequireAuthorityMember(user, template.AuthorityId);
        return await db.CaseTemplateParticipants.AsNoTracking()
            .Where(assignment => assignment.CaseTemplateId == caseTemplateId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Case>> ListCasesAsync(string? participantId, string? authorityId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var query = db.Cases.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(participantId))
        {
            await RequireParticipantReadAsync(user, participantId, cancellationToken);
            query = query.Where(caseRecord => caseRecord.ParticipantId == participantId);
        }
        else if (!string.IsNullOrWhiteSpace(authorityId))
        {
            RequireAuthorityMember(user, authorityId);
            query = query.Where(caseRecord => caseRecord.AuthorityId == authorityId);
        }
        else
        {
            var participantIds = await ReadableParticipantIdsAsync(user, cancellationToken);
            query = query.Where(caseRecord => participantIds.Contains(caseRecord.ParticipantId));
        }

        return await query.OrderBy(caseRecord => caseRecord.Id).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Entities.Task>> ListTasksAsync(string caseId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
        await RequireCaseReadAsync(user, caseRecord, cancellationToken);
        return await db.Tasks.AsNoTracking()
            .Where(task => task.CaseId == caseId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParticipantSupplier>> ListParticipantSuppliersAsync(string participantId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        await RequireParticipantReadAsync(user, participantId, cancellationToken);
        return await db.ParticipantSuppliers.AsNoTracking()
            .Where(supplier => supplier.ParticipantId == participantId)
            .OrderBy(supplier => supplier.SupplierName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccessGrant>> ListAccessGrantsAsync(string? participantId, string? stakeholderId, string? agentId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var query = db.AccessGrants.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(participantId))
        {
            await RequireParticipantAdminAsync(user, participantId, cancellationToken);
            query = query.Where(grant => grant.ParticipantId == participantId);
        }
        else if (!string.IsNullOrWhiteSpace(stakeholderId))
        {
            RequireStakeholderMember(user, stakeholderId);
            query = query.Where(grant => grant.GranteeStakeholderId == stakeholderId && grant.Status == "ACTIVE");
        }
        else if (!string.IsNullOrWhiteSpace(agentId))
        {
            RequireAgentMember(user, agentId);
            query = query.Where(grant => grant.GranteeAgentId == agentId && grant.Status == "ACTIVE");
        }
        else
        {
            throw new DomainException("Specify participantId, stakeholderId, or agentId.");
        }

        return await query.OrderBy(grant => grant.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StakeholderParticipantAccess>> ListStakeholderAccessAsync(string stakeholderId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireStakeholderMember(user, stakeholderId);
        return await db.StakeholderParticipantAccesses.AsNoTracking()
            .Where(access => access.StakeholderId == stakeholderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserMembershipDto>> ListMembershipUsersAsync(string entityType, string entityId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        IQueryable<Membership> memberships = entityType switch
        {
            "authority" => db.AuthorityUsers.Where(membership => membership.EntityId == entityId),
            "participant" => db.ParticipantUsers.Where(membership => membership.EntityId == entityId),
            "stakeholder" => db.StakeholderUsers.Where(membership => membership.EntityId == entityId),
            "agent" => db.AgentUsers.Where(membership => membership.EntityId == entityId),
            _ => throw new DomainException("Unsupported membership entity type."),
        };

        await RequireMembershipAdminAsync(user, entityType, entityId, cancellationToken);
        return await memberships.AsNoTracking()
            .Join(db.UserAccounts.AsNoTracking(), membership => membership.UserAccountId, account => account.Id, (membership, account) => new UserMembershipDto(membership, account))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserAccount> UpdateUserEmailAsync(string userAccountId, UpdateUserEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        if (user.User.Id != userAccountId && !user.Memberships.Any(membership => membership.Type == "authority"))
        {
            throw DomainException.Forbidden();
        }

        var normalized = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !normalized.Contains('@'))
        {
            throw new DomainException("Enter a valid email address.");
        }

        if (await db.UserAccounts.AnyAsync(account => account.Id != userAccountId && account.Email.ToLower() == normalized, cancellationToken))
        {
            throw new DomainException("A user account with this email already exists.");
        }

        var account = await RequireUserAccountAsync(userAccountId, cancellationToken);
        account.Email = normalized;
        account.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<UserAccount> RegisterUserAccountWithEntraAsync(string userAccountId, RegisterUserAccountWithEntraCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        if (user.User.Id != userAccountId && !user.Memberships.Any(membership => membership.Type == "authority"))
        {
            throw DomainException.Forbidden();
        }

        var objectId = command.EntraObjectId.Trim();
        if (string.IsNullOrWhiteSpace(objectId))
        {
            throw new DomainException("The Entra object id was not returned.");
        }

        var account = await RequireUserAccountAsync(userAccountId, cancellationToken);
        account.EntraObjectId = objectId;
        account.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Participant> CreateParticipantAsync(CreateParticipantCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, command.AuthorityId);
        await RequireAuthorityAsync(command.AuthorityId, cancellationToken);
        var now = Clock();
        var participant = new Participant
        {
            Id = NextId("participant"),
            AuthorityId = command.AuthorityId,
            DisplayName = Required(command.DisplayName, "Enter a participant name."),
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.Participants.Add(participant);
        if (command.InitialUser is not null)
        {
            var account = await CreateUserAccountAsync(command.InitialUser, cancellationToken);
            db.ParticipantUsers.Add(NewMembership<ParticipantUser>("participant-user", participant.Id, account.Id, now));
        }
        await db.SaveChangesAsync(cancellationToken);
        return participant;
    }

    public async Task<object> CreateMembershipUserAsync(string entityType, string entityId, EntityUserCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        await RequireMembershipAdminAsync(user, entityType, entityId, cancellationToken);
        var now = Clock();
        var account = await CreateUserAccountAsync(command, cancellationToken);
        Membership membership = entityType switch
        {
            "authority" => NewMembership<AuthorityUser>("authority-user", entityId, account.Id, now),
            "participant" => NewMembership<ParticipantUser>("participant-user", entityId, account.Id, now),
            "stakeholder" => NewMembership<StakeholderUser>("stakeholder-user", entityId, account.Id, now),
            "agent" => NewMembership<AgentUser>("agent-user", entityId, account.Id, now),
            _ => throw new DomainException("Unsupported membership entity type."),
        };
        db.Add(membership);
        await db.SaveChangesAsync(cancellationToken);
        return new { userAccount = account, membership };
    }

    public async Task<Stakeholder> CreateStakeholderAsync(CreateEntityCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, command.AuthorityId);
        var now = Clock();
        var stakeholder = new Stakeholder { Id = NextId("stakeholder"), AuthorityId = command.AuthorityId, DisplayName = Required(command.DisplayName, "Enter a stakeholder name."), CreatedAt = now, UpdatedAt = now };
        db.Stakeholders.Add(stakeholder);
        await db.SaveChangesAsync(cancellationToken);
        return stakeholder;
    }

    public async Task<Agent> CreateAgentAsync(CreateEntityCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, command.AuthorityId);
        var now = Clock();
        var agent = new Agent { Id = NextId("agent"), AuthorityId = command.AuthorityId, DisplayName = Required(command.DisplayName, "Enter an agent name."), CreatedAt = now, UpdatedAt = now };
        db.Agents.Add(agent);
        await db.SaveChangesAsync(cancellationToken);
        return agent;
    }

    public async Task<StakeholderParticipantAccess> GrantStakeholderAccessAsync(GrantStakeholderAccessCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        await RequireParticipantAdminAsync(user, command.ParticipantId, cancellationToken);
        var stakeholder = await RequireStakeholderAsync(command.StakeholderId, cancellationToken);
        var participant = await RequireParticipantAsync(command.ParticipantId, cancellationToken);
        await RequireUserAccountAsync(command.ApprovedByUserId, cancellationToken);
        if (stakeholder.AuthorityId != participant.AuthorityId)
        {
            throw new DomainException("Stakeholder and participant must belong to the same authority.");
        }

        if (await db.StakeholderParticipantAccesses.AnyAsync(access => access.StakeholderId == command.StakeholderId && access.ParticipantId == command.ParticipantId, cancellationToken))
        {
            throw new DomainException("Stakeholder access already exists for this participant.");
        }

        var now = Clock();
        var access = new StakeholderParticipantAccess
        {
            Id = NextId("access"),
            StakeholderId = command.StakeholderId,
            ParticipantId = command.ParticipantId,
            ApprovedByUserId = command.ApprovedByUserId,
            ApprovedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.StakeholderParticipantAccesses.Add(access);
        db.AccessGrants.Add(new AccessGrant
        {
            Id = NextId("grant"),
            AuthorityId = participant.AuthorityId,
            ParticipantId = participant.Id,
            GranteeType = "STAKEHOLDER",
            GranteeStakeholderId = stakeholder.Id,
            PermissionLevel = "REQUEST_INFORMATION",
            DataScopeType = "PARTICIPANT",
            Status = "ACTIVE",
            CreatedByUserId = command.ApprovedByUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync(cancellationToken);
        return access;
    }

    public async Task<AccessGrant> CreateAccessGrantAsync(CreateAccessGrantCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        await RequireParticipantAdminAsync(user, command.ParticipantId, cancellationToken);
        var authority = await RequireAuthorityAsync(command.AuthorityId, cancellationToken);
        var participant = await RequireParticipantAsync(command.ParticipantId, cancellationToken);
        await RequireUserAccountAsync(command.CreatedByUserId, cancellationToken);
        if (participant.AuthorityId != authority.Id)
        {
            throw new DomainException("Access grant participant must belong to the selected authority.");
        }

        await ValidateGrantScopeAsync(command, participant, cancellationToken);
        var dataScopeType = command.DataScopeType ?? "PARTICIPANT";
        var dataScopeId = command.DataScopeId;
        var duplicate = await db.AccessGrants.AnyAsync(grant =>
            grant.ParticipantId == command.ParticipantId &&
            grant.GranteeType == command.GranteeType &&
            grant.GranteeStakeholderId == command.GranteeStakeholderId &&
            grant.GranteeAgentId == command.GranteeAgentId &&
            grant.GranteeUserId == command.GranteeUserId &&
            grant.DataScopeType == dataScopeType &&
            grant.DataScopeId == dataScopeId &&
            grant.Status != "REVOKED", cancellationToken);
        if (duplicate)
        {
            throw new DomainException("An active access grant already exists for that grantee and scope.");
        }

        var now = Clock();
        var accessGrant = new AccessGrant
        {
            Id = NextId("grant"),
            AuthorityId = authority.Id,
            ParticipantId = participant.Id,
            GranteeType = command.GranteeType,
            GranteeStakeholderId = command.GranteeStakeholderId,
            GranteeAgentId = command.GranteeAgentId,
            GranteeUserId = command.GranteeUserId,
            PermissionLevel = command.PermissionLevel,
            DataScopeType = dataScopeType,
            DataScopeId = dataScopeId,
            Status = command.Status ?? "ACTIVE",
            CreatedByUserId = command.CreatedByUserId,
            ExpiresAt = command.ExpiresAt,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.AccessGrants.Add(accessGrant);
        await db.SaveChangesAsync(cancellationToken);
        return accessGrant;
    }

    public async Task<AccessGrant> UpdateAccessGrantStatusAsync(string accessGrantId, UpdateAccessGrantStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var grant = await RequireAccessGrantAsync(accessGrantId, cancellationToken);
        await RequireParticipantAdminAsync(user, grant.ParticipantId, cancellationToken);
        grant.Status = command.Status;
        grant.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return grant;
    }

    public async Task<ParticipantSupplier> CreateParticipantSupplierAsync(CreateParticipantSupplierCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        await RequireParticipantEditAsync(user, command.ParticipantId, cancellationToken);
        var authority = await RequireAuthorityAsync(command.AuthorityId, cancellationToken);
        var participant = await RequireParticipantAsync(command.ParticipantId, cancellationToken);
        if (participant.AuthorityId != authority.Id)
        {
            throw new DomainException("Participant supplier record must belong to the selected authority.");
        }

        var now = Clock();
        var supplier = new ParticipantSupplier
        {
            Id = NextId("participant-supplier"),
            AuthorityId = authority.Id,
            ParticipantId = participant.Id,
            SupplierName = Required(command.SupplierName, "Enter a participant supplier name."),
            RelationshipType = string.IsNullOrWhiteSpace(command.RelationshipType) ? "Supplier" : command.RelationshipType.Trim(),
            ServicesProvided = command.ServicesProvided.Trim(),
            DataExposure = command.DataExposure.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.ParticipantSuppliers.Add(supplier);
        await db.SaveChangesAsync(cancellationToken);
        return supplier;
    }

    public async Task<Case> LinkParticipantSupplierToCaseAsync(string caseId, LinkParticipantSupplierCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
        await RequireParticipantEditAsync(user, caseRecord.ParticipantId, cancellationToken);
        var supplier = await RequireParticipantSupplierAsync(command.ParticipantSupplierId, cancellationToken);
        if (caseRecord.AuthorityId != supplier.AuthorityId || caseRecord.ParticipantId != supplier.ParticipantId)
        {
            throw new DomainException("Supplier and case must belong to the same participant.");
        }
        if (await db.Cases.AnyAsync(candidate => candidate.ParticipantSupplierId == supplier.Id && candidate.Id != caseRecord.Id, cancellationToken))
        {
            throw new DomainException("Supplier is already linked to a case.");
        }
        if (caseRecord.ParticipantSupplierId is not null && caseRecord.ParticipantSupplierId != supplier.Id)
        {
            throw new DomainException("Case is already linked to another supplier.");
        }

        caseRecord.ParticipantSupplierId = supplier.Id;
        caseRecord.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return caseRecord;
    }

    public async Task<Case> UnlinkParticipantSupplierFromCaseAsync(string caseId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
        await RequireParticipantEditAsync(user, caseRecord.ParticipantId, cancellationToken);
        caseRecord.ParticipantSupplierId = null;
        caseRecord.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return caseRecord;
    }

    public async Task<IReadOnlyList<StakeholderReview>> ListStakeholderReviewsAsync(string caseId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
        await RequireCaseReadAsync(user, caseRecord, cancellationToken);
        return await db.StakeholderReviews.AsNoTracking().Where(review => review.CaseId == caseId).ToListAsync(cancellationToken);
    }

    public async Task<StakeholderReview> UpsertStakeholderReviewAsync(UpsertStakeholderReviewCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireStakeholderMember(user, command.StakeholderId);
        var caseRecord = await RequireCaseAsync(command.CaseId, cancellationToken);
        if (!await HasStakeholderGrantForCaseAsync(command.StakeholderId, caseRecord, false, cancellationToken))
        {
            throw DomainException.Forbidden("Stakeholder review requires an active access grant.");
        }

        var now = Clock();
        var review = await db.StakeholderReviews.SingleOrDefaultAsync(item => item.StakeholderId == command.StakeholderId && item.CaseId == command.CaseId, cancellationToken);
        if (review is null)
        {
            review = new StakeholderReview { Id = NextId("stakeholder-review"), StakeholderId = command.StakeholderId, CaseId = command.CaseId, Note = command.Note, ReviewedByUserId = command.ReviewedByUserId, ReviewedAt = now, CreatedAt = now, UpdatedAt = now };
            db.StakeholderReviews.Add(review);
        }
        else
        {
            review.Note = command.Note;
            review.ReviewedByUserId = command.ReviewedByUserId;
            review.ReviewedAt = now;
            review.UpdatedAt = now;
        }
        await db.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task<IReadOnlyList<RequestForInformation>> ListRequestsForInformationAsync(string? caseId, string? participantId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var query = db.RequestsForInformation.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(caseId))
        {
            var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
            await RequireCaseReadAsync(user, caseRecord, cancellationToken);
            query = query.Where(request => request.CaseId == caseId);
        }
        else if (!string.IsNullOrWhiteSpace(participantId))
        {
            await RequireParticipantReadAsync(user, participantId, cancellationToken);
            query = query.Where(request => request.ParticipantId == participantId);
        }
        else
        {
            throw new DomainException("Specify caseId or participantId.");
        }

        return await query.OrderByDescending(request => request.RequestedAt).ToListAsync(cancellationToken);
    }

    public async Task<RequestForInformation> CreateRequestForInformationAsync(CreateRequestForInformationCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireStakeholderMember(user, command.StakeholderId);
        var caseRecord = await RequireCaseAsync(command.CaseId, cancellationToken);
        Entities.Task? task = null;
        if (!string.IsNullOrWhiteSpace(command.TaskId))
        {
            task = await RequireTaskAsync(command.TaskId, cancellationToken);
            if (task.CaseId != caseRecord.Id)
            {
                throw new DomainException("The selected task does not belong to this case.");
            }
        }
        if (!await HasStakeholderGrantForCaseAsync(command.StakeholderId, caseRecord, true, cancellationToken))
        {
            throw DomainException.Forbidden("Requesting information requires an active access grant with request permission.");
        }

        var requestText = Required(command.RequestText, "Enter a request for information.");
        var now = Clock();
        var statusHistory = JsonSerializer.Serialize(new[] { new { status = "OPEN", at = now, byUserId = command.RequestedByUserId, note = "Request created" } });
        var request = new RequestForInformation
        {
            Id = NextId("rfi"),
            AuthorityId = caseRecord.AuthorityId,
            ParticipantId = caseRecord.ParticipantId,
            StakeholderId = command.StakeholderId,
            CaseId = caseRecord.Id,
            TaskId = task?.Id,
            RequestText = requestText,
            ResponseText = "",
            Status = "OPEN",
            RequestedByUserId = command.RequestedByUserId,
            RequestedAt = now,
            StatusHistoryJson = statusHistory,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.RequestsForInformation.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<RequestForInformation> RespondToRequestForInformationAsync(string requestId, RespondToRequestForInformationCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var request = await RequireRequestForInformationAsync(requestId, cancellationToken);
        if (command.RespondedByUserId != user.User.Id)
        {
            throw DomainException.Forbidden();
        }
        await RequireParticipantEditAsync(user, request.ParticipantId, cancellationToken);
        var responseText = Required(command.ResponseText, "Enter a response before updating the request.");
        var status = command.Status ?? "ANSWERED";
        if (status is not ("IN_PROGRESS" or "ANSWERED"))
        {
            throw new DomainException("Response status must be IN_PROGRESS or ANSWERED.");
        }

        var now = Clock();
        request.ResponseText = responseText;
        request.Status = status;
        request.AssignedToUserId ??= user.User.Id;
        request.RespondedByUserId = user.User.Id;
        request.RespondedAt = now;
        request.UpdatedAt = now;
        request.StatusHistoryJson = AppendStatusHistory(request.StatusHistoryJson, status, now, user.User.Id, status == "ANSWERED" ? "Participant answered request" : "Participant response in progress");
        await db.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<RequestForInformation> UpdateRequestForInformationStatusAsync(string requestId, UpdateRequestForInformationStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var request = await RequireRequestForInformationAsync(requestId, cancellationToken);
        RequireStakeholderMember(user, request.StakeholderId);
        if (command.UpdatedByUserId != user.User.Id)
        {
            throw DomainException.Forbidden();
        }
        if (command.Status is not ("ACCEPTED" or "WITHDRAWN"))
        {
            throw new DomainException("Request status must be ACCEPTED or WITHDRAWN.");
        }

        var caseRecord = request.CaseId is null ? null : await RequireCaseAsync(request.CaseId, cancellationToken);
        if (caseRecord is not null && !await HasStakeholderGrantForCaseAsync(request.StakeholderId, caseRecord, false, cancellationToken))
        {
            throw DomainException.Forbidden();
        }

        var now = Clock();
        request.Status = command.Status;
        request.UpdatedAt = now;
        request.StatusHistoryJson = AppendStatusHistory(request.StatusHistoryJson, command.Status, now, user.User.Id, command.Note ?? command.Status.ToLowerInvariant());
        await db.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<CaseTemplate> CreateCaseTemplateAsync(CreateCaseTemplateCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        RequireAuthorityMember(user, command.AuthorityId);
        var now = Clock();
        var template = new CaseTemplate
        {
            Id = NextId("template"),
            AuthorityId = command.AuthorityId,
            Name = Required(command.Name, "Enter a case template name."),
            Description = command.Description.Trim(),
            Status = "DRAFT",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.CaseTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<TemplateTask> AddTaskToTemplateAsync(string caseTemplateId, AddTaskToTemplateCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var template = await RequireCaseTemplateAsync(caseTemplateId, cancellationToken);
        RequireAuthorityMember(user, template.AuthorityId);
        if (template.Status == "FINALIZED")
        {
            throw new DomainException("Finalized case templates cannot be edited.");
        }
        await RequireTaskTypeAsync(command.TaskTypeId, cancellationToken);
        var maxSort = await db.TemplateTasks.Where(task => task.CaseTemplateId == caseTemplateId).Select(task => (int?)task.SortOrder).MaxAsync(cancellationToken) ?? 0;
        var now = Clock();
        var task = new TemplateTask
        {
            Id = NextId("template-task"),
            CaseTemplateId = caseTemplateId,
            TaskTypeId = command.TaskTypeId,
            Title = Required(command.Title, "Enter a task title."),
            Description = command.Description.Trim(),
            ParametersJson = DomainDto.JsonString(command.ParametersJson),
            SortOrder = maxSort + 1,
            Status = "ACTIVE",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.TemplateTasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<CaseTemplate> FinalizeCaseTemplateAsync(string caseTemplateId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var template = await RequireCaseTemplateAsync(caseTemplateId, cancellationToken);
        RequireAuthorityMember(user, template.AuthorityId);
        if (template.Status != "FINALIZED")
        {
            template.Status = "FINALIZED";
            template.UpdatedAt = Clock();
            await db.SaveChangesAsync(cancellationToken);
        }
        return template;
    }

    public async Task<CaseTemplateParticipant> AssignParticipantToTemplateAsync(string caseTemplateId, AssignParticipantToTemplateCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var template = await RequireCaseTemplateAsync(caseTemplateId, cancellationToken);
        RequireAuthorityMember(user, template.AuthorityId);
        if (template.Status != "FINALIZED")
        {
            throw new DomainException("Case templates must be finalized before participants can be assigned.");
        }
        var participant = await RequireParticipantAsync(command.ParticipantId, cancellationToken);
        if (participant.AuthorityId != template.AuthorityId)
        {
            throw new DomainException("Participant must belong to the template authority.");
        }
        if (await db.CaseTemplateParticipants.AnyAsync(assignment => assignment.CaseTemplateId == caseTemplateId && assignment.ParticipantId == command.ParticipantId, cancellationToken))
        {
            throw new DomainException("Participant is already assigned to this template.");
        }

        var now = Clock();
        var caseRecord = new Case
        {
            Id = NextId("case"),
            AuthorityId = template.AuthorityId,
            CaseTemplateId = template.Id,
            ParticipantId = participant.Id,
            Status = "INCOMPLETE",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.Cases.Add(caseRecord);
        var activeTasks = await db.TemplateTasks.AsNoTracking()
            .Where(task => task.CaseTemplateId == template.Id && task.Status == "ACTIVE")
            .OrderBy(task => task.SortOrder)
            .ToListAsync(cancellationToken);
        foreach (var templateTask in activeTasks)
        {
            db.Tasks.Add(new Entities.Task { Id = NextId("task"), CaseId = caseRecord.Id, TemplateTaskId = templateTask.Id, Status = "NOT_STARTED", ResponseJson = EmptyJson, EvidenceJson = EmptyJson, CreatedAt = now, UpdatedAt = now });
        }
        var assignment = new CaseTemplateParticipant
        {
            Id = NextId("template-participant"),
            CaseTemplateId = template.Id,
            ParticipantId = participant.Id,
            CaseId = caseRecord.Id,
            ExemptionReason = command.ExemptionReason,
            DecidedByUserId = command.DecidedByUserId,
            DecidedAt = command.DecidedByUserId is null ? null : now,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.CaseTemplateParticipants.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<Entities.Task> CompleteTaskAsync(string taskId, CompleteTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await RequireEditableTaskAsync(taskId, cancellationToken);
        task.ResponseJson = command.ResponseJson.GetRawText();
        task.Status = "IN_PROGRESS";
        task.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<Entities.Task> UploadEvidenceAsync(string taskId, UploadEvidenceCommand command, CancellationToken cancellationToken)
    {
        var task = await RequireEditableTaskAsync(taskId, cancellationToken);
        task.EvidenceJson = command.EvidenceJson.GetRawText();
        if (task.Status == "NOT_STARTED")
        {
            task.Status = "IN_PROGRESS";
        }
        task.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<Entities.Task> SubmitTaskAsync(string taskId, CancellationToken cancellationToken)
    {
        var task = await RequireEditableTaskAsync(taskId, cancellationToken);
        task.Status = "SUBMITTED";
        task.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<Case> SubmitCaseAsync(string caseId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
        await RequireParticipantEditAsync(user, caseRecord.ParticipantId, cancellationToken);
        if (caseRecord.Status == "WITHDRAWN")
        {
            throw new DomainException("Withdrawn cases cannot be completed.");
        }
        var hasUnsubmittedTasks = await db.Tasks.AnyAsync(task => task.CaseId == caseId && task.Status != "WITHDRAWN" && (task.Status == "NOT_STARTED" || task.Status == "IN_PROGRESS"), cancellationToken);
        if (hasUnsubmittedTasks)
        {
            throw new DomainException("All active tasks must be submitted before the case can be submitted.");
        }
        var now = Clock();
        caseRecord.Status = "COMPLETE";
        caseRecord.SubmittedAt = now;
        caseRecord.ClosedAt = now;
        caseRecord.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        return caseRecord;
    }

    public async Task<Entities.Task> ReviewTaskAsync(string taskId, ReviewTaskCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var task = await RequireTaskAsync(taskId, cancellationToken);
        if (task.Status == "WITHDRAWN")
        {
            throw new DomainException("Withdrawn tasks cannot be reviewed.");
        }
        var caseRecord = await RequireCaseAsync(task.CaseId, cancellationToken);
        RequireAuthorityMember(user, caseRecord.AuthorityId);
        if (command.Decision is not ("PASSED" or "FAILED"))
        {
            throw new DomainException("Task review decision must be PASSED or FAILED.");
        }
        task.Status = command.Decision;
        task.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TemplateTask> WithdrawTemplateTaskAsync(string templateTaskId, WithdrawTemplateTaskCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var templateTask = await RequireTemplateTaskAsync(templateTaskId, cancellationToken);
        var template = await RequireCaseTemplateAsync(templateTask.CaseTemplateId, cancellationToken);
        RequireAuthorityMember(user, template.AuthorityId);
        if (template.Status == "FINALIZED")
        {
            throw new DomainException("Finalized case templates cannot be edited.");
        }
        await RequireUserAccountAsync(command.WithdrawnByUserId, cancellationToken);
        var now = Clock();
        templateTask.Status = "WITHDRAWN";
        templateTask.WithdrawnAt = now;
        templateTask.WithdrawnByUserId = command.WithdrawnByUserId;
        templateTask.WithdrawnReason = command.WithdrawnReason;
        templateTask.UpdatedAt = now;
        var generatedTasks = await db.Tasks.Where(task => task.TemplateTaskId == templateTaskId && task.Status != "PASSED" && task.Status != "FAILED" && task.Status != "WITHDRAWN").ToListAsync(cancellationToken);
        foreach (var task in generatedTasks)
        {
            task.Status = "WITHDRAWN";
            task.WithdrawnAt = now;
            task.UpdatedAt = now;
        }
        await db.SaveChangesAsync(cancellationToken);
        return templateTask;
    }

    public async Task<Case> WithdrawCaseAsync(string caseId, WithdrawCaseCommand command, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
        RequireAuthorityMember(user, caseRecord.AuthorityId);
        if (caseRecord.Status == "WITHDRAWN")
        {
            throw new DomainException("Case is already withdrawn.");
        }
        var now = Clock();
        caseRecord.Status = "WITHDRAWN";
        caseRecord.WithdrawnAt = now;
        caseRecord.WithdrawnByUserId = command.WithdrawnByUserId;
        caseRecord.WithdrawnReason = Required(command.WithdrawnReason, "Enter a withdrawal reason.");
        caseRecord.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        return caseRecord;
    }

    public async Task<Case> ReinstateCaseAsync(string caseId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var caseRecord = await RequireCaseAsync(caseId, cancellationToken);
        RequireAuthorityMember(user, caseRecord.AuthorityId);
        if (caseRecord.Status != "WITHDRAWN")
        {
            throw new DomainException("Only withdrawn cases can be reinstated.");
        }
        caseRecord.Status = "INCOMPLETE";
        caseRecord.WithdrawnAt = null;
        caseRecord.WithdrawnByUserId = null;
        caseRecord.WithdrawnReason = null;
        caseRecord.UpdatedAt = Clock();
        await db.SaveChangesAsync(cancellationToken);
        return caseRecord;
    }

    public async Task<CaseTemplate> DeleteCaseTemplateAsync(string caseTemplateId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var template = await RequireCaseTemplateAsync(caseTemplateId, cancellationToken);
        RequireAuthorityMember(user, template.AuthorityId);
        if (await db.CaseTemplateParticipants.AnyAsync(assignment => assignment.CaseTemplateId == caseTemplateId, cancellationToken))
        {
            throw new DomainException("Case templates with assigned participants cannot be deleted.");
        }
        db.CaseTemplates.Remove(template);
        await db.SaveChangesAsync(cancellationToken);
        return template;
    }

    private async Task<ApplicationUserContext> RequireUserAsync(CancellationToken cancellationToken)
    {
        var user = await resolver.ResolveApplicationUserAsync(cancellationToken);
        if (user?.User.Status != "ACTIVE")
        {
            throw DomainException.Forbidden("The authenticated user is not linked to an active application account.");
        }
        return user;
    }

    private async Task RequireAuthorityReadAsync(ApplicationUserContext user, string authorityId, CancellationToken cancellationToken)
    {
        if (user.IsAuthorityMember(authorityId))
        {
            return;
        }

        var participantIds = await ReadableParticipantIdsAsync(user, cancellationToken);
        var hasAuthorityParticipant = await db.Participants.AnyAsync(participant => participant.AuthorityId == authorityId && participantIds.Contains(participant.Id), cancellationToken);
        if (!hasAuthorityParticipant)
        {
            throw DomainException.Forbidden();
        }
    }

    private static void RequireAuthorityMember(ApplicationUserContext user, string authorityId)
    {
        if (!user.IsAuthorityMember(authorityId))
        {
            throw DomainException.Forbidden();
        }
    }

    private static void RequireStakeholderMember(ApplicationUserContext user, string stakeholderId)
    {
        if (!user.IsStakeholderMember(stakeholderId))
        {
            throw DomainException.Forbidden();
        }
    }

    private static void RequireAgentMember(ApplicationUserContext user, string agentId)
    {
        if (!user.IsAgentMember(agentId))
        {
            throw DomainException.Forbidden();
        }
    }

    private async Task RequireParticipantReadAsync(ApplicationUserContext user, string participantId, CancellationToken cancellationToken)
    {
        if (user.IsParticipantMember(participantId))
        {
            return;
        }

        var participant = await RequireParticipantAsync(participantId, cancellationToken);
        if (user.IsAuthorityMember(participant.AuthorityId) ||
            await HasStakeholderGrantForParticipantAsync(user, participantId, cancellationToken) ||
            await HasAgentGrantForParticipantAsync(user, participantId, false, cancellationToken))
        {
            return;
        }

        throw DomainException.Forbidden();
    }

    private async Task RequireCaseReadAsync(ApplicationUserContext user, Case caseRecord, CancellationToken cancellationToken)
    {
        if (user.IsParticipantMember(caseRecord.ParticipantId) || user.IsAuthorityMember(caseRecord.AuthorityId))
        {
            return;
        }

        foreach (var stakeholderId in user.Memberships.Where(membership => membership.Type == "stakeholder").Select(membership => membership.EntityId))
        {
            if (await HasStakeholderGrantForCaseAsync(stakeholderId, caseRecord, false, cancellationToken))
            {
                return;
            }
        }

        if (await HasAgentGrantForParticipantAsync(user, caseRecord.ParticipantId, false, cancellationToken))
        {
            return;
        }

        throw DomainException.Forbidden();
    }

    private async Task RequireParticipantAdminAsync(ApplicationUserContext user, string participantId, CancellationToken cancellationToken)
    {
        if (user.IsParticipantMember(participantId))
        {
            return;
        }

        var participant = await RequireParticipantAsync(participantId, cancellationToken);
        if (user.IsAuthorityMember(participant.AuthorityId) ||
            await HasAgentGrantForParticipantAsync(user, participantId, true, cancellationToken, administerOnly: true))
        {
            return;
        }

        throw DomainException.Forbidden();
    }

    private async Task RequireParticipantEditAsync(ApplicationUserContext user, string participantId, CancellationToken cancellationToken)
    {
        if (user.IsParticipantMember(participantId))
        {
            return;
        }

        var participant = await RequireParticipantAsync(participantId, cancellationToken);
        if (user.IsAuthorityMember(participant.AuthorityId) ||
            await HasAgentGrantForParticipantAsync(user, participantId, true, cancellationToken))
        {
            return;
        }

        throw DomainException.Forbidden();
    }

    private async Task RequireMembershipAdminAsync(ApplicationUserContext user, string entityType, string entityId, CancellationToken cancellationToken)
    {
        switch (entityType)
        {
            case "authority":
                RequireAuthorityMember(user, entityId);
                break;
            case "participant":
                await RequireParticipantAdminAsync(user, entityId, cancellationToken);
                break;
            case "stakeholder":
                var stakeholder = await RequireStakeholderAsync(entityId, cancellationToken);
                RequireAuthorityMember(user, stakeholder.AuthorityId);
                break;
            case "agent":
                var agent = await RequireAgentAsync(entityId, cancellationToken);
                RequireAuthorityMember(user, agent.AuthorityId);
                break;
            default:
                throw new DomainException("Unsupported membership entity type.");
        }
    }

    private async Task<string[]> ReadableParticipantIdsAsync(ApplicationUserContext user, CancellationToken cancellationToken)
    {
        var ids = user.Memberships.Where(membership => membership.Type == "participant").Select(membership => membership.EntityId).ToHashSet(StringComparer.Ordinal);
        var authorityIds = user.Memberships.Where(membership => membership.Type == "authority").Select(membership => membership.EntityId).ToArray();
        foreach (var participantId in await db.Participants.AsNoTracking().Where(participant => authorityIds.Contains(participant.AuthorityId)).Select(participant => participant.Id).ToListAsync(cancellationToken))
        {
            ids.Add(participantId);
        }
        var stakeholderIds = user.Memberships.Where(membership => membership.Type == "stakeholder").Select(membership => membership.EntityId).ToArray();
        foreach (var participantId in await db.AccessGrants.AsNoTracking().Where(grant => grant.Status == "ACTIVE" && grant.GranteeStakeholderId != null && stakeholderIds.Contains(grant.GranteeStakeholderId)).Select(grant => grant.ParticipantId).ToListAsync(cancellationToken))
        {
            ids.Add(participantId);
        }
        var agentIds = user.Memberships.Where(membership => membership.Type == "agent").Select(membership => membership.EntityId).ToArray();
        foreach (var participantId in await db.AccessGrants.AsNoTracking().Where(grant => grant.Status == "ACTIVE" && grant.GranteeAgentId != null && agentIds.Contains(grant.GranteeAgentId)).Select(grant => grant.ParticipantId).ToListAsync(cancellationToken))
        {
            ids.Add(participantId);
        }
        return ids.ToArray();
    }

    private async Task<bool> HasStakeholderGrantForParticipantAsync(ApplicationUserContext user, string participantId, CancellationToken cancellationToken)
    {
        var stakeholderIds = user.Memberships.Where(membership => membership.Type == "stakeholder").Select(membership => membership.EntityId).ToArray();
        return await db.AccessGrants.AsNoTracking().AnyAsync(grant => grant.Status == "ACTIVE" && grant.ParticipantId == participantId && grant.GranteeStakeholderId != null && stakeholderIds.Contains(grant.GranteeStakeholderId), cancellationToken);
    }

    private async Task<bool> HasStakeholderGrantForCaseAsync(string stakeholderId, Case caseRecord, bool requireRequestPermission, CancellationToken cancellationToken)
    {
        var grants = await db.AccessGrants.AsNoTracking()
            .Where(grant => grant.Status == "ACTIVE" && grant.GranteeType == "STAKEHOLDER" && grant.GranteeStakeholderId == stakeholderId && grant.ParticipantId == caseRecord.ParticipantId)
            .ToListAsync(cancellationToken);
        foreach (var grant in grants)
        {
            if (requireRequestPermission && grant.PermissionLevel == "READ_ONLY")
            {
                continue;
            }
            if (await GrantAllowsCaseAsync(grant, caseRecord, cancellationToken))
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> HasAgentGrantForParticipantAsync(ApplicationUserContext user, string participantId, bool requireEdit, CancellationToken cancellationToken, bool administerOnly = false)
    {
        var agentIds = user.Memberships.Where(membership => membership.Type == "agent").Select(membership => membership.EntityId).ToArray();
        var query = db.AccessGrants.AsNoTracking()
            .Where(grant => grant.Status == "ACTIVE" && grant.GranteeType == "AGENT" && grant.GranteeAgentId != null && agentIds.Contains(grant.GranteeAgentId) && grant.ParticipantId == participantId);
        if (administerOnly)
        {
            query = query.Where(grant => grant.PermissionLevel == "ADMINISTER_GRANTS");
        }
        else if (requireEdit)
        {
            query = query.Where(grant => grant.PermissionLevel == "CREATE_AND_EDIT" || grant.PermissionLevel == "ADMINISTER_GRANTS");
        }
        return await query.AnyAsync(cancellationToken);
    }

    private async Task<bool> GrantAllowsCaseAsync(AccessGrant grant, Case caseRecord, CancellationToken cancellationToken)
    {
        if (grant.ParticipantId != caseRecord.ParticipantId)
        {
            return false;
        }
        return grant.DataScopeType switch
        {
            "PARTICIPANT" => true,
            "CASE" => grant.DataScopeId == caseRecord.Id,
            "TASK" => await db.Tasks.AsNoTracking().AnyAsync(task => task.CaseId == caseRecord.Id && task.Id == grant.DataScopeId, cancellationToken),
            "PARTICIPANT_SUPPLIER" => caseRecord.ParticipantSupplierId == grant.DataScopeId,
            "EVIDENCE_METADATA" => true,
            _ => false,
        };
    }

    private async Task ValidateGrantScopeAsync(CreateAccessGrantCommand command, Participant participant, CancellationToken cancellationToken)
    {
        switch (command.GranteeType)
        {
            case "STAKEHOLDER":
                if (string.IsNullOrWhiteSpace(command.GranteeStakeholderId))
                {
                    throw new DomainException("Select a stakeholder for this access grant.");
                }
                var stakeholder = await RequireStakeholderAsync(command.GranteeStakeholderId, cancellationToken);
                if (stakeholder.AuthorityId != participant.AuthorityId)
                {
                    throw new DomainException("Access grant grantee must belong to the selected authority.");
                }
                break;
            case "AGENT":
                if (string.IsNullOrWhiteSpace(command.GranteeAgentId))
                {
                    throw new DomainException("Select an agent for this access grant.");
                }
                var agent = await RequireAgentAsync(command.GranteeAgentId, cancellationToken);
                if (agent.AuthorityId != participant.AuthorityId)
                {
                    throw new DomainException("Access grant grantee must belong to the selected authority.");
                }
                break;
            case "USER":
                if (string.IsNullOrWhiteSpace(command.GranteeUserId))
                {
                    throw new DomainException("Select a user for this access grant.");
                }
                await RequireUserAccountAsync(command.GranteeUserId, cancellationToken);
                break;
        }

        if (command.DataScopeType == "PARTICIPANT_SUPPLIER")
        {
            if (string.IsNullOrWhiteSpace(command.DataScopeId))
            {
                throw new DomainException("Select a participant supplier record for this scoped grant.");
            }
            var supplier = await RequireParticipantSupplierAsync(command.DataScopeId, cancellationToken);
            if (supplier.ParticipantId != participant.Id || supplier.AuthorityId != participant.AuthorityId)
            {
                throw new DomainException("Participant supplier grant scope must belong to the granting participant.");
            }
        }
    }

    private async Task<Entities.Task> RequireEditableTaskAsync(string taskId, CancellationToken cancellationToken)
    {
        var user = await RequireUserAsync(cancellationToken);
        var task = await RequireTaskAsync(taskId, cancellationToken);
        if (task.Status == "WITHDRAWN")
        {
            throw new DomainException("Withdrawn tasks cannot be updated.");
        }
        var caseRecord = await RequireCaseAsync(task.CaseId, cancellationToken);
        await RequireParticipantEditAsync(user, caseRecord.ParticipantId, cancellationToken);
        return task;
    }

    private async Task<Authority> RequireAuthorityAsync(string id, CancellationToken cancellationToken) =>
        await db.Authorities.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Authority {id} was not found.");

    private async Task<Participant> RequireParticipantAsync(string id, CancellationToken cancellationToken) =>
        await db.Participants.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Participant {id} was not found.");

    private async Task<Stakeholder> RequireStakeholderAsync(string id, CancellationToken cancellationToken) =>
        await db.Stakeholders.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Stakeholder {id} was not found.");

    private async Task<Agent> RequireAgentAsync(string id, CancellationToken cancellationToken) =>
        await db.Agents.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Agent {id} was not found.");

    private async Task<UserAccount> RequireUserAccountAsync(string id, CancellationToken cancellationToken) =>
        await db.UserAccounts.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"User account {id} was not found.");

    private async Task<TaskType> RequireTaskTypeAsync(string id, CancellationToken cancellationToken) =>
        await db.TaskTypes.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Task type {id} was not found.");

    private async Task<CaseTemplate> RequireCaseTemplateAsync(string id, CancellationToken cancellationToken) =>
        await db.CaseTemplates.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Case template {id} was not found.");

    private async Task<TemplateTask> RequireTemplateTaskAsync(string id, CancellationToken cancellationToken) =>
        await db.TemplateTasks.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Template task {id} was not found.");

    private async Task<Case> RequireCaseAsync(string id, CancellationToken cancellationToken) =>
        await db.Cases.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Case {id} was not found.");

    private async Task<Entities.Task> RequireTaskAsync(string id, CancellationToken cancellationToken) =>
        await db.Tasks.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Task {id} was not found.");

    private async Task<ParticipantSupplier> RequireParticipantSupplierAsync(string id, CancellationToken cancellationToken) =>
        await db.ParticipantSuppliers.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Participant supplier record {id} was not found.");

    private async Task<AccessGrant> RequireAccessGrantAsync(string id, CancellationToken cancellationToken) =>
        await db.AccessGrants.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Access grant {id} was not found.");

    private async Task<RequestForInformation> RequireRequestForInformationAsync(string id, CancellationToken cancellationToken) =>
        await db.RequestsForInformation.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw DomainException.NotFound($"Request for information {id} was not found.");

    private async Task<UserAccount> CreateUserAccountAsync(EntityUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        if (await db.UserAccounts.AnyAsync(account => account.Email.ToLower() == normalizedEmail, cancellationToken))
        {
            throw new DomainException("A user account with this email already exists.");
        }
        var now = Clock();
        var account = new UserAccount
        {
            Id = NextId("user"),
            EntraObjectId = $"entra-{normalizedEmail}",
            Email = normalizedEmail,
            DisplayName = Required(command.DisplayName, "Enter a display name."),
            Status = "ACTIVE",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.UserAccounts.Add(account);
        return account;
    }

    private TMembership NewMembership<TMembership>(string prefix, string entityId, string userAccountId, DateTimeOffset now)
        where TMembership : Membership, new() =>
        new()
        {
            Id = NextId(prefix),
            EntityId = entityId,
            UserAccountId = userAccountId,
            CreatedAt = now,
            UpdatedAt = now,
        };

    private static string Required(string value, string message)
    {
        var trimmed = value.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? throw new DomainException(message) : trimmed;
    }

    private static string AppendStatusHistory(string json, string status, DateTimeOffset at, string byUserId, string note)
    {
        var history = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json) ?? [];
        history.Add(new Dictionary<string, object?> { ["status"] = status, ["at"] = at, ["byUserId"] = byUserId, ["note"] = note });
        return JsonSerializer.Serialize(history);
    }

    private static DateTimeOffset Clock() => DateTimeOffset.UtcNow;

    private static string NextId(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 17, prefix.Length + 33)];
}
