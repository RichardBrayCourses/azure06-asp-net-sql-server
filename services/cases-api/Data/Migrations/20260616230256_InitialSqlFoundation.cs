using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AllChecksOut.Cases.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cases");

            migrationBuilder.CreateTable(
                name: "AccessGrants",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GranteeType = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GranteeStakeholderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    GranteeAgentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    GranteeUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PermissionLevel = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DataScopeType = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DataScopeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentUsers",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authorities",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SystemOwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorityTerminologies",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LabelsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityTerminologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorityUsers",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CaseTemplateId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParticipantSupplierId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WithdrawnAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WithdrawnByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    WithdrawnReason = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseTemplateParticipants",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CaseTemplateId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CaseId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ExemptionReason = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecidedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseTemplateParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseTemplates",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantSuppliers",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ServicesProvided = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DataExposure = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantSuppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantUsers",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestsForInformation",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StakeholderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CaseId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TaskId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RequestText = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ResponseText = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RespondedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StatusHistoryJson = table.Column<string>(type: "nvarchar(max)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestsForInformation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderParticipantAccesses",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StakeholderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderParticipantAccesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderReviews",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StakeholderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CaseId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stakeholders",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stakeholders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderUsers",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemOwners",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOwners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemOwnerUsers",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOwnerUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CaseId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TemplateTaskId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", maxLength: 450, nullable: false),
                    EvidenceJson = table.Column<string>(type: "nvarchar(max)", maxLength: 450, nullable: false),
                    WithdrawnAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskTypes",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParameterSchemaJson = table.Column<string>(type: "nvarchar(max)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateTasks",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CaseTemplateId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TaskTypeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", maxLength: 450, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    WithdrawnReason = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    WithdrawnAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WithdrawnByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                schema: "cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntraObjectId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AccessGrants",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CreatedAt", "CreatedByUserId", "DataScopeId", "DataScopeType", "ExpiresAt", "GranteeAgentId", "GranteeStakeholderId", "GranteeType", "GranteeUserId", "ParticipantId", "PermissionLevel", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { "grant-harrington-cobalt", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-lewis-green", null, "PARTICIPANT", null, null, "harrington-financial", "STAKEHOLDER", null, "cobalt-workflow", "REVIEW_AND_COMMENT", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "grant-harrington-northstar", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-aisha-khan", null, "PARTICIPANT", null, null, "harrington-financial", "STAKEHOLDER", null, "northstar-cloud", "REQUEST_INFORMATION", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "grant-mercury-cobalt", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-lewis-green", null, "PARTICIPANT", null, null, "mercury-retail", "STAKEHOLDER", null, "cobalt-workflow", "REQUEST_INFORMATION", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "grant-mercury-northstar-stratuspay", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-aisha-khan", "participant-supplier-northstar-stratuspay", "PARTICIPANT_SUPPLIER", null, null, "mercury-retail", "STAKEHOLDER", null, "northstar-cloud", "REQUEST_INFORMATION", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "grant-mercury-pinebridge", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-maya-patel", null, "PARTICIPANT", null, null, "mercury-retail", "STAKEHOLDER", null, "pinebridge-data", "REVIEW_AND_COMMENT", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "grant-sentinel-asteria", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-owen-clarke", null, "PARTICIPANT", null, "sentinel-grc", null, "AGENT", null, "asteria-identity", "REVIEW_AND_COMMENT", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "grant-sentinel-northstar", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-aisha-khan", null, "PARTICIPANT", null, "sentinel-grc", null, "AGENT", null, "northstar-cloud", "CREATE_AND_EDIT", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "AgentUsers",
                schema: "cases",
                columns: new[] { "Id", "CreatedAt", "EntityId", "UpdatedAt", "UserAccountId" },
                values: new object[,]
                {
                    { "agent-user-ellen-brooks", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ledgerfield-legal", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-ellen-brooks" },
                    { "agent-user-george-evans", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "sentinel-grc", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-george-evans" },
                    { "agent-user-priya-shah", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "sentinel-grc", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-priya-shah" }
                });

            migrationBuilder.InsertData(
                table: "Agents",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CreatedAt", "DisplayName", "UpdatedAt" },
                values: new object[,]
                {
                    { "ledgerfield-legal", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Ledgerfield Legal LLP", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "sentinel-grc", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sentinel GRC Advisory", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "Authorities",
                schema: "cases",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "SystemOwnerId", "UpdatedAt" },
                values: new object[] { "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "An authority defining case expectations for member participants.", "Digital Platform Assurance Association", "all-checks-out", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "AuthorityTerminologies",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CreatedAt", "LabelsJson", "UpdatedAt" },
                values: new object[] { "terminology-northstar-association", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{\"authority\":{\"singular\":\"authority\",\"plural\":\"authorities\"},\"participant\":{\"singular\":\"participant\",\"plural\":\"participants\"},\"stakeholder\":{\"singular\":\"stakeholder\",\"plural\":\"stakeholders\"},\"agent\":{\"singular\":\"agent\",\"plural\":\"agents\"},\"case\":{\"singular\":\"case\",\"plural\":\"cases\"},\"caseTemplate\":{\"singular\":\"case template\",\"plural\":\"case templates\"},\"task\":{\"singular\":\"task\",\"plural\":\"tasks\"},\"participantSupplier\":{\"singular\":\"participant supplier\",\"plural\":\"participant suppliers\"},\"evidence\":{\"singular\":\"evidence\",\"plural\":\"evidence\"},\"accessGrant\":{\"singular\":\"access grant\",\"plural\":\"access grants\"},\"requestForInformation\":{\"singular\":\"request for information\",\"plural\":\"requests for information\"}}", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "AuthorityUsers",
                schema: "cases",
                columns: new[] { "Id", "CreatedAt", "EntityId", "UpdatedAt", "UserAccountId" },
                values: new object[,]
                {
                    { "authority-user-amara-singh", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-association", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-amara-singh" },
                    { "authority-user-jonathan-price", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-association", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-jonathan-price" }
                });

            migrationBuilder.InsertData(
                table: "CaseTemplateParticipants",
                schema: "cases",
                columns: new[] { "Id", "CaseId", "CaseTemplateId", "CreatedAt", "DecidedAt", "DecidedByUserId", "ExemptionReason", "ParticipantId", "UpdatedAt" },
                values: new object[,]
                {
                    { "template-participant-asteria", "case-2026-asteria", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "asteria-identity", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "template-participant-cobalt", "case-2026-cobalt", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "cobalt-workflow", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "template-participant-northstar", "case-2026-northstar", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "northstar-cloud", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "template-participant-northstar-stratuspay", "case-supplier-northstar-stratuspay", "template-critical-supplier-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "northstar-cloud", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "template-participant-pinebridge", "case-2026-pinebridge", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "pinebridge-data", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "CaseTemplates",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CreatedAt", "Description", "Name", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { "template-annual-platform-ddq", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Case", "Annual Platform Participant Case 2026", "FINALIZED", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "template-critical-supplier-ddq", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Participant supplier case", "Critical Supplier Case", "FINALIZED", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "Cases",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CaseTemplateId", "ClosedAt", "CreatedAt", "ParticipantId", "ParticipantSupplierId", "Status", "SubmittedAt", "UpdatedAt", "WithdrawnAt", "WithdrawnByUserId", "WithdrawnReason" },
                values: new object[,]
                {
                    { "case-2026-asteria", "northstar-association", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 6, 14, 9, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "asteria-identity", null, "COMPLETE", new DateTimeOffset(new DateTime(2026, 6, 5, 11, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "case-2026-cobalt", "northstar-association", "template-annual-platform-ddq", null, new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cobalt-workflow", null, "INCOMPLETE", null, new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "case-2026-northstar", "northstar-association", "template-annual-platform-ddq", null, new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-cloud", null, "COMPLETE", new DateTimeOffset(new DateTime(2026, 6, 12, 15, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "case-2026-pinebridge", "northstar-association", "template-annual-platform-ddq", null, new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "pinebridge-data", null, "INCOMPLETE", new DateTimeOffset(new DateTime(2026, 6, 10, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "case-supplier-northstar-stratuspay", "northstar-association", "template-critical-supplier-ddq", null, new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-cloud", "participant-supplier-northstar-stratuspay", "INCOMPLETE", null, new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null }
                });

            migrationBuilder.InsertData(
                table: "ParticipantSuppliers",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CreatedAt", "DataExposure", "ParticipantId", "RelationshipType", "ServicesProvided", "SupplierName", "UpdatedAt" },
                values: new object[,]
                {
                    { "participant-supplier-cobalt-docuhold", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Stores encrypted customer documents and retention metadata.", "cobalt-workflow", "Document retention provider", "Long-term document retention and secure archive export for workflow records.", "DocuHold Archive Services", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "participant-supplier-northstar-observiq", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Receives service telemetry and limited diagnostic logs with customer tenant references.", "northstar-cloud", "Monitoring and observability provider", "Infrastructure monitoring, alerting, log aggregation, and incident diagnostics.", "ObservIQ Telemetry", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "participant-supplier-northstar-stratuspay", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Processes production customer identifiers and payment tokens in UK and EU regions.", "northstar-cloud", "Payment processing subprocessor", "Hosted payment processing, payment tokenisation, and transaction monitoring for regulated customers.", "StratusPay Processing", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "participant-supplier-pinebridge-eu-host", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Hosts production customer data and encrypted backups in EU regions.", "pinebridge-data", "Cloud hosting provider", "Primary database hosting, backup replication, key management integration, and platform telemetry.", "Azure EU Hosting Operations", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "ParticipantUsers",
                schema: "cases",
                columns: new[] { "Id", "CreatedAt", "EntityId", "UpdatedAt", "UserAccountId" },
                values: new object[,]
                {
                    { "participant-user-aisha-khan", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-cloud", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-aisha-khan" },
                    { "participant-user-amelia-wright", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cobalt-workflow", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-amelia-wright" },
                    { "participant-user-lewis-green", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cobalt-workflow", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-lewis-green" },
                    { "participant-user-maya-patel", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "pinebridge-data", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-maya-patel" },
                    { "participant-user-michael-reeves", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-cloud", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-michael-reeves" },
                    { "participant-user-nadia-cole", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cobalt-workflow", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-nadia-cole" },
                    { "participant-user-owen-clarke", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "asteria-identity", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-owen-clarke" }
                });

            migrationBuilder.InsertData(
                table: "Participants",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CreatedAt", "DisplayName", "UpdatedAt" },
                values: new object[,]
                {
                    { "asteria-identity", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Asteria Identity Services", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "cobalt-workflow", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cobalt Workflow Systems", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "northstar-cloud", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Northstar Cloud Platforms", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "pinebridge-data", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Pinebridge Data Exchange", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "RequestsForInformation",
                schema: "cases",
                columns: new[] { "Id", "AssignedToUserId", "AuthorityId", "CaseId", "CreatedAt", "ParticipantId", "RequestText", "RequestedAt", "RequestedByUserId", "RespondedAt", "RespondedByUserId", "ResponseText", "StakeholderId", "Status", "StatusHistoryJson", "TaskId", "UpdatedAt" },
                values: new object[,]
                {
                    { "rfi-harrington-northstar-subprocessors", null, "northstar-association", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-cloud", "Please confirm whether the EU monitoring provider listed in the subprocessor register has access to production customer data.", new DateTimeOffset(new DateTime(2026, 6, 13, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-rachel-morgan", null, null, "", "harrington-financial", "OPEN", "[{\"status\":\"OPEN\",\"at\":\"2026-06-13T10:00:00.0000000+00:00\",\"byUserId\":\"user-rachel-morgan\",\"note\":\"Request created\"}]", "task-northstar-subprocessors", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "rfi-mercury-cobalt-pack", "user-lewis-green", "northstar-association", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cobalt-workflow", "When do you expect to submit the remaining certification and incident response evidence for stakeholder review?", new DateTimeOffset(new DateTime(2026, 6, 13, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-sophie-turner", new DateTimeOffset(new DateTime(2026, 6, 14, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-lewis-green", "Certification evidence is being validated by the audit team. We expect to submit the remaining items by 21 June.", "mercury-retail", "IN_PROGRESS", "[{\"status\":\"OPEN\",\"at\":\"2026-06-13T10:00:00.0000000+00:00\",\"byUserId\":\"user-sophie-turner\",\"note\":\"Request created\"},{\"status\":\"IN_PROGRESS\",\"at\":\"2026-06-14T11:30:00.0000000+00:00\",\"byUserId\":\"user-lewis-green\",\"note\":\"Participant response updated\"}]", null, new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "rfi-mercury-pinebridge-restore", "user-maya-patel", "northstar-association", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "pinebridge-data", "The restore-test evidence is older than the current policy cycle. Please provide a 2026 restore-test summary or explain the gap.", new DateTimeOffset(new DateTime(2026, 6, 13, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-sophie-turner", new DateTimeOffset(new DateTime(2026, 6, 14, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-maya-patel", "The May 2026 restore-test summary has been added to the evidence metadata and the exception is tracked in the remediation register.", "mercury-retail", "ANSWERED", "[{\"status\":\"OPEN\",\"at\":\"2026-06-13T10:00:00.0000000+00:00\",\"byUserId\":\"user-sophie-turner\",\"note\":\"Request created\"},{\"status\":\"ANSWERED\",\"at\":\"2026-06-14T11:30:00.0000000+00:00\",\"byUserId\":\"user-maya-patel\",\"note\":\"Participant response updated\"}]", "task-pinebridge-backup", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "StakeholderParticipantAccesses",
                schema: "cases",
                columns: new[] { "Id", "ApprovedAt", "ApprovedByUserId", "CreatedAt", "ParticipantId", "StakeholderId", "UpdatedAt" },
                values: new object[,]
                {
                    { "access-harrington-cobalt", new DateTimeOffset(new DateTime(2026, 1, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-lewis-green", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cobalt-workflow", "harrington-financial", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "access-harrington-northstar", new DateTimeOffset(new DateTime(2026, 1, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-aisha-khan", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "northstar-cloud", "harrington-financial", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "access-mercury-cobalt", new DateTimeOffset(new DateTime(2026, 1, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-lewis-green", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cobalt-workflow", "mercury-retail", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "access-mercury-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-maya-patel", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "pinebridge-data", "mercury-retail", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "StakeholderReviews",
                schema: "cases",
                columns: new[] { "Id", "CaseId", "CreatedAt", "Note", "ReviewedAt", "ReviewedByUserId", "StakeholderId", "UpdatedAt" },
                values: new object[,]
                {
                    { "review-harrington-cobalt", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Waiting for Cobalt to submit the full Case.", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-rachel-morgan", "harrington-financial", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "review-harrington-northstar", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Security and subprocessor evidence is under procurement review.", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-rachel-morgan", "harrington-financial", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "review-mercury-cobalt", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Review started after access grant was accepted.", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-sophie-turner", "mercury-retail", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "review-mercury-pinebridge", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Restore-test evidence and certification evidence need clarification.", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-sophie-turner", "mercury-retail", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "StakeholderUsers",
                schema: "cases",
                columns: new[] { "Id", "CreatedAt", "EntityId", "UpdatedAt", "UserAccountId" },
                values: new object[,]
                {
                    { "stakeholder-user-benjamin-foster", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "mercury-retail", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-benjamin-foster" },
                    { "stakeholder-user-peter-walsh", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "harrington-financial", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-peter-walsh" },
                    { "stakeholder-user-rachel-morgan", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "harrington-financial", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-rachel-morgan" },
                    { "stakeholder-user-sophie-turner", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "mercury-retail", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user-sophie-turner" }
                });

            migrationBuilder.InsertData(
                table: "Stakeholders",
                schema: "cases",
                columns: new[] { "Id", "AuthorityId", "CreatedAt", "DisplayName", "UpdatedAt" },
                values: new object[,]
                {
                    { "harrington-financial", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Harrington Financial Group", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "mercury-retail", "northstar-association", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Mercury Retail PLC", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "SystemOwners",
                schema: "cases",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { "all-checks-out", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "All Checks Out Ltd", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "TaskTypes",
                schema: "cases",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "Name", "ParameterSchemaJson", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { "task-type-access-control", "ACCESS_CONTROL_MFA", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Confirm MFA, privileged access, and joiner/mover/leaver controls.", "Access control and MFA", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-ai-disclosure", "AI_USAGE_DISCLOSURE", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Declare whether AI services process customer data and how they are controlled.", "AI usage disclosure", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-certification", "CERTIFICATION_EVIDENCE", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload ISO 27001, SOC 2, Cyber Essentials, or equivalent assurance evidence.", "Certification evidence", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-control-attestation", "CONTROL_ATTESTATION", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Confirm that a required control exists and is operating.", "Control attestation", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-hosting-residency", "HOSTING_RESIDENCY", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Document hosting provider, regions, backup locations, and customer-data residency.", "Hosting and data residency", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-participant-ddq", "PARTICIPANT_SUPPLIER_CASE", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Record case on a critical supplier or participant supplier.", "Participant supplier case", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-policy-document", "POLICY_DOCUMENT", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload a controlled policy, plan, or governance document.", "Policy document upload", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-questionnaire", "QUESTIONNAIRE", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Answer structured case questions.", "Questionnaire", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-risk-register", "RISK_REGISTER", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Record issues, owners, mitigation dates, and residual risk.", "Risk and remediation register", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-signature", "DIGITAL_SIGNATURE", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Capture a senior officer attestation.", "Digital signature", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-supplier-register", "SUPPLIER_REGISTER", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "List subprocessors, hosting providers, support tools, and critical suppliers.", "Supplier register", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "task-type-upload-document", "UPLOAD_DOCUMENT", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Record uploaded evidence metadata in this frontend phase.", "Evidence metadata upload", "{}", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                schema: "cases",
                columns: new[] { "Id", "CaseId", "CreatedAt", "EvidenceJson", "ResponseJson", "Status", "TemplateTaskId", "UpdatedAt", "WithdrawnAt" },
                values: new object[,]
                {
                    { "task-asteria-access-control", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-access-control", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-ai", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-ai-disclosure", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-attestation", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-senior-attestation", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-backup", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-backup-restore", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-bcp", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-bcp-dr", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-certification", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-certification", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-critical-supplier", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-critical-supplier", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-data-protection", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-data-protection", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-dpa", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-dpa", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-hosting", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-hosting-residency", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-incident", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-incident-response", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-insurance", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-cyber-insurance", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-penetration-test", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-penetration-test", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-profile", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-company-profile", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-sdlc", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-secure-development", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-security-policy", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-security-policy", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-subprocessors", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-subprocessor-register", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-asteria-vulnerability", "case-2026-asteria", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-vulnerability-management", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-access-control", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-access-control", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-ai", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-ai-disclosure", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-attestation", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-senior-attestation", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-backup", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-backup-restore", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-bcp", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-bcp-dr", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-certification", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-certification", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-critical-supplier", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-critical-supplier", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-data-protection", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-data-protection", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-dpa", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-dpa", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-hosting", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-hosting-residency", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-incident", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-incident-response", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-insurance", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-cyber-insurance", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-penetration-test", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-penetration-test", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-profile", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-company-profile", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-sdlc", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-secure-development", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-security-policy", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-security-policy", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-subprocessors", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-subprocessor-register", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-cobalt-vulnerability", "case-2026-cobalt", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-vulnerability-management", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-access-control", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-access-control", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-ai", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-ai-disclosure", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-attestation", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-senior-attestation", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-backup", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-backup-restore", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-bcp", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-bcp-dr", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-certification", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-certification", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-critical-supplier", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-critical-supplier", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-data-protection", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-data-protection", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-dpa", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-dpa", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-hosting", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-hosting-residency", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-incident", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-incident-response", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-insurance", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-cyber-insurance", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-penetration-test", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-penetration-test", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-profile", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-company-profile", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-sdlc", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-secure-development", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-security-policy", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-security-policy", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-subprocessors", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-subprocessor-register", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-northstar-vulnerability", "case-2026-northstar", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-vulnerability-management", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-access-control", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-access-control", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-ai", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-ai-disclosure", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-attestation", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-senior-attestation", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-backup", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "FAILED", "template-task-backup-restore", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-bcp", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-bcp-dr", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-certification", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "FAILED", "template-task-certification", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-critical-supplier", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "FAILED", "template-task-critical-supplier", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-data-protection", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-data-protection", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-dpa", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-dpa", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-hosting", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-hosting-residency", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-incident", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-incident-response", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-insurance", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-cyber-insurance", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-penetration-test", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "FAILED", "template-task-penetration-test", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-profile", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-company-profile", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-sdlc", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-secure-development", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-security-policy", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-security-policy", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-subprocessors", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-subprocessor-register", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-pinebridge-vulnerability", "case-2026-pinebridge", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "SUBMITTED", "template-task-vulnerability-management", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-stratuspay-controls", "case-supplier-northstar-stratuspay", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "IN_PROGRESS", "template-task-supplier-controls", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-stratuspay-profile", "case-supplier-northstar-stratuspay", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "PASSED", "template-task-supplier-profile", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "task-stratuspay-risk", "case-supplier-northstar-stratuspay", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{}", "{}", "NOT_STARTED", "template-task-supplier-risk", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.InsertData(
                table: "TemplateTasks",
                schema: "cases",
                columns: new[] { "Id", "CaseTemplateId", "CreatedAt", "Description", "ParametersJson", "SortOrder", "Status", "TaskTypeId", "Title", "UpdatedAt", "WithdrawnAt", "WithdrawnByUserId", "WithdrawnReason" },
                values: new object[,]
                {
                    { "template-task-access-control", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Confirm MFA enforcement, privileged access controls, joiner/mover/leaver process, and access review cadence.", "{\"due\":\"22 Jun 2026\"}", 6, "ACTIVE", "task-type-access-control", "Access control and MFA attestation", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-ai-disclosure", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Declare whether AI services process customer data, which providers are used, and what controls apply.", "{\"due\":\"27 Jun 2026\"}", 16, "ACTIVE", "task-type-ai-disclosure", "AI usage and customer-data disclosure", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-backup-restore", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Describe backup frequency, retention, encryption, and upload most recent restore-test evidence.", "{\"due\":\"25 Jun 2026\"}", 11, "ACTIVE", "task-type-upload-document", "Backup and restore evidence", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-bcp-dr", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload BCP or disaster recovery plan and confirm last test date.", "{\"due\":\"25 Jun 2026\"}", 12, "ACTIVE", "task-type-policy-document", "Business continuity and disaster recovery plan", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-certification", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload current ISO 27001 certificate, SOC 2 Type II report, Cyber Essentials certificate, or equivalent controls evidence.", "{\"due\":\"20 Jun 2026\"}", 3, "ACTIVE", "task-type-certification", "ISO 27001 or SOC 2 evidence", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-company-profile", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Confirm company details, platform summary, core services, and regulated customer sectors.", "{\"due\":\"18 Jun 2026\"}", 1, "ACTIVE", "task-type-questionnaire", "Company profile and platform summary", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-critical-supplier", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Identify critical suppliers and complete case for material third-party dependencies.", "{\"due\":\"28 Jun 2026\"}", 17, "ACTIVE", "task-type-participant-ddq", "Critical supplier and participant supplier Case", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-cyber-insurance", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload cyber insurance certificate and coverage summary.", "{\"due\":\"26 Jun 2026\"}", 14, "ACTIVE", "task-type-certification", "Cyber insurance evidence", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-data-protection", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Provide ICO or equivalent data protection registration details and data protection officer contact.", "{\"due\":\"23 Jun 2026\"}", 7, "ACTIVE", "task-type-questionnaire", "Data protection registration", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-dpa", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload standard DPA and describe controller/processor responsibilities for customer data.", "{\"due\":\"23 Jun 2026\"}", 8, "ACTIVE", "task-type-policy-document", "GDPR data processing agreement", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-hosting-residency", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "State hosting provider, primary regions, backup regions, and whether customer data leaves the UK or EU.", "{\"due\":\"24 Jun 2026\"}", 10, "ACTIVE", "task-type-hosting-residency", "Hosting and data residency statement", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-incident-response", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload incident response plan and describe breach notification procedure.", "{\"due\":\"26 Jun 2026\"}", 13, "ACTIVE", "task-type-policy-document", "Incident response plan", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-penetration-test", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Provide latest penetration test executive summary, provider name, test date, and remediation status.", "{\"due\":\"21 Jun 2026\"}", 4, "ACTIVE", "task-type-upload-document", "Penetration test summary", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-secure-development", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Describe secure coding, code review, dependency scanning, release controls, and segregation of duties.", "{\"due\":\"27 Jun 2026\"}", 15, "ACTIVE", "task-type-questionnaire", "Secure development lifecycle questionnaire", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-security-policy", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload the current information security policy and confirm senior management approval date.", "{\"due\":\"19 Jun 2026\"}", 2, "ACTIVE", "task-type-policy-document", "Information security policy", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-senior-attestation", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A senior officer confirms the submitted information is accurate and complete.", "{\"due\":\"30 Jun 2026\"}", 18, "ACTIVE", "task-type-signature", "Senior officer attestation and signature", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-subprocessor-register", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "List subprocessors, hosting providers, support tools, analytics services, and monitoring providers.", "{\"due\":\"24 Jun 2026\"}", 9, "ACTIVE", "task-type-supplier-register", "Subprocessor register", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-supplier-controls", "template-critical-supplier-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Confirm key security and resilience controls for the critical supplier.", "{\"due\":\"No due date\"}", 2, "ACTIVE", "task-type-control-attestation", "Supplier control attestation", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-supplier-profile", "template-critical-supplier-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Record the supplier relationship, services provided, and customer-data exposure.", "{\"due\":\"No due date\"}", 1, "ACTIVE", "task-type-questionnaire", "Supplier profile", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-supplier-risk", "template-critical-supplier-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Record known supplier risks, mitigation owners, and target remediation dates.", "{\"due\":\"No due date\"}", 3, "ACTIVE", "task-type-risk-register", "Supplier risk and remediation", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { "template-task-vulnerability-management", "template-annual-platform-ddq", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Describe scanning cadence, severity thresholds, patching SLAs, and exception governance.", "{\"due\":\"21 Jun 2026\"}", 5, "ACTIVE", "task-type-questionnaire", "Vulnerability management process", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null }
                });

            migrationBuilder.InsertData(
                table: "UserAccounts",
                schema: "cases",
                columns: new[] { "Id", "CreatedAt", "DisplayName", "Email", "EntraObjectId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { "user-aisha-khan", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Aisha Khan", "aisha.khan@northstar-cloud.example", "entra-user-aisha-khan", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-amara-singh", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Amara Singh", "amara.singh@dpaa.example", "entra-user-amara-singh", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-amelia-wright", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Amelia Wright", "amelia.wright@cobalt-workflow.example", "entra-user-amelia-wright", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-benjamin-foster", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Benjamin Foster", "benjamin.foster@mercury-retail.example", "entra-user-benjamin-foster", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-ellen-brooks", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Ellen Brooks", "ellen.brooks@ledgerfield.example", "entra-user-ellen-brooks", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-george-evans", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "George Evans", "george.evans@sentinel-grc.example", "entra-user-george-evans", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-jonathan-price", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Jonathan Price", "jonathan.price@dpaa.example", "entra-user-jonathan-price", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-lewis-green", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Lewis Green", "lewis.green@cobalt-workflow.example", "entra-user-lewis-green", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-maya-patel", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Maya Patel", "maya.patel@pinebridge-data.example", "entra-user-maya-patel", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-michael-reeves", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Michael Reeves", "michael.reeves@northstar-cloud.example", "entra-user-michael-reeves", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-nadia-cole", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Nadia Cole", "nadia.cole@portfolio.example", "entra-user-nadia-cole", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-owen-clarke", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Owen Clarke", "owen.clarke@asteria-identity.example", "entra-user-owen-clarke", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-peter-walsh", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Peter Walsh", "peter.walsh@harrington.example", "entra-user-peter-walsh", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-priya-shah", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Priya Shah", "priya.shah@sentinel-grc.example", "entra-user-priya-shah", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-rachel-morgan", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Rachel Morgan", "rachel.morgan@harrington.example", "entra-user-rachel-morgan", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user-sophie-turner", new DateTimeOffset(new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sophie Turner", "sophie.turner@mercury-retail.example", "entra-user-sophie-turner", "ACTIVE", new DateTimeOffset(new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentUsers_EntityId_UserAccountId",
                table: "AgentUsers",
                schema: "cases",
                columns: new[] { "EntityId", "UserAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityTerminologies_AuthorityId",
                table: "AuthorityTerminologies",
                schema: "cases",
                column: "AuthorityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityUsers_EntityId_UserAccountId",
                table: "AuthorityUsers",
                schema: "cases",
                columns: new[] { "EntityId", "UserAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantUsers_EntityId_UserAccountId",
                table: "ParticipantUsers",
                schema: "cases",
                columns: new[] { "EntityId", "UserAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderUsers_EntityId_UserAccountId",
                table: "StakeholderUsers",
                schema: "cases",
                columns: new[] { "EntityId", "UserAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemOwnerUsers_EntityId_UserAccountId",
                table: "SystemOwnerUsers",
                schema: "cases",
                columns: new[] { "EntityId", "UserAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskTypes_Code",
                table: "TaskTypes",
                schema: "cases",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                table: "UserAccounts",
                schema: "cases",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_EntraObjectId",
                table: "UserAccounts",
                schema: "cases",
                column: "EntraObjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessGrants",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "Agents",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "AgentUsers",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "Authorities",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "AuthorityTerminologies",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "AuthorityUsers",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "Cases",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "CaseTemplateParticipants",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "CaseTemplates",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "Participants",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "ParticipantSuppliers",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "ParticipantUsers",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "RequestsForInformation",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "StakeholderParticipantAccesses",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "StakeholderReviews",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "Stakeholders",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "StakeholderUsers",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "SystemOwners",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "SystemOwnerUsers",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "TaskTypes",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "TemplateTasks",
                schema: "cases");

            migrationBuilder.DropTable(
                name: "UserAccounts",
                schema: "cases");
        }
    }
}
