# All Checks Out - Technical Specification

## Purpose

This specification describes the Azure course target architecture and the technical scope currently implemented in `azure02-deploy-website-to-azure`.

It is based on:

- `/Users/richardbray/src/000-Planning/courses/azure/tech-stack-comparison.md`
- The current repository source code
- The Azure course phase sequence in `/Users/richardbray/src/141-course-portal/src/data/courses.json`

## Current Phase

- Course module: `azure06`
- Module title: Azure SQL, ASP.NET Core API, and Entity Framework Core
- Repository purpose:
  - Build a React and TypeScript frontend.
  - Add an ASP.NET Core API foundation.
  - Add an Entity Framework Core SQL Server persistence layer.
  - Prepare the frontend to move from in-memory state to protected API calls.
  - Provision Azure Blob static website hosting with Bicep.
  - Upload the production Vite bundle to the `$web` container.
  - Verify a live Azure-hosted single-page application.
- Current implementation boundary:
  - Stage 1 backend foundation exists.
  - Stage 1.5 repository boundary restructure exists.
  - Frontend still uses in-memory domain data.
  - In-memory domain data.
  - Entra frontend sign-in exists, but API token-bearing calls are not wired yet.
  - ASP.NET Core API shell exists.
  - EF Core model, migration, and seed data exist inside `services/cases-api`.
  - Local SQL Server Docker Compose support exists.
  - No real file storage workflow yet.

## Target Azure Stack

| Area | Target technology |
| --- | --- |
| Frontend | React, TypeScript, Vite, Tailwind CSS, shadcn-style UI primitives |
| Backend | C#, ASP.NET Core Web API |
| API endpoints | ASP.NET Core HTTP endpoints |
| Database | Azure SQL Database Serverless, Microsoft SQL Server |
| ORM | Entity Framework Core |
| Authentication | Microsoft Entra External ID |
| Authorization | Application roles plus Azure RBAC where appropriate |
| Object storage | Azure Blob Storage |
| Events | Azure Event Grid |
| Queues | Azure Queue Storage |
| Monitoring | Application Insights, Azure Monitor |
| Infrastructure as code | Bicep |
| DNS and edge | Azure DNS, Azure Managed Certificates, future Azure Front Door or Static Web Apps where needed |

## Current Repository Structure

```text
.
+-- package.json
+-- AllChecksOut.sln
+-- README.md
+-- docs
+-- apps
|   +-- shell
|       +-- src
|       +-- index.html
|       +-- package.json
|       +-- vite.config.ts
|       +-- tsconfig.json
+-- services
|   +-- cases-api
|       +-- Program.cs
|       +-- Entities
|       +-- Data
|           +-- AllChecksOutDbContext.cs
|           +-- Migrations
+-- tests
|   +-- cases-api.Tests
+-- packages
|   +-- ui
|   +-- config
|   +-- contracts
+-- database
|   +-- README.md
|   +-- seed
+-- environments
+-- infra
|   +-- bicep
|       +-- main.bicep
+-- docker-compose.sql.yml
+-- scripts
|   +-- config.sh
|   +-- deploy-infra.sh
|   +-- destroy-infra.sh
|   +-- show-url.sh
|   +-- upload-shell.sh
|   +-- what-if-infra.sh
+-- pnpm-workspace.yaml
```

## Target Repository Structure

Stage 1.5 repository restructure is complete. Deployment boundaries are visible from the folder structure.

Target direction:

```text
apps/
  shell/
  cases-web/                    # future
  people-web/                   # future
  admin-web/                    # future

services/
  cases-api/
  people-api/                   # future
  identity-api/                 # future
  notifications-api/            # future

tests/
  cases-api.Tests/

packages/
  ui/
  config/
  contracts/

database/
  README.md
  seed/

infra/
  bicep/
    main.bicep
    modules/
    parameters/
```

Do not directly share C# domain models with TypeScript. The frontend/backend boundary should be the HTTP API and, later, a generated TypeScript client.

See `docs/repository-architecture.md` for the repository boundary rules.

## Frontend Implementation

- Current app location: `apps/shell`
- Framework:
  - React 19
  - TypeScript 5.9
  - Vite 7
  - React Router 7
- Styling:
  - Tailwind CSS 4
  - shadcn-style component primitives
  - lucide-react icons
  - GOV.UK-influenced typography, spacing, status colors, and table-heavy console layout
  - Azure/AWS-style header with app launcher, global search, account menu, and utility icons
- Runtime state:
  - `AuthContext` stores demo login selection in `localStorage`.
  - `ThemeContext` stores light/dark preference in `localStorage`.
  - `DomainDataContext` exposes the in-memory database and refresh counter.
- Main UI routes:
  - `/`
  - `/admin`
  - `/admin/participants`
  - `/admin/participants/:participantId`
  - `/admin/stakeholders`
  - `/admin/stakeholders/:stakeholderId`
  - `/admin/case-templates`
  - `/admin/case-templates/:templateId`
  - `/admin/task-types`
  - `/admin/users`
  - `/cases`
  - `/cases/suppliers`
  - `/cases/users`
  - `/cases/:caseId`
  - `/cases/:caseId/tasks/:taskId`
  - `/stakeholder`
  - `/stakeholder/participants/:participantId`
  - `/stakeholder/:caseId`

## Current Domain Runtime

- Source of truth: `apps/shell/src/data/console.ts`
- Storage: in-memory TypeScript entities and DTOs
- Refresh model:
  - Mutations run through `InMemoryAllChecksOutDatabase`.
  - UI calls `refreshConsoleViewModels()`.
  - `DomainDataProvider` increments a version value so screens re-render.
- Current seeded authorities:
  - Northstar Trade Association
  - Cobalt Home Services
  - Pinebridge Borough Council
- Current seeded scenario types:
  - Trade association verification
  - Plumbing and electrical service visits
  - Resident permit renewals

## Backend Service Implementation

- Current service location: `services/cases-api`
- Project: `services/cases-api/Cases.Api.csproj`
- Current HTTP endpoints:
  - `/`
  - `/health`
- EF Core ownership:
  - DbContext: `services/cases-api/Data/AllChecksOutDbContext.cs`
  - Seed data: `services/cases-api/Data/AllChecksOutSeedData.cs`
  - Migrations: `services/cases-api/Data/Migrations`
  - SQL schema: `cases`
- Service-local namespaces:
  - `AllChecksOut.Cases.Api.Entities`
  - `AllChecksOut.Cases.Api.Data`

Future services should follow the same ownership rule: service-owned entities, service-owned DbContext, service-owned migrations, and no direct table access across service schemas.

## Domain Model

- `SystemOwner`
- `Authority`
- `AuthorityUser`
- `Participant`
- `ParticipantUser`
- `Stakeholder`
- `StakeholderUser`
- `Agent`
- `AgentUser`
- `StakeholderParticipantAccess`
- `UserAccount`
- `TaskType`
- `CaseTemplate`
- `TemplateTask`
- `CaseTemplateParticipant`
- `Case`
- `Task`

## Domain Rules

- Tenant boundary:
  - `Authority`
  - Participants, stakeholders, templates, cases, and assignments belong to exactly one authority.
- Sign-in context:
  - The current UI exposes Authority, Participant, Stakeholder, and Agent sign-in contexts.
- Task types:
  - Global software capabilities.
  - Not owned by an authority.
- Cases:
  - Generated from published case templates.
  - Not lazily created.
- Tasks:
  - Never physically deleted.
  - Published template tasks may be withdrawn.
  - Incomplete generated tasks are withdrawn with the template task.
  - Passed and failed tasks are preserved.

## Implemented Domain Commands

- `createParticipant`
- `createParticipantUser`
- `createAgent`
- `createAgentUser`
- `createStakeholder`
- `createStakeholderUser`
- `grantStakeholderAccess`
- `createCaseTemplate`
- `addTaskToTemplate`
- `assignParticipantToTemplate`
- `publishTemplate`
- `completeTask`
- `uploadEvidence`
- `submitTask`
- `submitCase`
- `reviewTask`
- `withdrawTemplateTask`

## Implemented Domain Queries

- List and get authorities
- List and get participants
- List and get stakeholders
- List and get case templates
- List and get cases
- List tasks for a case
- List stakeholder access
- List participants visible to a stakeholder
- List users for an authority
- List users for a participant
- List users for a stakeholder
- List agents for an authority
- List users for an agent

## Azure Infrastructure

- Current template: `infra/bicep/main.bicep`
- Provisioned resource:
  - Azure Storage account
- Storage settings:
  - `StorageV2`
  - `Standard_LRS`
  - Blob public access allowed for static website hosting
- Static website hosting:
  - Enabled by `scripts/deploy-infra.sh`
  - Index document: `index.html`
  - 404 document: `index.html`
  - SPA routes are served by returning the React app for unknown paths.
- Outputs:
  - `storageAccountName`

## Automation

- Repository scripts run from the project root.
- Main scripts:
  - `pnpm run shell:dev`
  - `pnpm run shell:build`
  - `pnpm run shell:preview`
  - `pnpm run type-check`
  - `pnpm run infra:what-if`
  - `pnpm run infra:deploy`
  - `pnpm run shell:upload`
  - `pnpm run shell:url`
  - `pnpm run deploy-website`
  - `pnpm run deploy-everything`
  - `pnpm run infra:destroy`
- Configurable environment values:
  - `AZURE_LOCATION`
  - `AZURE_RESOURCE_GROUP`
  - `AZURE_DEPLOYMENT_NAME`
  - `AZURE_APP_NAME`
  - `AZURE_STORAGE_AUTH_MODE`
  - `SHELL_DIST_DIR`

## Current Limitations

- Authentication is a demo selector, not Microsoft Entra External ID.
- Authorization is client-side route scoping, not server-enforced authorization.
- Data is in memory and resets on reload.
- Evidence upload stores metadata only, not binary files.
- Task activity timestamps are partly UI-side display data.
- Task type administration and user administration routes exist as placeholders.
- There are no automated tests yet.
- No ASP.NET Core API is present in this phase.
- No Azure SQL database is present in this phase.
- No Application Insights or Key Vault integration is present in this phase.

## Future Technical Direction

- Move domain commands into an ASP.NET Core Web API.
- Replace demo auth with Microsoft Entra External ID.
- Persist the domain model in Azure SQL Database through EF Core.
- Store uploaded evidence in Azure Blob Storage.
- Use Azure Event Grid and Queue Storage for asynchronous workflows.
- Add Application Insights and structured logging.
- Add Key Vault-backed configuration.
- Split the frontend into microfrontends in the later architecture phase.
- Add Azure AI Vision for evidence and image tagging workflows.
