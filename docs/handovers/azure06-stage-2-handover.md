# Azure06 Stage 2 Handover

Stage 2 adds the authenticated API shell for `services/cases-api` while preserving the Stage 1.5 repository boundaries.

## Summary

- Configured JWT bearer authentication for Microsoft Entra access tokens in `services/cases-api`.
- Added authorization middleware with an authenticated-user fallback policy.
- Kept `/health` and `/` explicitly public.
- Added protected `/api/me`.
- Added CORS configuration for local Vite by default and deployed frontend domains through configuration.
- Added service-local current-user/application-user resolver code under `services/cases-api`.
- Kept EF Core entities, DbContext, seed data, and migrations service-local.
- Did not recreate repository-level backend `src/` projects or shared backend packages.
- Did not replace `InMemoryAllChecksOutDatabase`.

## Files Changed

- `services/cases-api/Program.cs`
- `services/cases-api/Authentication/AuthOptions.cs`
- `services/cases-api/CurrentUser/ApplicationUserResolver.cs`
- `services/cases-api/Endpoints/CasesApiEndpoints.cs`
- `services/cases-api/appsettings.json`
- `services/cases-api/Cases.Api.csproj`
- `tests/cases-api.Tests/MeEndpointTests.cs`
- `tests/cases-api.Tests/Cases.Api.Tests.csproj`
- `apps/shell/src/lib/entra/auth.ts`
- `apps/shell/src/lib/entra/config.ts`

## Backend Auth Configuration

`cases-api` reads Entra JWT validation settings from:

```text
Authentication:Entra:Authority
Authentication:Entra:TenantId
Authentication:Entra:Audience
Authentication:Entra:ClientId
Authentication:Entra:ValidAudiences
```

Use either `Authority` directly or `TenantId`, which builds:

```text
https://login.microsoftonline.com/{tenantId}
```

Set `Audience` to the API audience expected in the access token, usually the API application ID URI or API app registration client ID. `ClientId` is accepted as a fallback audience. `ValidAudiences` can be supplied when both forms need to validate.

Environment variable examples:

```bash
Authentication__Entra__TenantId="..."
Authentication__Entra__Audience="api://..."
Authentication__Entra__ValidAudiences__0="api://..."
Cors__AllowedOrigins__0="http://localhost:5173"
Cors__AllowedOrigins__1="https://<deployed-frontend-domain>"
```

## CORS

Default allowed origins are:

```text
http://localhost:5173
http://127.0.0.1:5173
```

Deployed frontend domains should be added through `Cors:AllowedOrigins`.

## `/api/me`

`GET /api/me` requires a bearer token.

The resolver reads claims from `HttpContext.User` and prefers:

- `oid` for Entra object ID.
- `tid` for tenant ID.
- `sub` for subject.
- `email`, then `preferred_username`, for email.
- `name` for display name.

The Entra `oid` is matched against `cases.UserAccounts.EntraObjectId`.

The response includes:

- authenticated Entra identity
- resolved application user, or `null` if no `UserAccount` matches
- application memberships for system owner, authority, participant, stakeholder, and agent contexts

Seeded user accounts currently use placeholder Entra IDs such as:

```text
entra-user-aisha-khan
```

## Frontend Notes

The shell still uses the in-memory data layer. The only frontend auth change was to keep sign-in scopes as:

```text
openid profile email
```

`getEntraAccessToken()` remains the single helper for acquiring an API access token using `VITE_ENTRA_API_SCOPE`.

For real API calls, configure:

```bash
VITE_ENTRA_API_SCOPE="api://<api-app-id-or-uri>/<scope-name>"
```

## How To Run Locally

Start SQL Server if you need database-backed manual API calls:

```bash
docker compose -f docker-compose.sql.yml up -d
dotnet ef database update --project services/cases-api --startup-project services/cases-api
dotnet run --project services/cases-api
```

Public health check:

```bash
curl http://localhost:<port>/health
```

Protected current user endpoint:

```bash
curl -H "Authorization: Bearer <access-token>" http://localhost:<port>/api/me
```

Without a valid token, `/api/me` returns `401`.

## Tests Run

Passed:

```bash
pnpm run shell:build
dotnet build AllChecksOut.sln
dotnet test AllChecksOut.sln
dotnet ef migrations list --project services/cases-api --startup-project services/cases-api
```

`dotnet test` passed 4 tests, including:

- `/health` is public.
- `/api/me` returns `401` without authentication.
- `/api/me` returns identity and participant membership when authenticated with a test principal.

EF migration listing built successfully and listed:

```text
20260616230256_InitialSqlFoundation
```

Applied/pending migration status was not shown because local SQL Server was not running.

The frontend build completed with Vite's existing chunk-size warning for the main bundle.

## Known Gaps For Stage 3

- `/api/me` was tested with a test authentication scheme, not a live Entra access token.
- Entra app registration/API scope configuration still needs to be applied in Azure.
- No domain command/query endpoints have been implemented yet.
- The shell still uses `InMemoryAllChecksOutDatabase`.
- No generated API client exists yet.
- No Azure API hosting, Azure SQL provisioning, or deployment pipeline changes were added.
- Seeded `UserAccount.EntraObjectId` values are placeholders until real Entra users are linked.
