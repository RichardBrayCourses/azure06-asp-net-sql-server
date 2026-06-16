# Azure06 Stage 1.5 Handover

Stage 1.5 was corrected to align the repository with the intended future direction: independently deployable frontend apps, independently deployable backend services, service-owned EF Core models, and no repository-wide Clean Architecture layout.

## Final Repository Tree

```text
apps/
  shell/
services/
  cases-api/
    Program.cs
    Entities/
    Data/
      AllChecksOutDbContext.cs
      AllChecksOutSeedData.cs
      DesignTimeDbContextFactory.cs
      Migrations/
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
scripts/
```

## Moves Performed

- Renamed the frontend app from `apps/ui` to `apps/shell`.
- Moved the ASP.NET Core host from `apps/api/AllChecksOut.Api` to `services/cases-api`.
- Removed the repository-level `src/AllChecksOut.Domain`, `src/AllChecksOut.Application`, and `src/AllChecksOut.Infrastructure` project layout.
- Moved entities, DbContext, seed data, design-time factory, and EF migrations into `services/cases-api`.
- Renamed the service project to `services/cases-api/Cases.Api.csproj`.
- Renamed the test project to `tests/cases-api.Tests/Cases.Api.Tests.csproj`.
- Moved Bicep to `infra/bicep/main.bicep`.
- Added placeholders for future frontend shared packages under `packages/ui`, `packages/config`, and `packages/contracts`.
- Added `docs/repository-architecture.md`.

## Script And Config Changes

- Root frontend scripts now use `shell:*`.
- `deploy-website` runs `shell:env`, `shell:build`, and `shell:upload`.
- Deployment helpers are now:
  - `scripts/generate-shell-env.sh`
  - `scripts/upload-shell.sh`
- `pnpm-workspace.yaml` covers `apps/*` and `packages/*`.
- Static website upload reads `apps/shell/dist` through `SHELL_DIST_DIR`.
- Bicep deployment scripts resolve `infra/bicep/main.bicep` through `BICEP_TEMPLATE_FILE`.
- EF migration command now uses the service project:

```bash
dotnet ef database update --project services/cases-api --startup-project services/cases-api
```

## Service Ownership

- `services/cases-api` currently owns the Stage 1 SQL model, seed data, DbContext, and migrations.
- The DbContext uses the `cases` SQL schema.
- EF migrations live in `services/cases-api/Data/Migrations`.
- Backend entity and data namespaces are service-local:
  - `AllChecksOut.Cases.Api.Entities`
  - `AllChecksOut.Cases.Api.Data`

## Deliberate Choices

- Did not create empty `cases-web`, `people-web`, `admin-web`, `people-api`, `identity-api`, or `notifications-api` apps/services. Those should be added when their route, data, and deployment boundaries are real.
- Did not keep shared backend Application/Domain/Infrastructure projects because that would work against service ownership.
- Kept the current React app as `apps/shell` until the route areas are extracted into deployable microfrontends.

## Tests Run

Passed:

```bash
pnpm install --lockfile-only
pnpm run shell:build
dotnet build AllChecksOut.sln
dotnet test AllChecksOut.sln
dotnet ef migrations list --project services/cases-api --startup-project services/cases-api
bash -n scripts/*.sh
```

EF migration listing built successfully and listed:

```text
20260616230256_InitialSqlFoundation
```

Applied/pending database status was not shown because local SQL Server was not running.

The frontend build completed with Vite's existing chunk-size warning for the main bundle.

## Known Gaps For Stage 2

- No JWT bearer authentication has been configured yet.
- No protected `/api/me` endpoint exists yet.
- No current-user/application-user resolver exists yet.
- No CORS policy has been added yet.
- No domain command/query endpoints have been ported yet.
- Frontend still uses `InMemoryAllChecksOutDatabase`.
- `cases-api` still contains the full Stage 1 data model; future bounded service extraction should split real ownership deliberately.
- No generated TypeScript API client exists yet.
