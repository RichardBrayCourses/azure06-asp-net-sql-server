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
