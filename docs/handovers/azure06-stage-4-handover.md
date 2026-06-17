# Azure06 Stage 4 Handover

Stage 4 replaces the shell's mutation path with authenticated `cases-api` calls and adds a small API client/repository layer while preserving the current UI.

## Summary

- Added frontend API base URL configuration with a local default of `http://localhost:5294`.
- Added a token-bearing API client that calls `getEntraAccessToken()` and sends `Authorization: Bearer <accessToken>`.
- Added consistent API error handling for `401`, `403`, and problem responses.
- Added an API-backed repository with method names matching `InMemoryAllChecksOutDatabase`.
- Repointed `DomainDataContext` to the API-backed repository and hydrates domain view models after sign-in.
- Updated console command handlers to await API-backed mutations and preserve existing error panels.
- Kept the pre-authentication sign-in selector backed by seeded local demo data because protected API calls require an Entra access token.
- Did not add Azure SQL provisioning, API hosting infrastructure, generated API clients, shared backend packages, or visual UI rewrites.

## Files Changed

- `apps/shell/src/lib/api/config.ts`
- `apps/shell/src/lib/api/client.ts`
- `apps/shell/src/data/apiRepository.ts`
- `apps/shell/src/context/DomainDataContext.tsx`
- `apps/shell/src/data/console.ts`
- `apps/shell/src/pages/ConsolePages.tsx`
- `apps/shell/src/pages/EntraCallbackPage.tsx`
- `apps/shell/src/pages/SetEmailAddressesPage.tsx`
- `apps/shell/src/vite-env.d.ts`
- `docs/local-sql-development.md`
- `docs/handovers/azure06-stage-4-handover.md`

## API Client Files Added

- `apps/shell/src/lib/api/config.ts`
  - Reads `VITE_API_BASE_URL`.
  - Defaults locally to `http://localhost:5294`.
  - Normalizes trailing slashes.
- `apps/shell/src/lib/api/client.ts`
  - Calls `getEntraAccessToken()`.
  - Sends bearer tokens for protected API calls.
  - Parses JSON responses.
  - Converts `401`, `403`, and problem responses into `ApiError`.
- `apps/shell/src/data/apiRepository.ts`
  - Extends the existing repository shape.
  - Hydrates console DTO collections from Stage 3 endpoints.
  - Implements representative commands through protected API calls.

## UI Files Refactored

- `DomainDataContext` now owns an `ApiBackedAllChecksOutDatabase` instance and calls `hydrateFromApi()` after sign-in.
- `console.ts` now exposes `setConsoleDatabase()` so existing view-model builders can read from the API-backed repository without broad page rewrites.
- `ConsolePages.tsx` command handlers now await API-backed mutations.
- `SetEmailAddressesPage.tsx` awaits email updates through the repository.
- `EntraCallbackPage.tsx` awaits the Entra registration attempt and refreshes data before navigation.

## Configuration Notes

Local shell defaults:

```text
VITE_API_BASE_URL=http://localhost:5294
VITE_ENTRA_API_SCOPE=api://<api-app-id-or-uri>/<scope-name>
```

Run locally:

```bash
pnpm run sql:up
pnpm run backend:migrate
dotnet run --project services/cases-api
pnpm run shell:dev
```

`VITE_ENTRA_API_SCOPE` must be set for protected API calls. If it is blank, the client raises a `401`-style local error before calling the API.

## Remaining In-Memory References

- `apps/shell/src/data/console.ts` still contains `InMemoryAllChecksOutDatabase`, seeded DTOs, entity wrappers, and view-model builders.
- The sign-in and email setup flows still use seeded local demo data before an API access token exists.
- The API-backed repository currently upserts hydrated API DTOs into the existing console collections instead of removing every stale seeded record. Backend authorization remains enforced by `cases-api`; frontend display scoping should be tightened in a later pass.
- First-time Entra object ID linking is still constrained by the protected backend domain endpoint model. If the token `oid` is not already linked to a `UserAccount`, `RegisterUserAccountWithEntra` can be forbidden by the API. The callback keeps the UI moving and this should be solved with a dedicated verified linking/onboarding endpoint later.

## Tests Run

Passed:

```bash
pnpm -C apps/shell run type-check
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

## Known Gaps

- No Azure API hosting, Azure SQL provisioning, migration runner, deployment workflow, or App Configuration `VITE_API_BASE_URL` generation was added in this pass.
- No generated API client was added.
- Hydration is pragmatic and partial for non-authority roles because some Stage 3 list endpoints are intentionally authority-admin scoped.
- Manual local end-to-end testing needs a running SQL Server, migrated database, valid Entra API scope, and seeded/linked `UserAccount.EntraObjectId`.
- Production hardening should add a proper account-linking endpoint, stronger frontend stale-data eviction, and more frontend tests around `401`/`403` handling.
