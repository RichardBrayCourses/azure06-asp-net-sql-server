# Azure06 Stage 3 Handover

Stage 3 ports the current in-memory domain command/query surface into protected `cases-api` endpoints while keeping the React shell on `InMemoryAllChecksOutDatabase`.

## Summary

- Added service-local domain API code under `services/cases-api/Domain`.
- Added protected `/api/...` endpoints for the current UI's core data sets and representative commands.
- Kept `/health` and `/` public.
- Kept `/api/me` protected and unchanged in behavior.
- Kept EF entities, DbContext, seed data, and migrations owned by `services/cases-api`.
- Did not add shared backend packages, generated clients, Azure SQL provisioning, or frontend data-layer replacement.

## Files Changed

- `services/cases-api/Program.cs`
- `services/cases-api/Endpoints/CasesApiEndpoints.cs`
- `services/cases-api/Domain/DomainDtos.cs`
- `services/cases-api/Domain/DomainErrors.cs`
- `services/cases-api/Domain/CasesDomainService.cs`
- `tests/cases-api.Tests/DomainEndpointTests.cs`
- `docs/handovers/azure06-stage-3-handover.md`

## Endpoint List

All endpoints below require authentication through the fallback authorization policy.

- `GET /api/authorities`
- `GET /api/authorities/{authorityId}/terminology`
- `PUT /api/authorities/{authorityId}/terminology`
- `GET /api/authorities/{authorityId}/users`
- `POST /api/authorities/{authorityId}/users`
- `GET /api/users`
- `PATCH /api/users/{userAccountId}/email`
- `PATCH /api/users/{userAccountId}/entra`
- `GET /api/participants`
- `POST /api/participants`
- `GET /api/participants/{participantId}/users`
- `POST /api/participants/{participantId}/users`
- `GET /api/participants/{participantId}/suppliers`
- `POST /api/participant-suppliers`
- `GET /api/stakeholders`
- `POST /api/stakeholders`
- `GET /api/stakeholders/{stakeholderId}/users`
- `POST /api/stakeholders/{stakeholderId}/users`
- `GET /api/stakeholders/{stakeholderId}/access`
- `POST /api/stakeholder-access`
- `GET /api/agents`
- `POST /api/agents`
- `GET /api/agents/{agentId}/users`
- `POST /api/agents/{agentId}/users`
- `GET /api/task-types`
- `GET /api/case-templates`
- `POST /api/case-templates`
- `DELETE /api/case-templates/{caseTemplateId}`
- `POST /api/case-templates/{caseTemplateId}/finalize`
- `GET /api/case-templates/{caseTemplateId}/tasks`
- `POST /api/case-templates/{caseTemplateId}/tasks`
- `POST /api/template-tasks/{templateTaskId}/withdraw`
- `GET /api/case-templates/{caseTemplateId}/participants`
- `POST /api/case-templates/{caseTemplateId}/participants`
- `GET /api/cases`
- `POST /api/cases/{caseId}/submit`
- `POST /api/cases/{caseId}/withdraw`
- `POST /api/cases/{caseId}/reinstate`
- `PUT /api/cases/{caseId}/participant-supplier`
- `DELETE /api/cases/{caseId}/participant-supplier`
- `GET /api/cases/{caseId}/tasks`
- `GET /api/cases/{caseId}/stakeholder-reviews`
- `POST /api/tasks/{taskId}/complete`
- `POST /api/tasks/{taskId}/evidence`
- `POST /api/tasks/{taskId}/submit`
- `POST /api/tasks/{taskId}/review`
- `GET /api/access-grants`
- `POST /api/access-grants`
- `PATCH /api/access-grants/{accessGrantId}/status`
- `POST /api/stakeholder-reviews`
- `GET /api/requests-for-information`
- `POST /api/requests-for-information`
- `POST /api/requests-for-information/{requestId}/response`
- `PATCH /api/requests-for-information/{requestId}/status`

## DTO Notes

- DTOs intentionally stay close to the TypeScript DTO fields in `apps/shell/src/data/console.ts`.
- JSON-backed columns are returned as JSON objects/arrays rather than raw strings for:
  - authority terminology labels
  - task type parameter schema
  - template task parameters
  - task response and evidence
  - request-for-information status history
- Command names and payloads mirror the existing TypeScript command names where practical.

## Commands Ported

- update user email
- register user account with Entra object ID
- update authority terminology
- create participant and initial participant user
- create authority, participant, stakeholder, and agent users
- create stakeholder
- create agent
- grant stakeholder access
- create access grant
- update access grant status
- create participant supplier
- link/unlink participant supplier to/from case
- upsert stakeholder review
- create/respond/update request for information
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

## Authorization Checks

- Application user is resolved from the authenticated Entra `oid` through `UserAccount.EntraObjectId`.
- Authority users administer authority-owned participants, stakeholders, agents, templates, cases, task reviews, and terminology.
- Participant users read and mutate only their participant-owned cases, tasks, suppliers, RFIs, and grants.
- Stakeholder users read only participants/cases covered by active grants and can create RFIs only when the active grant is not `READ_ONLY`.
- Agent users read through active agent grants and can mutate participant work only with `CREATE_AND_EDIT` or `ADMINISTER_GRANTS`.
- Cross-participant supplier links and duplicate non-revoked access grants are blocked.

## Tests Added

- Domain endpoints require authentication.
- Stakeholder cannot read a participant outside active grant scope.
- Participant cannot submit a case while active tasks are not submitted.
- Authority can assign a finalized template and duplicate assignment is blocked.
- Stakeholder with a read-only grant cannot create a request for information.

## Tests Run

Passed:

```bash
pnpm run shell:build
dotnet build AllChecksOut.sln
dotnet test AllChecksOut.sln
dotnet ef migrations list --project services/cases-api --startup-project services/cases-api
```

`dotnet test` passed 9 tests.

`dotnet ef migrations list` built successfully and listed:

```text
20260616230256_InitialSqlFoundation
```

It also reported that applied/pending status could not be determined because local SQL Server was not running.

The frontend build completed with Vite's existing chunk-size warning for the main bundle.

## Known Gaps For Stage 4

- React still uses `InMemoryAllChecksOutDatabase`; no API-backed repository/client has been wired yet.
- Endpoint shapes are hand-written and not generated into `packages/contracts`.
- Some lower-risk validation is intentionally pragmatic and should be tightened while wiring the real frontend calls.
- JSON label merging still happens in the frontend; the API stores and returns the submitted terminology JSON.
- Tests use the local test auth scheme, not live Entra tokens.
- No Azure API deployment, SQL provisioning, migration runner, App Service, or Container Apps setup has been added.
