# Azure06 Functions SQL Server

This repository deploys the All Checks Out React shell, Azure Functions API, and Azure SQL database into three Azure environments.

| Environment | Branch | Resource group | Public URL |
| --- | --- | --- | --- |
| Testing | `testing` | `all-checks-out-testing-rg` | `https://testing.all-checks-out.com` |
| Staging | `staging` | `all-checks-out-staging-rg` | `https://staging.all-checks-out.com` |
| Production | `production` | `all-checks-out-production-rg` | `https://www.all-checks-out.com` |

Development happens on `main`. Code is promoted from `main` to `testing`, then `staging`, then `production`.

## Repository Shape

```text
apps/shell              React/Vite shell
services/functions-api  C# Azure Functions API with EF Core migrations
tests/functions-api.Tests
infra/bicep             Azure infrastructure
scripts                 Local deployment and environment commands
```

The shell calls stable API paths such as `/api/me`, `/api/authorities`, `/api/participants`, and `/api/cases`. The deployed `VITE_API_BASE_URL` points at the Function App host.

## Prerequisites

```bash
node --version
pnpm --version
az version
dotnet --version
git --version
```

This repository targets .NET 10. For EF Core database commands:

```bash
dotnet tool install --global dotnet-ef --version 10.0.9
```

For GitHub Actions setup, also install and sign in with GitHub CLI:

```bash
gh --version
gh auth status
```

## Validate Locally

```bash
pnpm install
pnpm run type-check
pnpm run backend:build
pnpm run backend:test
```

## Local Shell Development

Run the shell against a deployed environment:

```bash
pnpm run shell:dev:testing
pnpm run shell:dev:staging
pnpm run shell:dev:production
```

Each command reads Azure App Configuration, writes `apps/shell/.env`, and starts Vite.

## Deploy

Sign in to Azure first:

```bash
az login
az account show --output table
```

Deploy an environment:

```bash
pnpm run deploy:testing
pnpm run deploy:staging
pnpm run deploy:production
```

Each deploy command:

1. Deploys Bicep infrastructure.
2. Applies EF Core migrations to Azure SQL.
3. Publishes and zip-deploys the Azure Functions API.
4. Generates the shell `.env` file.
5. Builds and uploads the shell.
6. Prints public URLs.

## Database Commands

```bash
pnpm run database:update:testing
pnpm run database:reset:testing
pnpm run migration-source:reset
```

EF Core migrations live in `services/functions-api/Data/Migrations`.

## GitHub Actions

Set up branches and Azure credentials once:

```bash
pnpm run repo:init
REPO_PREFIX_CODE=azure06 APP_PREFIX="all-checks-out-$REPO_PREFIX_CODE-github-actions" pnpm run setup:github-azure
```

Promote and wait for deployments:

```bash
pnpm run release:testing
pnpm run wait-for-deploy:testing

pnpm run release:staging
pnpm run wait-for-deploy:staging

pnpm run release:production
pnpm run wait-for-deploy:production
```

## Inspect And Destroy

Preview infrastructure:

```bash
pnpm run whatif:testing
pnpm run whatif:staging
pnpm run whatif:production
```

Print URLs:

```bash
pnpm run url:testing
pnpm run url:staging
pnpm run url:production
```

Delete an environment resource group:

```bash
pnpm run destroy:testing
pnpm run destroy:staging
pnpm run destroy:production
```

Production deletion requires the confirmation text `DELETE-PRODUCTION`.
