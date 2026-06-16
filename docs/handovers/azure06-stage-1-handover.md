# Azure06 Stage 1 Handover

Stage 1 added the .NET backend foundation and SQL persistence layer without replacing the frontend in-memory data flows.

## Projects Added

- `AllChecksOut.sln`
- `apps/api/AllChecksOut.Api`
- `src/AllChecksOut.Domain`
- `src/AllChecksOut.Infrastructure`
- `tests/AllChecksOut.Infrastructure.Tests`

## Packages Added

- `Microsoft.EntityFrameworkCore.SqlServer` in `AllChecksOut.Infrastructure`
- `Microsoft.EntityFrameworkCore.Design` in `AllChecksOut.Infrastructure`
- `Microsoft.EntityFrameworkCore.Design` in `AllChecksOut.Api` for EF tooling with the startup project
- `Microsoft.EntityFrameworkCore.Sqlite` in `AllChecksOut.Infrastructure.Tests`

## Database Model

The domain project now contains entity classes for every record in `docs/database-entities.md`.

`AllChecksOutDbContext` is in `src/AllChecksOut.Infrastructure` and exposes DbSets for:

- system owners, authorities, authority terminology
- participants, stakeholders, agents, users, memberships
- stakeholder access, access grants
- task types, case templates, template tasks, template participants
- cases, tasks, stakeholder reviews, requests for information, participant suppliers

## Migration

Initial migration:

- `20260616230256_InitialSqlFoundation`

Files:

- `src/AllChecksOut.Infrastructure/Migrations/20260616230256_InitialSqlFoundation.cs`
- `src/AllChecksOut.Infrastructure/Migrations/20260616230256_InitialSqlFoundation.Designer.cs`
- `src/AllChecksOut.Infrastructure/Migrations/AllChecksOutDbContextModelSnapshot.cs`

## Seed Data

Seed data lives in `src/AllChecksOut.Infrastructure/AllChecksOutSeedData.cs`.

It preserves the current UI demo IDs from `apps/ui/src/data/console.ts`, including the Northstar/Digital Platform Assurance Association scenario, participants, stakeholders, agents, user accounts, memberships, access grants, participant suppliers, templates, cases, tasks, reviews, and RFIs.

## Local SQL

Local SQL Server support was added through:

- `docker-compose.sql.yml`
- `docs/local-sql-development.md`

Start SQL:

```bash
pnpm run sql:up
```

Apply migrations:

```bash
pnpm run backend:migrate
```

Default local connection string:

```text
Server=localhost,1433;Database=AllChecksOut;User Id=sa;Password=AllChecksOut_2026!;TrustServerCertificate=True;Encrypt=False
```

Override with `ConnectionStrings__AllChecksOut`.

## Root Scripts

Added to `package.json`:

- `backend:build`
- `backend:test`
- `backend:migrate`
- `sql:up`
- `sql:down`

## Entity Mapping Compromises

- IDs remain strings to match current TypeScript DTO IDs.
- Status values are string columns for now, matching the frontend literal values and avoiding early enum migration churn.
- JSON-shaped fields are stored as strings. SQL Server migrations use `nvarchar(max)`.
- Tests use SQLite, so JSON column type configuration is applied only when the provider is SQL Server.
- Evidence remains metadata JSON only. No Blob Storage or document binaries were added.

## Tests Run

Passed:

```bash
dotnet build AllChecksOut.sln
dotnet test AllChecksOut.sln
pnpm run ui:build
```

Migration listing:

```bash
dotnet ef migrations list --project src/AllChecksOut.Infrastructure --startup-project apps/api/AllChecksOut.Api
```

This listed `20260616230256_InitialSqlFoundation`. It could not determine applied database status because local SQL Server was not running.

## Immediate Next Step

Before Stage 2 authentication work, run Stage 1.5 repository restructure. The current layout is a working Stage 1 scaffold, but frontend/backend separation and future shared-package boundaries should be clarified before more code is added.

See `docs/azure06-staged-implementation-plan.md` for the Stage 1.5 target layout and acceptance criteria.

## Known Gaps For Stage 2

- No JWT bearer authentication has been configured yet.
- No protected `/api/me` endpoint exists yet.
- No current-user/application-user resolver exists yet.
- No CORS policy has been added yet.
- No domain command/query endpoints have been ported yet.
- Frontend still uses `InMemoryAllChecksOutDatabase`.
