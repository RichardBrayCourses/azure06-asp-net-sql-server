# Azure06 ASP.NET SQL Server

## Completely Clean Out An Environment

These commands delete the entire Azure resource group for an environment. That removes the static website storage account, App Configuration store, Azure SQL server, Azure SQL database, and any data in that environment.

Run from the repository root after `az login`:

```bash
cd /Users/richardbray/src/azure06-asp-net-sql-server
az account show --output table
```

Delete testing completely:

```bash
pnpm run destroy:testing
```

Delete staging completely:

```bash
pnpm run destroy:staging
```

Delete production completely:

```bash
pnpm run destroy:production
```

Production deletion asks for confirmation. Type exactly:

```text
DELETE-PRODUCTION
```

Check whether an environment resource group still exists:

```bash
az group exists --name all-checks-out-testing-rg
az group exists --name all-checks-out-staging-rg
az group exists --name all-checks-out-production-rg
```

After a clean-out, redeploy from scratch with:

```bash
pnpm run migrations:reset
pnpm run deploy:testing
pnpm run testing:migrate:azure
```

For a completely fresh testing rebuild while the repo is still being actively shaped, use:

```bash
pnpm run destroy:testing
pnpm run migrations:reset
pnpm run deploy:testing
pnpm run testing:migrate:azure
```

`destroy:testing` deletes Azure resources. `migrations:reset` changes repository files by deleting EF migration source files and leaving the repo with no migration history. `testing:migrate:azure` creates the first fresh `InitialSqlFoundation` migration from the current model if no migrations exist, then applies it to Azure SQL. Keep Azure cleanup and repo cleanup separate so an Azure cleanup command never silently rewrites git files.

This repository deploys the All Checks Out React shell, ASP.NET Core cases API model, and Azure SQL database support into three Azure environments:

| Environment | Branch | Resource group | Public URL |
| --- | --- | --- | --- |
| Testing | `testing` | `all-checks-out-testing-rg` | `https://testing.all-checks-out.com` |
| Staging | `staging` | `all-checks-out-staging-rg` | `https://staging.all-checks-out.com` |
| Production | `production` | `all-checks-out-production-rg` | `https://www.all-checks-out.com` |

## Deploy The System

Run commands from the repository root:

```bash
cd /Users/richardbray/src/azure06-asp-net-sql-server
```

Install dependencies:

```bash
pnpm install
```

Sign in to Azure:

```bash
az login
az account show --output table
```

If the wrong subscription is selected:

```bash
az account set --subscription "<subscription-id-or-name>"
```

### Local Deployment

Use local deployment when you want your terminal to deploy directly to Azure.

First-time testing deployment:

```bash
pnpm run migrations:reset
pnpm run deploy:testing
pnpm run testing:migrate:azure
```

First-time staging deployment:

```bash
pnpm run deploy:staging
pnpm run staging:migrate:azure
```

First-time production deployment:

```bash
pnpm run deploy:production
pnpm run production:migrate:azure
```

Use the same commands for later deployments. The deploy command creates or updates infrastructure and uploads the frontend. The migrate command applies any pending EF Core migrations to Azure SQL.

The SQL administrator password is generated only as a throwaway value required by Azure SQL server creation. It is not typed, stored, or reused. `infra:deploy` sets the current Azure identity as the SQL Microsoft Entra admin, and the Azure migration command authenticates with Microsoft Entra.

### GitHub Actions Deployment

Use GitHub Actions when you want branch promotion and remote deployment.

Set up GitHub branches and the Azure credential once:

```bash
pnpm run repo:init
REPO_PREFIX_CODE=azure06 APP_PREFIX="all-checks-out-$REPO_PREFIX_CODE-github-actions" pnpm run setup:github-azure
```

Commit and push `main`:

```bash
git add .
git commit -m "Prepare Azure06 deployment"
git push -u origin main
```

Release testing:

```bash
pnpm run release:testing
pnpm run testing:wait-for-deploy
```

Promote to staging:

```bash
pnpm run release:staging
pnpm run staging:wait-for-deploy
```

Promote to production:

```bash
pnpm run release:production
pnpm run production:wait-for-deploy
```

GitHub Actions runs `deploy:<environment>` and then `<environment>:migrate:azure`. The Azure login service principal becomes the SQL Microsoft Entra admin during deployment, and migrations authenticate with that same identity. No GitHub SQL password secret is required.

### Preview, Inspect, And Delete

Preview infrastructure changes:

```bash
pnpm run whatif:testing
pnpm run whatif:staging
pnpm run whatif:production
```

Print the Azure static website URL and CNAME target:

```bash
pnpm run shell:url
```

Delete an environment resource group:

```bash
pnpm run destroy:testing
pnpm run destroy:staging
pnpm run destroy:production
```

Production deletion asks for this confirmation:

```text
DELETE-PRODUCTION
```

## Script Configuration

All deployment scripts load `scripts/config.sh`. That file decides which environment is being deployed and converts it into concrete Azure names.

### Environment Selection

The public package scripts set `DEPLOY_ENV`:

```json
"deploy:testing": "DEPLOY_ENV=testing pnpm run deploy-everything",
"deploy:staging": "DEPLOY_ENV=staging pnpm run deploy-everything",
"deploy:production": "DEPLOY_ENV=production pnpm run deploy-everything"
```

`scripts/config.sh` then uses:

```bash
AZURE_ENVIRONMENT="${AZURE_ENVIRONMENT:-${DEPLOY_ENV:-}}"
```

`AZURE_ENVIRONMENT` can override `DEPLOY_ENV`, but normal commands use `DEPLOY_ENV`.

Only these environment names are accepted:

```bash
testing
staging
production
```

Each environment reads one JSON file:

```text
environments/testing.json
environments/staging.json
environments/production.json
```

Each JSON file contains:

```json
{
  "environmentName": "testing",
  "resourceGroup": "all-checks-out-testing-rg",
  "location": "uksouth",
  "sqlLocation": "swedencentral",
  "appName": "allcheckouttest",
  "deploymentName": "all-checks-out-testing",
  "domainName": "testing.all-checks-out.com"
}
```

### Values Produced By `config.sh`

`ENVIRONMENT_NAME` is the logical environment name, such as `testing`.

`AZURE_LOCATION` is the Azure region, defaulting to the JSON `location`.

`AZURE_SQL_LOCATION` is the Azure SQL region, defaulting to JSON `sqlLocation` and then falling back to `AZURE_LOCATION`.

`AZURE_RESOURCE_GROUP` is the resource group for this environment.

`AZURE_DEPLOYMENT_NAME` is the Azure deployment record name. Scripts use this to read Bicep outputs later.

`AZURE_APP_NAME` is the short name prefix used to build Azure resource names.

`AZURE_DOMAIN_NAME` is the public domain name for the environment.

`AZURE_STORAGE_AUTH_MODE` defaults to `key`.

`ENTRA_APP_DISPLAY_NAME` defaults to `All Checks Out Azure06 <environment>`.

`ENTRA_API_SCOPE` defaults to empty.

`AZURE_SQL_ADMIN_LOGIN` defaults to `allchecksoutadmin`.

`AZURE_SQL_DATABASE_NAME` defaults to `AllChecksOut`.

`SHELL_DIST_DIR` defaults to `apps/shell/dist`.

`BICEP_TEMPLATE_FILE` defaults to `infra/bicep/main.bicep`.

### Deployment Command Chain

`pnpm run deploy:testing` expands to:

```text
deploy:testing
  -> deploy-everything
  -> infra:deploy
  -> shell:env
  -> shell:build
  -> shell:upload
  -> shell:url
```

`infra:deploy` runs `scripts/deploy-infra.sh`.

`shell:env` runs `scripts/generate-shell-env.sh`.

`shell:build` runs the Vite build in `apps/shell`.

`shell:upload` runs `scripts/upload-shell.sh`.

`shell:url` runs `scripts/show-url.sh`.

### SQL Authentication Flow

`scripts/deploy-infra.sh` generates an unsaved SQL administrator password for the Azure SQL server deployment parameter, then sets the current Azure identity as the SQL Microsoft Entra admin.

For local deployment, the current identity is the user from `az login`.

For GitHub Actions deployment, the current identity is the service principal from the `AZURE_CREDENTIALS` secret used by `azure/login`.

`scripts/migrate-azure-sql.sh` then uses a Microsoft Entra connection string:

```text
Authentication=Active Directory Default
```

`AZURE_SQL_CONNECTION_STRING` can still override this for one-off troubleshooting.

### Frontend Configuration Flow

`scripts/generate-shell-env.sh` reads the App Configuration store name from the Bicep deployment output:

```bash
properties.outputs.appConfigurationName.value
```

It then reads these keys from Azure App Configuration:

```text
VITE_ENTRA_CLIENT_ID
VITE_ENTRA_AUTHORITY
VITE_ENTRA_API_SCOPE
```

It writes:

```text
apps/shell/.env
apps/shell/.env.generated.<environment>
```

Vite reads `apps/shell/.env` during local dev and build.

## Bicep File Explained

The infrastructure template is `infra/bicep/main.bicep`.

```bicep
@description('The Azure region where the storage account will be created.')
param location string = resourceGroup().location
```

Adds help text for the `location` parameter and defaults the region to the resource group's region.

```bicep
@description('The Azure region where Azure SQL will be created.')
param sqlLocation string = location
```

Allows Azure SQL to use a different region from the static website resources. This is useful when a subscription is blocked from provisioning Azure SQL in the primary environment region.

```bicep
@description('A short lowercase name used to build the storage account name.')
@minLength(3)
@maxLength(16)
param appName string = 'azure02web'
```

Declares the short app/environment name. The min/max decorators keep names usable for Azure resources. The default is only a fallback; deployment scripts pass values such as `allcheckouttest`.

```bicep
@description('The deployment environment name.')
param environmentName string
```

Receives `testing`, `staging`, or `production`.

```bicep
@description('The public domain for this environment.')
param domainName string
```

Receives the public hostname, such as `testing.all-checks-out.com`.

```bicep
@description('The Microsoft Entra application client ID for the UI.')
param entraClientId string
```

Receives the browser app registration client ID created by `deploy-infra.sh`.

```bicep
@description('The Microsoft Entra tenant ID used to build the authority URL.')
param entraTenantId string
```

Receives the current Azure tenant ID.

```bicep
@description('Optional API scope requested by the UI.')
param entraApiScope string = ''
```

Allows a backend API scope to be passed to the frontend. It is currently empty unless overridden.

```bicep
@description('The Azure App Configuration SKU.')
@allowed([
  'free'
  'standard'
])
param appConfigurationSku string = 'free'
```

Declares the App Configuration SKU and restricts it to Azure's `free` or `standard` values.

```bicep
@description('The Azure SQL administrator login name.')
param sqlAdministratorLogin string = 'allchecksoutadmin'
```

Sets the SQL administrator username.

```bicep
@description('The Azure SQL administrator password.')
@secure()
param sqlAdministratorPassword string
```

Receives the throwaway SQL password generated by `scripts/deploy-infra.sh`. `@secure()` prevents Azure from logging the value as a normal deployment parameter.

```bicep
@description('The Azure SQL database name.')
param sqlDatabaseName string = 'AllChecksOut'
```

Sets the SQL database name.

```bicep
var storageAccountName = take('${appName}${uniqueString(resourceGroup().id)}', 24)
```

Builds a stable, globally unique storage account name and trims it to Azure's 24-character storage-account limit.

```bicep
var appConfigurationName = take('${appName}-cfg-${uniqueString(resourceGroup().id)}', 50)
```

Builds a stable App Configuration store name.

```bicep
var sqlServerName = take('${appName}-${sqlLocation}-sql-${uniqueString(resourceGroup().id, sqlLocation)}', 63)
```

Builds a stable Azure SQL server name that includes the SQL region. Including `sqlLocation` avoids reusing a failed server name if SQL has to move to another Azure region. `take(..., 63)` trims it to Azure's SQL server name length limit.

```bicep
var entraAuthority = uri(environment().authentication.loginEndpoint, entraTenantId)
```

Builds the Entra authority URL from Azure's current cloud login endpoint and the tenant ID.

```bicep
resource websiteStorage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
```

Starts the storage account resource.

```bicep
  name: storageAccountName
```

Uses the generated storage account name.

```bicep
  location: location
```

Deploys the storage account in the selected region.

```bicep
  sku: {
    name: 'Standard_LRS'
  }
```

Uses locally redundant standard storage.

```bicep
  kind: 'StorageV2'
```

Creates a general-purpose v2 storage account.

```bicep
  properties: {
    allowBlobPublicAccess: true
  }
}
```

Allows public blob access so static website hosting can serve files.

```bicep
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
```

Starts the App Configuration store resource.

```bicep
  name: appConfigurationName
  location: location
  sku: {
    name: appConfigurationSku
  }
}
```

Names it, places it in the selected region, and applies the selected SKU.

```bicep
resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
```

Starts the Azure SQL logical server resource.

```bicep
  name: sqlServerName
  location: sqlLocation
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
  }
}
```

Creates the server in `sqlLocation` with the configured admin login, throwaway password, TLS 1.2 minimum, public network access, and SQL Server API version `12.0`.

```bicep
resource sqlAllowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
```

Starts a firewall rule child resource for the SQL server.

```bicep
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}
```

Adds Azure's special `0.0.0.0` rule, which allows Azure services to reach the SQL server.

```bicep
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01' = {
```

Starts the Azure SQL database resource.

```bicep
  parent: sqlServer
  name: sqlDatabaseName
  location: sqlLocation
```

Creates the database under the SQL server, with the configured database name and SQL region.

```bicep
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
```

Uses a serverless General Purpose Gen5 database with 1 vCore.

```bicep
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    requestedBackupStorageRedundancy: 'Local'
  }
}
```

Auto-pauses after 60 minutes, allows a 0.5 vCore minimum, and uses locally redundant backup storage.

```bicep
resource environmentConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'APP_ENVIRONMENT'
  properties: {
    value: environmentName
    contentType: 'text/plain'
  }
}
```

Writes `APP_ENVIRONMENT` into App Configuration.

```bicep
resource domainConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'APP_DOMAIN_NAME'
  properties: {
    value: domainName
    contentType: 'text/plain'
  }
}
```

Writes `APP_DOMAIN_NAME` into App Configuration.

```bicep
resource entraClientIdConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_ENTRA_CLIENT_ID'
  properties: {
    value: entraClientId
    contentType: 'text/plain'
  }
}
```

Writes the frontend Entra client ID for Vite.

```bicep
resource entraAuthorityConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_ENTRA_AUTHORITY'
  properties: {
    value: entraAuthority
    contentType: 'text/plain'
  }
}
```

Writes the frontend Entra authority URL for Vite.

```bicep
resource entraApiScopeConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_ENTRA_API_SCOPE'
  properties: {
    value: entraApiScope
    contentType: 'text/plain'
  }
}
```

Writes the optional API scope for Vite.

```bicep
output storageAccountName string = websiteStorage.name
```

Outputs the generated storage account name so scripts can upload the frontend.

```bicep
output appConfigurationName string = appConfiguration.name
```

Outputs the App Configuration store name so scripts can read frontend build settings.

```bicep
output sqlServerName string = sqlServer.name
```

Outputs the SQL server resource name for firewall and database commands.

```bicep
output sqlServerFullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
```

Outputs the SQL server hostname used in EF connection strings.

```bicep
output sqlDatabaseName string = sqlDatabase.name
```

Outputs the SQL database name used in EF connection strings.

```bicep
output sqlAdministratorLogin string = sqlAdministratorLogin
```

Outputs the SQL administrator username. The password is never output or stored.

## Database And EF Core Commands

### Local SQL Server

Start local SQL Server in Docker:

```bash
pnpm run sql:up
```

Stop local SQL Server:

```bash
pnpm run sql:down
```

Apply EF migrations to local Docker SQL:

```bash
pnpm run backend:migrate
```

Equivalent direct command:

```bash
dotnet ef database update \
  --project services/cases-api \
  --startup-project services/cases-api
```

The local connection string lives in:

```text
services/cases-api/appsettings.Development.json
```

Override it for one command:

```bash
ConnectionStrings__AllChecksOut="Server=...;Database=...;..." pnpm run backend:migrate
```

### Azure SQL

Deploy Azure SQL and other infrastructure:

```bash
pnpm run deploy:testing
```

Apply EF migrations to Azure SQL:

```bash
pnpm run testing:migrate:azure
```

The migration script:

1. Reads SQL server/database outputs from the Azure deployment.
2. Adds your current public IP to the SQL firewall as `AllowMigrationClient`.
3. Runs `dotnet ef database update` with a Microsoft Entra Azure SQL connection string.

Run a specific target migration in Azure:

```bash
DEPLOY_ENV=testing pnpm run backend:migrate:azure -- InitialSqlFoundation
```

Use an explicit Azure SQL connection string instead of deployment outputs:

```bash
AZURE_SQL_CONNECTION_STRING="Server=tcp:...;Initial Catalog=...;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;" \
  DEPLOY_ENV=testing pnpm run backend:migrate:azure
```

### Create A New EF Migration

Edit the EF model or seed data first, then run:

```bash
dotnet ef migrations add <MigrationName> \
  --project services/cases-api \
  --startup-project services/cases-api \
  --output-dir Data/Migrations
```

### Reset EF Migrations During Development

If no important database has successfully applied the existing migrations yet, you can scrub the EF migration history and return the repo to a pre-migration source state:

```bash
pnpm run migrations:reset
```

The reset script deletes `services/cases-api/Data/Migrations/*.cs` and lists the resulting migration stack without connecting to a database. It should report that no migrations were found.

When you next run `pnpm run testing:migrate:azure`, the Azure migration script creates a new `InitialSqlFoundation` migration from the current model if no migration files exist, then applies it to the Azure SQL database. Use this clean-room workflow during development, not after migrations have been applied to an environment with data you need to keep.

List migrations without connecting to a database:

```bash
dotnet ef migrations list \
  --project services/cases-api \
  --startup-project services/cases-api \
  --no-connect
```

Generate an idempotent SQL script:

```bash
dotnet ef migrations script \
  --project services/cases-api \
  --startup-project services/cases-api \
  --idempotent \
  --output migration.sql
```

Remove the last migration before it has been applied:

```bash
dotnet ef migrations remove \
  --project services/cases-api \
  --startup-project services/cases-api
```

### Reset Azure SQL Back To Seed Data

Testing:

```bash
SQL_SERVER_NAME=$(az deployment group show \
  --resource-group all-checks-out-testing-rg \
  --name all-checks-out-testing \
  --query "properties.outputs.sqlServerName.value" \
  --output tsv)

SQL_DATABASE_NAME=$(az deployment group show \
  --resource-group all-checks-out-testing-rg \
  --name all-checks-out-testing \
  --query "properties.outputs.sqlDatabaseName.value" \
  --output tsv)

az sql db delete \
  --resource-group all-checks-out-testing-rg \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DATABASE_NAME" \
  --yes

pnpm run deploy:testing
pnpm run testing:migrate:azure
```

Staging: replace `testing` with `staging`, use `all-checks-out-staging-rg`, and use deployment name `all-checks-out-staging`.

Production: replace `testing` with `production`, use `all-checks-out-production-rg`, and use deployment name `all-checks-out-production`. This deletes production data.

### Check Database Resources

List SQL servers:

```bash
az sql server list --output table
```

List databases for testing:

```bash
SQL_SERVER_NAME=$(az deployment group show \
  --resource-group all-checks-out-testing-rg \
  --name all-checks-out-testing \
  --query "properties.outputs.sqlServerName.value" \
  --output tsv)

az sql db list \
  --resource-group all-checks-out-testing-rg \
  --server "$SQL_SERVER_NAME" \
  --output table
```

Show the current SQL Microsoft Entra admin:

```bash
az sql server ad-admin list \
  --resource-group all-checks-out-testing-rg \
  --server "$SQL_SERVER_NAME" \
  --output table
```
