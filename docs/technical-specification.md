# All Checks Out Technical Specification

## Azure Stack

| Area | Technology |
| --- | --- |
| Frontend | React, TypeScript, Vite, Tailwind CSS |
| Backend API | C# Azure Functions, isolated worker |
| Database | Azure SQL Database Serverless |
| Data access | Entity Framework Core |
| Authentication | Microsoft Entra External ID and API bearer tokens |
| Infrastructure | Bicep |
| Static hosting | Azure Storage static website |
| Configuration | Azure App Configuration |

## Runtime Shape

- `apps/shell` contains the React shell.
- `services/functions-api` contains the HTTP-trigger Functions API.
- `services/functions-api/Data/Migrations` contains EF Core migrations.
- `tests/functions-api.Tests` validates the EF model and domain authorization behavior.

The Function App keeps the frontend API contract stable:

```text
GET /health
GET /api/demo/sign-in-options
GET /api/me
GET /api/authorities
GET /api/participants
GET /api/cases
```

The Function App uses `host.json` with an empty route prefix so paths are exposed exactly as shown.

## Authentication And Authorization

The shell signs users in with MSAL and sends an access token as:

```http
Authorization: Bearer <access-token>
```

The Functions API validates the token against the configured Entra authority and accepted audiences. After token validation, `ApplicationUserResolver` maps the Entra object id claim to `UserAccounts.EntraObjectId` in Azure SQL.

Application authorization is enforced in `CasesDomainService` using database memberships and access grants.

## Deployment

Bicep provisions:

- Static website storage for the shell.
- Azure Functions storage for the API runtime.
- Function App for HTTP API endpoints.
- Azure App Configuration.
- Azure SQL server and database.

Top-level deploy commands run infrastructure, database migrations, API deployment, shell environment generation, shell build, and shell upload in sequence.
