# Azure06 Staged Implementation Plan

This document is the master handover plan for implementing Azure06 in separate sequential agent stages.

Azure06 replaces the current in-memory frontend persistence with Azure SQL, Entity Framework Core, an ASP.NET Core API, protected endpoints, and token-bearing frontend API calls.

Future agents should read these documents before starting:

- `docs/phased-project-plan.md`
- `docs/technical-specification.md`
- `docs/functional-specification.md`
- `docs/database-entities.md`
- `docs/azure06-authentication-decisions.md`

## Current Repository State

The repository currently has:

- React, TypeScript, Vite frontend in `apps/ui`.
- In-memory domain state and domain commands in `apps/ui/src/data/console.ts`.
- Demo/domain sign-in context stored in local storage under `user`.
- Entra sign-in through MSAL in `apps/ui/src/lib/entra/auth.ts`.
- MSAL configured to use local storage for its own cache.
- Azure infrastructure for static website hosting and App Configuration.
- No ASP.NET Core solution.
- No API project.
- No EF Core model or migrations.
- No Azure SQL database infrastructure.
- No frontend API client layer.

## Core Decisions

Use a staged approach. Each stage should leave the repository compiling and should write a handover note for the next stage.

Use access tokens for API calls. Do not manually read MSAL internal local storage entries and do not send Entra ID tokens to the API.

Frontend protected API calls should use:

```ts
Authorization: `Bearer ${accessToken}`
```

The ASP.NET Core API should validate Entra access tokens and then use SQL data for application authorization.

The database should use string IDs initially, matching the current TypeScript entity IDs. This keeps migration from seeded frontend data simple and avoids unnecessary UI churn during Azure06.

JSON-shaped fields should be stored as SQL text or JSON columns through EF Core value conversion, depending on provider support and implementation simplicity. Preserve the DTO shape used by the UI.

Do not attempt Blob Storage or real document binaries in Azure06. Evidence remains metadata in SQL. Blob Storage belongs to Azure07.

## Stage 1 - Backend, EF Core, SQL Foundation

### Goal

Create the .NET backend foundation and SQL persistence layer without yet replacing the frontend UI flows.

### Scope

Add a solution and backend projects:

- `AllChecksOut.sln`
- `apps/api/AllChecksOut.Api`
- `src/AllChecksOut.Domain`
- `src/AllChecksOut.Infrastructure`
- `tests/AllChecksOut.Api.Tests` or `tests/AllChecksOut.Infrastructure.Tests`

Use current ASP.NET Core minimal API conventions unless there is a strong reason to introduce MVC controllers.

Add EF Core with SQL Server provider.

Create entity classes for all records listed in `docs/database-entities.md`:

- `SystemOwner`
- `Authority`
- `AuthorityTerminology`
- `Participant`
- `Stakeholder`
- `Agent`
- `UserAccount`
- `SystemOwnerUser`
- `AuthorityUser`
- `ParticipantUser`
- `StakeholderUser`
- `AgentUser`
- `StakeholderParticipantAccess`
- `AccessGrant`
- `TaskType`
- `CaseTemplate`
- `TemplateTask`
- `CaseTemplateParticipant`
- `Case`
- `Task`
- `StakeholderReview`
- `RequestForInformation`
- `ParticipantSupplier`

Use a DbContext, for example `AllChecksOutDbContext`.

Add EF Core migrations.

Add seed data equivalent to the current seeded data in `apps/ui/src/data/console.ts`.

Add local SQL development support. Prefer a Docker Compose SQL Server option or clear documented connection string setup.

Add root scripts for backend build/test/migrate if they fit the existing package script style.

### Acceptance Criteria

- `dotnet build` succeeds.
- Tests for DbContext creation and basic seed/query behavior pass.
- Initial migration exists.
- Local connection string configuration is documented.
- Seeded data includes enough records for the current UI scenarios.
- Frontend still builds unchanged.

### Keep Out Of Scope

- Full endpoint implementation.
- Frontend migration away from `InMemoryAllChecksOutDatabase`.
- Entra JWT bearer authentication.
- Azure SQL Bicep deployment, unless a minimal placeholder is needed for configuration names.

### Handover Note To Write

Create `docs/handovers/azure06-stage-1-handover.md` with:

- Projects and packages added.
- Migration names.
- How to run local SQL and migrations.
- Any entity mapping compromises.
- Tests run and results.
- Known gaps for Stage 2.

## Stage 2 - Authentication, API Shell, Protected Endpoint Model

### Goal

Add the authenticated API surface and establish how the backend validates Entra tokens and resolves application users.

### Scope

Configure the ASP.NET Core API with:

- JWT bearer authentication for Microsoft Entra.
- Authorization middleware.
- CORS for local Vite and deployed frontend domains.
- Environment-based configuration.
- Health endpoint that is explicitly public.
- Protected `/api/me` endpoint.

Follow `docs/azure06-authentication-decisions.md`.

The `/api/me` endpoint should:

- Require a valid access token.
- Read Entra claims from `HttpContext.User`.
- Prefer the `oid` claim for Entra object ID.
- Find the matching `UserAccount.entraObjectId`.
- Return the Entra identity and application memberships.

Add an authorization/current-user service, for example:

- `ICurrentUser`
- `CurrentUserService`
- `ApplicationUserResolver`
- helper methods for authority, participant, stakeholder, and agent membership checks

Add API conventions:

- route prefix, likely `/api`
- consistent problem/error responses
- request validation pattern
- protected by default, public by explicit opt-out

Update Entra infrastructure/configuration only as much as needed for the API audience and scope:

- expose or document the API scope
- make sure `VITE_ENTRA_API_SCOPE` represents the API access scope
- keep frontend sign-in identity scopes available

### Acceptance Criteria

- API runs locally.
- Public health endpoint works without a token.
- `/api/me` returns 401 without a token.
- `/api/me` returns identity and memberships with a valid Entra access token.
- Backend tests cover unauthorized and authorized behavior where practical.
- Frontend still builds.

### Keep Out Of Scope

- Porting all domain commands and queries.
- Replacing the React data layer.
- Full Azure deployment of the API.

### Handover Note To Write

Create `docs/handovers/azure06-stage-2-handover.md` with:

- Auth packages and configuration added.
- Required Entra app registration/API scope settings.
- How to run and call `/api/me`.
- Claim names used.
- Any local token testing notes.
- Known gaps for Stage 3.

## Stage 3 - Port Domain Commands And Queries To API

### Goal

Move the behavior currently inside `InMemoryAllChecksOutDatabase` into backend services and API endpoints.

### Scope

Use `apps/ui/src/data/console.ts` as the behavioral source of truth.

Port query behavior for:

- authorities
- authority terminology
- participants
- stakeholders
- agents
- users and memberships
- case templates
- template tasks
- case template participants
- cases
- tasks
- stakeholder access
- access grants
- participant suppliers
- stakeholder reviews
- requests for information

Port command behavior for:

- update user email
- register user account with Entra object ID
- update authority terminology
- create participant
- create participant user
- create authority user
- create stakeholder
- create stakeholder user
- create agent
- create agent user
- grant stakeholder access
- create access grant
- update access grant status
- create participant supplier
- link participant supplier to case
- unlink participant supplier from case
- upsert stakeholder review
- create request for information
- respond to request for information
- update request for information status
- create case template
- add task to template
- finalize case template
- assign participant to template
- complete task
- upload evidence metadata
- submit task
- submit case
- review task
- withdraw template task
- withdraw case
- reinstate case
- delete case template where allowed

Preserve business rules from `docs/functional-specification.md`.

Apply backend authorization checks. Do not trust the frontend selected context. The frontend may send an intended account context, but the API must verify the authenticated user is a member or has an active grant for the requested data.

Create DTOs that are close to the existing TypeScript DTOs to minimize Stage 4 UI churn.

Add integration tests for the highest-risk rules:

- tenant boundary checks
- assigning finalized templates
- duplicate assignment prevention
- task withdrawal rules
- case submission rules
- stakeholder/agent access grant scoping
- request for information permissions

### Acceptance Criteria

- API exposes the domain surface needed by the current UI.
- Existing seeded demo scenarios can be read through the API.
- Major commands mutate SQL data correctly.
- Authorization checks prevent cross-tenant or out-of-scope access.
- Tests cover representative commands and queries.

### Keep Out Of Scope

- React page rewrites beyond any small DTO contract exploration.
- Blob document storage.
- AI verification workflows.

### Handover Note To Write

Create `docs/handovers/azure06-stage-3-handover.md` with:

- Endpoint list.
- DTO naming and shape notes.
- Commands ported.
- Commands intentionally deferred, if any.
- Authorization checks implemented.
- Tests run and results.
- Known gaps for Stage 4.

## Stage 4 - Frontend API Integration And Azure Deployment

### Goal

Replace direct in-memory database usage in the React app with authenticated API calls, then update Azure deployment for API and SQL.

### Scope

Add frontend API configuration:

- `VITE_API_BASE_URL`
- local development default
- deployed environment values

Add a frontend API client wrapper that:

- calls `getEntraAccessToken()`
- sends `Authorization: Bearer <accessToken>`
- handles JSON parsing
- handles 401 and 403 consistently
- centralizes errors

Refactor `DomainDataContext` and page code away from direct `db.*` calls.

The current UI has many direct calls to `db` in:

- `apps/ui/src/context/DomainDataContext.tsx`
- `apps/ui/src/pages/ConsolePages.tsx`
- `apps/ui/src/pages/EntraCallbackPage.tsx`
- `apps/ui/src/pages/SetEmailAddressesPage.tsx`
- `apps/ui/src/pages/SignInPage.tsx`

Choose a pragmatic migration approach:

- either build an API-backed repository with similar method names to the current in-memory database
- or gradually introduce dedicated query/mutation hooks

For this course phase, preserving UI behavior with minimal visual churn is more important than perfect frontend architecture.

Update infrastructure:

- Azure SQL Database Serverless.
- SQL server firewall or private access appropriate for the course environment.
- API hosting target, likely Azure App Service or Azure Container Apps depending on course direction.
- App settings for API connection string and Entra validation.
- App Configuration values for frontend API URL and Entra API scope.
- Deployment scripts for API, migrations, and frontend env generation.

Add local/deployment documentation.

### Acceptance Criteria

- Frontend no longer uses `InMemoryAllChecksOutDatabase` for persisted domain data.
- Login still works.
- Protected API calls include bearer access tokens.
- Current UI scenarios work against the API and SQL database.
- `pnpm run type-check` succeeds.
- `dotnet test` succeeds.
- Deployment scripts can provision SQL/API configuration or clearly document any manual step.

### Keep Out Of Scope

- Blob Storage document binaries.
- Microservices.
- Microfrontends.
- AI document verification.
- Major UI redesign.

### Handover Note To Write

Create `docs/handovers/azure06-stage-4-handover.md` with:

- API client files added.
- UI files refactored.
- Remaining in-memory references, if any.
- Infrastructure changes.
- Deployment commands.
- Tests run and results.
- Any production-hardening gaps for Azure12.

## Cross-Stage Rules

Each agent should:

- Preserve existing user changes.
- Keep unrelated refactors out of scope.
- Prefer current repository naming and style.
- Keep the app buildable at the end of the stage.
- Add tests proportional to the risk of the change.
- Update documentation when behavior or commands change.
- Write a handover document before finishing.

Each handover should include:

- Summary of changes.
- Files changed.
- Commands/tests run.
- Known issues.
- Exact next recommended steps.

## Recommended Final Verification

After all stages:

```bash
pnpm install
pnpm run type-check
pnpm run ui:build
dotnet build
dotnet test
```

Also verify manually:

- sign in through Entra
- call `/api/me`
- list participant cases
- complete a task
- submit a task
- submit a case only when allowed
- create an access grant
- view stakeholder-scoped data
- reject out-of-scope API access

