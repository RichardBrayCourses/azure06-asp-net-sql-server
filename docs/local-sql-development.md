# Local SQL Development

Azure06 Stage 1 adds a local SQL Server option for the ASP.NET Core services and EF Core migrations.

## Start SQL Server

```bash
docker compose -f docker-compose.sql.yml up -d
```

The default local connection string is:

```text
Server=localhost,1433;Database=AllChecksOut;User Id=sa;Password=AllChecksOut_2026!;TrustServerCertificate=True;Encrypt=False
```

It is configured in `services/cases-api/appsettings.Development.json` and can be overridden with:

```bash
ConnectionStrings__AllChecksOut="Server=...;Database=...;..."
```

## Run Migrations

```bash
pnpm run backend:migrate
```

Or use `dotnet` directly:

```bash
dotnet ef database update \
  --project services/cases-api \
  --startup-project services/cases-api
```

## Build And Test

```bash
pnpm run backend:build
pnpm run backend:test
```

## Run The API And Shell Together

Stage 4 wires the shell to authenticated `cases-api` calls.

Start SQL Server and apply migrations:

```bash
pnpm run sql:up
pnpm run backend:migrate
```

Start the API:

```bash
dotnet run --project services/cases-api
```

Configure the shell API URL in `apps/shell/.env` if the API is not listening on the default:

```text
VITE_API_BASE_URL=http://localhost:5294
VITE_ENTRA_API_SCOPE=api://<api-app-id-or-uri>/<scope-name>
```

Then run the shell:

```bash
pnpm run shell:dev
```

Protected shell API calls use `getEntraAccessToken()` and send `Authorization: Bearer <access-token>`.
The sign-in selector still uses seeded local demo data before Entra has issued an API token.
