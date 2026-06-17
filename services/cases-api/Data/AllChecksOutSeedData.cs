using AllChecksOut.Cases.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AllChecksOut.Cases.Api.Data;

internal static class AllChecksOutSeedData
{
    private static readonly DateTimeOffset CreatedAt = DateTimeOffset.Parse("2026-01-03T09:00:00.000Z");
    private static readonly DateTimeOffset UpdatedAt = DateTimeOffset.Parse("2026-06-15T09:00:00.000Z");
    private static readonly DateTimeOffset ApprovedAt = DateTimeOffset.Parse("2026-01-20T10:00:00.000Z");
    private static readonly DateTimeOffset RequestedAt = DateTimeOffset.Parse("2026-06-13T10:00:00.000Z");
    private static readonly DateTimeOffset RespondedAt = DateTimeOffset.Parse("2026-06-14T11:30:00.000Z");

    private const string EmptyJson = "{}";
    private const string DefaultTerminologyJson = """
        {"authority":{"singular":"authority","plural":"authorities"},"participant":{"singular":"participant","plural":"participants"},"stakeholder":{"singular":"stakeholder","plural":"stakeholders"},"agent":{"singular":"agent","plural":"agents"},"case":{"singular":"case","plural":"cases"},"caseTemplate":{"singular":"case template","plural":"case templates"},"task":{"singular":"task","plural":"tasks"},"participantSupplier":{"singular":"participant supplier","plural":"participant suppliers"},"evidence":{"singular":"evidence","plural":"evidence"},"accessGrant":{"singular":"access grant","plural":"access grants"},"requestForInformation":{"singular":"request for information","plural":"requests for information"}}
        """;

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemOwner>().HasData(Base(new SystemOwner { Id = "all-checks-out", Name = "All Checks Out Ltd" }));
        modelBuilder.Entity<Authority>().HasData(Base(new Authority
        {
            Id = "northstar-association",
            SystemOwnerId = "all-checks-out",
            Name = "Digital Platform Assurance Association",
            Description = "An authority defining case expectations for member participants.",
        }));
        modelBuilder.Entity<AuthorityTerminology>().HasData(Base(new AuthorityTerminology
        {
            Id = "terminology-northstar-association",
            AuthorityId = "northstar-association",
            LabelsJson = DefaultTerminologyJson,
        }));

        modelBuilder.Entity<Participant>().HasData(
            Participant("northstar-cloud", "Northstar Cloud Platforms"),
            Participant("cobalt-workflow", "Cobalt Workflow Systems"),
            Participant("pinebridge-data", "Pinebridge Data Exchange"),
            Participant("asteria-identity", "Asteria Identity Services"));

        modelBuilder.Entity<Stakeholder>().HasData(
            Stakeholder("harrington-financial", "Harrington Financial Group"),
            Stakeholder("mercury-retail", "Mercury Retail PLC"));

        modelBuilder.Entity<Agent>().HasData(
            Agent("sentinel-grc", "Sentinel GRC Advisory"),
            Agent("ledgerfield-legal", "Ledgerfield Legal LLP"));

        modelBuilder.Entity<UserAccount>().HasData(
            User("user-jonathan-price", "Jonathan Price", "arty.uptick@gmail.com"),
            User("user-amara-singh", "Amara Singh", "amara.singh@dpaa.example"),
            User("user-aisha-khan", "Aisha Khan", "aisha.khan@northstar-cloud.example"),
            User("user-michael-reeves", "Michael Reeves", "michael.reeves@northstar-cloud.example"),
            User("user-lewis-green", "Lewis Green", "lewis.green@cobalt-workflow.example"),
            User("user-amelia-wright", "Amelia Wright", "amelia.wright@cobalt-workflow.example"),
            User("user-maya-patel", "Maya Patel", "maya.patel@pinebridge-data.example"),
            User("user-owen-clarke", "Owen Clarke", "owen.clarke@asteria-identity.example"),
            User("user-rachel-morgan", "Rachel Morgan", "rachel.morgan@harrington.example"),
            User("user-peter-walsh", "Peter Walsh", "peter.walsh@harrington.example"),
            User("user-sophie-turner", "Sophie Turner", "sophie.turner@mercury-retail.example"),
            User("user-benjamin-foster", "Benjamin Foster", "benjamin.foster@mercury-retail.example"),
            User("user-priya-shah", "Priya Shah", "priya.shah@sentinel-grc.example"),
            User("user-george-evans", "George Evans", "george.evans@sentinel-grc.example"),
            User("user-ellen-brooks", "Ellen Brooks", "ellen.brooks@ledgerfield.example"),
            User("user-nadia-cole", "Nadia Cole", "nadia.cole@portfolio.example"));

        modelBuilder.Entity<AuthorityUser>().HasData(
            Membership<AuthorityUser>("authority-user-jonathan-price", "northstar-association", "user-jonathan-price"),
            Membership<AuthorityUser>("authority-user-amara-singh", "northstar-association", "user-amara-singh"));

        modelBuilder.Entity<ParticipantUser>().HasData(
            Membership<ParticipantUser>("participant-user-aisha-khan", "northstar-cloud", "user-aisha-khan"),
            Membership<ParticipantUser>("participant-user-michael-reeves", "northstar-cloud", "user-michael-reeves"),
            Membership<ParticipantUser>("participant-user-lewis-green", "cobalt-workflow", "user-lewis-green"),
            Membership<ParticipantUser>("participant-user-amelia-wright", "cobalt-workflow", "user-amelia-wright"),
            Membership<ParticipantUser>("participant-user-nadia-cole", "cobalt-workflow", "user-nadia-cole"),
            Membership<ParticipantUser>("participant-user-maya-patel", "pinebridge-data", "user-maya-patel"),
            Membership<ParticipantUser>("participant-user-owen-clarke", "asteria-identity", "user-owen-clarke"));

        modelBuilder.Entity<StakeholderUser>().HasData(
            Membership<StakeholderUser>("stakeholder-user-rachel-morgan", "harrington-financial", "user-rachel-morgan"),
            Membership<StakeholderUser>("stakeholder-user-peter-walsh", "harrington-financial", "user-peter-walsh"),
            Membership<StakeholderUser>("stakeholder-user-sophie-turner", "mercury-retail", "user-sophie-turner"),
            Membership<StakeholderUser>("stakeholder-user-benjamin-foster", "mercury-retail", "user-benjamin-foster"));

        modelBuilder.Entity<AgentUser>().HasData(
            Membership<AgentUser>("agent-user-priya-shah", "sentinel-grc", "user-priya-shah"),
            Membership<AgentUser>("agent-user-george-evans", "sentinel-grc", "user-george-evans"),
            Membership<AgentUser>("agent-user-ellen-brooks", "ledgerfield-legal", "user-ellen-brooks"));

        modelBuilder.Entity<StakeholderParticipantAccess>().HasData(
            Access("access-harrington-northstar", "harrington-financial", "northstar-cloud", "user-aisha-khan"),
            Access("access-harrington-cobalt", "harrington-financial", "cobalt-workflow", "user-lewis-green"),
            Access("access-mercury-cobalt", "mercury-retail", "cobalt-workflow", "user-lewis-green"),
            Access("access-mercury-pinebridge", "mercury-retail", "pinebridge-data", "user-maya-patel"));

        modelBuilder.Entity<AccessGrant>().HasData(
            Grant("grant-harrington-northstar", "northstar-cloud", "STAKEHOLDER", "harrington-financial", "REQUEST_INFORMATION", "user-aisha-khan"),
            Grant("grant-harrington-cobalt", "cobalt-workflow", "STAKEHOLDER", "harrington-financial", "REVIEW_AND_COMMENT", "user-lewis-green"),
            Grant("grant-mercury-cobalt", "cobalt-workflow", "STAKEHOLDER", "mercury-retail", "REQUEST_INFORMATION", "user-lewis-green"),
            Grant("grant-mercury-pinebridge", "pinebridge-data", "STAKEHOLDER", "mercury-retail", "REVIEW_AND_COMMENT", "user-maya-patel"),
            Grant("grant-mercury-northstar-stratuspay", "northstar-cloud", "STAKEHOLDER", "mercury-retail", "REQUEST_INFORMATION", "user-aisha-khan", "PARTICIPANT_SUPPLIER", "participant-supplier-northstar-stratuspay"),
            Grant("grant-sentinel-northstar", "northstar-cloud", "AGENT", "sentinel-grc", "CREATE_AND_EDIT", "user-aisha-khan"),
            Grant("grant-sentinel-asteria", "asteria-identity", "AGENT", "sentinel-grc", "REVIEW_AND_COMMENT", "user-owen-clarke"));

        modelBuilder.Entity<ParticipantSupplier>().HasData(
            Supplier("participant-supplier-northstar-stratuspay", "northstar-cloud", "StratusPay Processing", "Payment processing subprocessor", "Hosted payment processing, payment tokenisation, and transaction monitoring for regulated customers.", "Processes production customer identifiers and payment tokens in UK and EU regions."),
            Supplier("participant-supplier-northstar-observiq", "northstar-cloud", "ObservIQ Telemetry", "Monitoring and observability provider", "Infrastructure monitoring, alerting, log aggregation, and incident diagnostics.", "Receives service telemetry and limited diagnostic logs with customer tenant references."),
            Supplier("participant-supplier-cobalt-docuhold", "cobalt-workflow", "DocuHold Archive Services", "Document retention provider", "Long-term document retention and secure archive export for workflow records.", "Stores encrypted customer documents and retention metadata."),
            Supplier("participant-supplier-pinebridge-eu-host", "pinebridge-data", "Azure EU Hosting Operations", "Cloud hosting provider", "Primary database hosting, backup replication, key management integration, and platform telemetry.", "Hosts production customer data and encrypted backups in EU regions."));

        modelBuilder.Entity<TaskType>().HasData(TaskTypes());
        modelBuilder.Entity<CaseTemplate>().HasData(
            CaseTemplate("template-annual-platform-ddq", "Annual Platform Participant Case 2026", "Case"),
            CaseTemplate("template-critical-supplier-ddq", "Critical Supplier Case", "Participant supplier case"));
        modelBuilder.Entity<TemplateTask>().HasData(TemplateTasks());
        modelBuilder.Entity<Case>().HasData(Cases());
        modelBuilder.Entity<CaseTemplateParticipant>().HasData(CaseTemplateParticipants());
        modelBuilder.Entity<AllChecksOut.Cases.Api.Entities.Task>().HasData(Tasks());
        modelBuilder.Entity<StakeholderReview>().HasData(
            Review("review-harrington-northstar", "harrington-financial", "case-2026-northstar", "Security and subprocessor evidence is under procurement review.", "user-rachel-morgan"),
            Review("review-harrington-cobalt", "harrington-financial", "case-2026-cobalt", "Waiting for Cobalt to submit the full Case.", "user-rachel-morgan"),
            Review("review-mercury-cobalt", "mercury-retail", "case-2026-cobalt", "Review started after access grant was accepted.", "user-sophie-turner"),
            Review("review-mercury-pinebridge", "mercury-retail", "case-2026-pinebridge", "Restore-test evidence and certification evidence need clarification.", "user-sophie-turner"));
        modelBuilder.Entity<RequestForInformation>().HasData(RequestsForInformation());
    }

    private static T Base<T>(T entity) where T : Entity
    {
        entity.CreatedAt = CreatedAt;
        entity.UpdatedAt = UpdatedAt;
        return entity;
    }

    private static Participant Participant(string id, string displayName) => Base(new Participant { Id = id, AuthorityId = "northstar-association", DisplayName = displayName });
    private static Stakeholder Stakeholder(string id, string displayName) => Base(new Stakeholder { Id = id, AuthorityId = "northstar-association", DisplayName = displayName });
    private static Agent Agent(string id, string displayName) => Base(new Agent { Id = id, AuthorityId = "northstar-association", DisplayName = displayName });
    private static UserAccount User(string id, string displayName, string email) => Base(new UserAccount { Id = id, EntraObjectId = $"entra-{id}", DisplayName = displayName, Email = email, Status = "ACTIVE" });
    private static T Membership<T>(string id, string entityId, string userAccountId) where T : Membership, new() => Base(new T { Id = id, EntityId = entityId, UserAccountId = userAccountId });
    private static CaseTemplate CaseTemplate(string id, string name, string description) => Base(new CaseTemplate { Id = id, AuthorityId = "northstar-association", Name = name, Description = description, Status = "FINALIZED" });

    private static StakeholderParticipantAccess Access(string id, string stakeholderId, string participantId, string approvedByUserId) => Base(new StakeholderParticipantAccess
    {
        Id = id,
        StakeholderId = stakeholderId,
        ParticipantId = participantId,
        ApprovedByUserId = approvedByUserId,
        ApprovedAt = ApprovedAt,
    });

    private static AccessGrant Grant(string id, string participantId, string granteeType, string granteeEntityId, string permissionLevel, string createdByUserId, string dataScopeType = "PARTICIPANT", string? dataScopeId = null) => Base(new AccessGrant
    {
        Id = id,
        AuthorityId = "northstar-association",
        ParticipantId = participantId,
        GranteeType = granteeType,
        GranteeStakeholderId = granteeType == "STAKEHOLDER" ? granteeEntityId : null,
        GranteeAgentId = granteeType == "AGENT" ? granteeEntityId : null,
        GranteeUserId = null,
        PermissionLevel = permissionLevel,
        DataScopeType = dataScopeType,
        DataScopeId = dataScopeId,
        Status = "ACTIVE",
        CreatedByUserId = createdByUserId,
        ExpiresAt = null,
    });

    private static ParticipantSupplier Supplier(string id, string participantId, string name, string relationship, string services, string exposure) => Base(new ParticipantSupplier
    {
        Id = id,
        AuthorityId = "northstar-association",
        ParticipantId = participantId,
        SupplierName = name,
        RelationshipType = relationship,
        ServicesProvided = services,
        DataExposure = exposure,
    });

    private static TaskType[] TaskTypes() =>
    [
        TaskType("task-type-policy-document", "POLICY_DOCUMENT", "Policy document upload", "Upload a controlled policy, plan, or governance document."),
        TaskType("task-type-certification", "CERTIFICATION_EVIDENCE", "Certification evidence", "Upload ISO 27001, SOC 2, Cyber Essentials, or equivalent assurance evidence."),
        TaskType("task-type-questionnaire", "QUESTIONNAIRE", "Questionnaire", "Answer structured case questions."),
        TaskType("task-type-control-attestation", "CONTROL_ATTESTATION", "Control attestation", "Confirm that a required control exists and is operating."),
        TaskType("task-type-supplier-register", "SUPPLIER_REGISTER", "Supplier register", "List subprocessors, hosting providers, support tools, and critical suppliers."),
        TaskType("task-type-participant-ddq", "PARTICIPANT_SUPPLIER_CASE", "Participant supplier case", "Record case on a critical supplier or participant supplier."),
        TaskType("task-type-risk-register", "RISK_REGISTER", "Risk and remediation register", "Record issues, owners, mitigation dates, and residual risk."),
        TaskType("task-type-upload-document", "UPLOAD_DOCUMENT", "Evidence metadata upload", "Record uploaded evidence metadata in this frontend phase."),
        TaskType("task-type-signature", "DIGITAL_SIGNATURE", "Digital signature", "Capture a senior officer attestation."),
        TaskType("task-type-ai-disclosure", "AI_USAGE_DISCLOSURE", "AI usage disclosure", "Declare whether AI services process customer data and how they are controlled."),
        TaskType("task-type-access-control", "ACCESS_CONTROL_MFA", "Access control and MFA", "Confirm MFA, privileged access, and joiner/mover/leaver controls."),
        TaskType("task-type-hosting-residency", "HOSTING_RESIDENCY", "Hosting and data residency", "Document hosting provider, regions, backup locations, and customer-data residency."),
    ];

    private static TaskType TaskType(string id, string code, string name, string description) => Base(new TaskType { Id = id, Code = code, Name = name, Description = description, ParameterSchemaJson = EmptyJson, Status = "ACTIVE" });

    private static TemplateTask[] TemplateTasks() =>
    [
        TemplateTask("template-task-company-profile", "template-annual-platform-ddq", "task-type-questionnaire", "Company profile and platform summary", "Confirm company details, platform summary, core services, and regulated customer sectors.", 1, "18 Jun 2026"),
        TemplateTask("template-task-security-policy", "template-annual-platform-ddq", "task-type-policy-document", "Information security policy", "Upload the current information security policy and confirm senior management approval date.", 2, "19 Jun 2026"),
        TemplateTask("template-task-certification", "template-annual-platform-ddq", "task-type-certification", "ISO 27001 or SOC 2 evidence", "Upload current ISO 27001 certificate, SOC 2 Type II report, Cyber Essentials certificate, or equivalent controls evidence.", 3, "20 Jun 2026"),
        TemplateTask("template-task-penetration-test", "template-annual-platform-ddq", "task-type-upload-document", "Penetration test summary", "Provide latest penetration test executive summary, provider name, test date, and remediation status.", 4, "21 Jun 2026"),
        TemplateTask("template-task-vulnerability-management", "template-annual-platform-ddq", "task-type-questionnaire", "Vulnerability management process", "Describe scanning cadence, severity thresholds, patching SLAs, and exception governance.", 5, "21 Jun 2026"),
        TemplateTask("template-task-access-control", "template-annual-platform-ddq", "task-type-access-control", "Access control and MFA attestation", "Confirm MFA enforcement, privileged access controls, joiner/mover/leaver process, and access review cadence.", 6, "22 Jun 2026"),
        TemplateTask("template-task-data-protection", "template-annual-platform-ddq", "task-type-questionnaire", "Data protection registration", "Provide ICO or equivalent data protection registration details and data protection officer contact.", 7, "23 Jun 2026"),
        TemplateTask("template-task-dpa", "template-annual-platform-ddq", "task-type-policy-document", "GDPR data processing agreement", "Upload standard DPA and describe controller/processor responsibilities for customer data.", 8, "23 Jun 2026"),
        TemplateTask("template-task-subprocessor-register", "template-annual-platform-ddq", "task-type-supplier-register", "Subprocessor register", "List subprocessors, hosting providers, support tools, analytics services, and monitoring providers.", 9, "24 Jun 2026"),
        TemplateTask("template-task-hosting-residency", "template-annual-platform-ddq", "task-type-hosting-residency", "Hosting and data residency statement", "State hosting provider, primary regions, backup regions, and whether customer data leaves the UK or EU.", 10, "24 Jun 2026"),
        TemplateTask("template-task-backup-restore", "template-annual-platform-ddq", "task-type-upload-document", "Backup and restore evidence", "Describe backup frequency, retention, encryption, and upload most recent restore-test evidence.", 11, "25 Jun 2026"),
        TemplateTask("template-task-bcp-dr", "template-annual-platform-ddq", "task-type-policy-document", "Business continuity and disaster recovery plan", "Upload BCP or disaster recovery plan and confirm last test date.", 12, "25 Jun 2026"),
        TemplateTask("template-task-incident-response", "template-annual-platform-ddq", "task-type-policy-document", "Incident response plan", "Upload incident response plan and describe breach notification procedure.", 13, "26 Jun 2026"),
        TemplateTask("template-task-cyber-insurance", "template-annual-platform-ddq", "task-type-certification", "Cyber insurance evidence", "Upload cyber insurance certificate and coverage summary.", 14, "26 Jun 2026"),
        TemplateTask("template-task-secure-development", "template-annual-platform-ddq", "task-type-questionnaire", "Secure development lifecycle questionnaire", "Describe secure coding, code review, dependency scanning, release controls, and segregation of duties.", 15, "27 Jun 2026"),
        TemplateTask("template-task-ai-disclosure", "template-annual-platform-ddq", "task-type-ai-disclosure", "AI usage and customer-data disclosure", "Declare whether AI services process customer data, which providers are used, and what controls apply.", 16, "27 Jun 2026"),
        TemplateTask("template-task-critical-supplier", "template-annual-platform-ddq", "task-type-participant-ddq", "Critical supplier and participant supplier Case", "Identify critical suppliers and complete case for material third-party dependencies.", 17, "28 Jun 2026"),
        TemplateTask("template-task-senior-attestation", "template-annual-platform-ddq", "task-type-signature", "Senior officer attestation and signature", "A senior officer confirms the submitted information is accurate and complete.", 18, "30 Jun 2026"),
        TemplateTask("template-task-supplier-profile", "template-critical-supplier-ddq", "task-type-questionnaire", "Supplier profile", "Record the supplier relationship, services provided, and customer-data exposure.", 1, "No due date"),
        TemplateTask("template-task-supplier-controls", "template-critical-supplier-ddq", "task-type-control-attestation", "Supplier control attestation", "Confirm key security and resilience controls for the critical supplier.", 2, "No due date"),
        TemplateTask("template-task-supplier-risk", "template-critical-supplier-ddq", "task-type-risk-register", "Supplier risk and remediation", "Record known supplier risks, mitigation owners, and target remediation dates.", 3, "No due date"),
    ];

    private static TemplateTask TemplateTask(string id, string caseTemplateId, string taskTypeId, string title, string description, int sortOrder, string due) => Base(new TemplateTask
    {
        Id = id,
        CaseTemplateId = caseTemplateId,
        TaskTypeId = taskTypeId,
        Title = title,
        Description = description,
        ParametersJson = $$"""{"due":"{{due}}"}""",
        SortOrder = sortOrder,
        Status = "ACTIVE",
    });

    private static Case[] Cases() =>
    [
        Case("case-2026-northstar", "template-annual-platform-ddq", "northstar-cloud", null, "COMPLETE", "2026-06-12T15:30:00.000Z", null),
        Case("case-2026-cobalt", "template-annual-platform-ddq", "cobalt-workflow", null, "INCOMPLETE", null, null),
        Case("case-2026-pinebridge", "template-annual-platform-ddq", "pinebridge-data", null, "INCOMPLETE", "2026-06-10T12:00:00.000Z", null),
        Case("case-2026-asteria", "template-annual-platform-ddq", "asteria-identity", null, "COMPLETE", "2026-06-05T11:15:00.000Z", "2026-06-14T09:30:00.000Z"),
        Case("case-supplier-northstar-stratuspay", "template-critical-supplier-ddq", "northstar-cloud", "participant-supplier-northstar-stratuspay", "INCOMPLETE", null, null),
    ];

    private static Case Case(string id, string caseTemplateId, string participantId, string? supplierId, string status, string? submittedAt, string? closedAt) => Base(new Case
    {
        Id = id,
        AuthorityId = "northstar-association",
        CaseTemplateId = caseTemplateId,
        ParticipantId = participantId,
        ParticipantSupplierId = supplierId,
        Status = status,
        SubmittedAt = Parse(submittedAt),
        ClosedAt = Parse(closedAt),
    });

    private static CaseTemplateParticipant[] CaseTemplateParticipants() =>
    [
        TemplateParticipant("template-participant-northstar", "template-annual-platform-ddq", "northstar-cloud", "case-2026-northstar"),
        TemplateParticipant("template-participant-cobalt", "template-annual-platform-ddq", "cobalt-workflow", "case-2026-cobalt"),
        TemplateParticipant("template-participant-pinebridge", "template-annual-platform-ddq", "pinebridge-data", "case-2026-pinebridge"),
        TemplateParticipant("template-participant-asteria", "template-annual-platform-ddq", "asteria-identity", "case-2026-asteria"),
        TemplateParticipant("template-participant-northstar-stratuspay", "template-critical-supplier-ddq", "northstar-cloud", "case-supplier-northstar-stratuspay"),
    ];

    private static CaseTemplateParticipant TemplateParticipant(string id, string caseTemplateId, string participantId, string caseId) => Base(new CaseTemplateParticipant
    {
        Id = id,
        CaseTemplateId = caseTemplateId,
        ParticipantId = participantId,
        CaseId = caseId,
    });

    private static AllChecksOut.Cases.Api.Entities.Task[] Tasks()
    {
        var annualTemplateTaskIds = new[]
        {
            "company-profile", "security-policy", "certification", "penetration-test", "vulnerability-management", "access-control", "data-protection", "dpa",
            "subprocessor-register", "hosting-residency", "backup-restore", "bcp-dr", "incident-response", "cyber-insurance", "secure-development", "ai-disclosure",
            "critical-supplier", "senior-attestation",
        };
        var northstar = new[] { "PASSED", "PASSED", "PASSED", "SUBMITTED", "SUBMITTED", "PASSED", "PASSED", "PASSED", "SUBMITTED", "PASSED", "SUBMITTED", "PASSED", "PASSED", "PASSED", "SUBMITTED", "PASSED", "IN_PROGRESS", "SUBMITTED" };
        var cobalt = new[] { "PASSED", "PASSED", "IN_PROGRESS", "NOT_STARTED", "IN_PROGRESS", "PASSED", "PASSED", "IN_PROGRESS", "NOT_STARTED", "IN_PROGRESS", "NOT_STARTED", "IN_PROGRESS", "IN_PROGRESS", "NOT_STARTED", "IN_PROGRESS", "NOT_STARTED", "NOT_STARTED", "NOT_STARTED" };
        var pinebridge = new[] { "PASSED", "PASSED", "FAILED", "FAILED", "SUBMITTED", "PASSED", "PASSED", "PASSED", "SUBMITTED", "SUBMITTED", "FAILED", "SUBMITTED", "PASSED", "PASSED", "SUBMITTED", "SUBMITTED", "FAILED", "SUBMITTED" };
        var asteria = Enumerable.Repeat("PASSED", annualTemplateTaskIds.Length).ToArray();

        return BuildAnnualTasks("northstar", "case-2026-northstar", annualTemplateTaskIds, northstar)
            .Concat(BuildAnnualTasks("cobalt", "case-2026-cobalt", annualTemplateTaskIds, cobalt))
            .Concat(BuildAnnualTasks("pinebridge", "case-2026-pinebridge", annualTemplateTaskIds, pinebridge))
            .Concat(BuildAnnualTasks("asteria", "case-2026-asteria", annualTemplateTaskIds, asteria))
            .Concat([
                Task("task-stratuspay-profile", "case-supplier-northstar-stratuspay", "template-task-supplier-profile", "PASSED"),
                Task("task-stratuspay-controls", "case-supplier-northstar-stratuspay", "template-task-supplier-controls", "IN_PROGRESS"),
                Task("task-stratuspay-risk", "case-supplier-northstar-stratuspay", "template-task-supplier-risk", "NOT_STARTED"),
            ])
            .ToArray();
    }

    private static IEnumerable<AllChecksOut.Cases.Api.Entities.Task> BuildAnnualTasks(string participantPrefix, string caseId, string[] templateTaskIds, string[] statuses)
    {
        for (var i = 0; i < templateTaskIds.Length; i += 1)
        {
            var taskId = templateTaskIds[i] switch
            {
                "company-profile" => $"task-{participantPrefix}-profile",
                "vulnerability-management" => $"task-{participantPrefix}-vulnerability",
                "subprocessor-register" => $"task-{participantPrefix}-subprocessors",
                "hosting-residency" => $"task-{participantPrefix}-hosting",
                "backup-restore" => $"task-{participantPrefix}-backup",
                "bcp-dr" => $"task-{participantPrefix}-bcp",
                "incident-response" => $"task-{participantPrefix}-incident",
                "cyber-insurance" => $"task-{participantPrefix}-insurance",
                "secure-development" => $"task-{participantPrefix}-sdlc",
                "ai-disclosure" => $"task-{participantPrefix}-ai",
                "senior-attestation" => $"task-{participantPrefix}-attestation",
                _ => $"task-{participantPrefix}-{templateTaskIds[i]}",
            };
            yield return Task(taskId, caseId, $"template-task-{templateTaskIds[i]}", statuses[i]);
        }
    }

    private static AllChecksOut.Cases.Api.Entities.Task Task(string id, string caseId, string templateTaskId, string status) => Base(new AllChecksOut.Cases.Api.Entities.Task
    {
        Id = id,
        CaseId = caseId,
        TemplateTaskId = templateTaskId,
        Status = status,
        ResponseJson = EmptyJson,
        EvidenceJson = EmptyJson,
    });

    private static StakeholderReview Review(string id, string stakeholderId, string caseId, string note, string userId) => Base(new StakeholderReview
    {
        Id = id,
        StakeholderId = stakeholderId,
        CaseId = caseId,
        Note = note,
        ReviewedByUserId = userId,
        ReviewedAt = UpdatedAt,
    });

    private static RequestForInformation[] RequestsForInformation() =>
    [
        Rfi("rfi-harrington-northstar-subprocessors", "harrington-financial", "northstar-cloud", "case-2026-northstar", "task-northstar-subprocessors", "Please confirm whether the EU monitoring provider listed in the subprocessor register has access to production customer data.", "user-rachel-morgan", "OPEN"),
        Rfi("rfi-mercury-pinebridge-restore", "mercury-retail", "pinebridge-data", "case-2026-pinebridge", "task-pinebridge-backup", "The restore-test evidence is older than the current policy cycle. Please provide a 2026 restore-test summary or explain the gap.", "user-sophie-turner", "ANSWERED", "The May 2026 restore-test summary has been added to the evidence metadata and the exception is tracked in the remediation register.", "user-maya-patel"),
        Rfi("rfi-mercury-cobalt-pack", "mercury-retail", "cobalt-workflow", "case-2026-cobalt", null, "When do you expect to submit the remaining certification and incident response evidence for stakeholder review?", "user-sophie-turner", "IN_PROGRESS", "Certification evidence is being validated by the audit team. We expect to submit the remaining items by 21 June.", "user-lewis-green"),
    ];

    private static RequestForInformation Rfi(string id, string stakeholderId, string participantId, string caseId, string? taskId, string requestText, string requestedByUserId, string status, string responseText = "", string? respondedByUserId = null) => Base(new RequestForInformation
    {
        Id = id,
        AuthorityId = "northstar-association",
        ParticipantId = participantId,
        StakeholderId = stakeholderId,
        CaseId = caseId,
        TaskId = taskId,
        RequestText = requestText,
        ResponseText = responseText,
        Status = status,
        RequestedByUserId = requestedByUserId,
        AssignedToUserId = respondedByUserId,
        RespondedByUserId = respondedByUserId,
        RequestedAt = RequestedAt,
        RespondedAt = respondedByUserId is null ? null : RespondedAt,
        StatusHistoryJson = respondedByUserId is null
            ? $$"""[{"status":"OPEN","at":"{{RequestedAt:O}}","byUserId":"{{requestedByUserId}}","note":"Request created"}]"""
            : $$"""[{"status":"OPEN","at":"{{RequestedAt:O}}","byUserId":"{{requestedByUserId}}","note":"Request created"},{"status":"{{status}}","at":"{{RespondedAt:O}}","byUserId":"{{respondedByUserId}}","note":"Participant response updated"}]""",
    });

    private static DateTimeOffset? Parse(string? value) => value is null ? null : DateTimeOffset.Parse(value);
}
