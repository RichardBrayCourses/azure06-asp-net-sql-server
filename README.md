# Azure 05 - Entra Authentication

## The Simple Idea

This repository deploys the same React static website into three separate Azure environments, with Microsoft Entra sign-in configured separately for each one.

| Environment | Branch       | Azure resource group           | Public URL                           |
| ----------- | ------------ | ------------------------------ | ------------------------------------ |
| Testing     | `testing`    | `all-checks-out-testing-rg`    | `https://testing.all-checks-out.com` |
| Staging     | `staging`    | `all-checks-out-staging-rg`    | `https://staging.all-checks-out.com` |
| Production  | `production` | `all-checks-out-production-rg` | `https://www.all-checks-out.com`     |

Development happens on `main`, but `main` does not deploy automatically.

Code is promoted in one direction:

```text
main
  |
  v
testing
  |
  v
staging
  |
  v
production
```

Each environment has its own Azure resource group, storage account, static website endpoint, App Configuration store, Entra app registration, redirect URI, generated frontend `.env` file, and Cloudflare DNS record.

## The Three Command Families

These commands look similar, but they do different jobs.

| Command family | Example                    | Meaning                                                        |
| -------------- | -------------------------- | -------------------------------------------------------------- |
| `whatif:*`     | `pnpm run whatif:testing`  | Ask Azure what infrastructure would change. This is a preview. |
| `deploy:*`     | `pnpm run deploy:testing`  | Deploy directly from your terminal to Azure.                   |
| `release:*`    | `pnpm run release:testing` | Promote a Git branch and let GitHub Actions deploy.            |

Normal course flow uses `release:*`.

Manual troubleshooting or first Azure smoke tests can use `deploy:*`.

## What Changes Where?

Read this before running commands.

| Command                           | Your machine                                                                                                 | Azure                                                                                                                  | GitHub                                                               | Cloudflare                                |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- | ----------------------------------------- |
| `pnpm install`                    | Installs dependencies into `node_modules`.                                                                   | No change.                                                                                                             | No change.                                                           | No change.                                |
| `pnpm run type-check`             | Runs TypeScript checks.                                                                                      | No change.                                                                                                             | No change.                                                           | No change.                                |
| `pnpm run shell:build`               | Builds `apps/shell/dist`. Needs Vite Entra values in `apps/shell/.env`.                                            | No change.                                                                                                             | No change.                                                           | No change.                                |
| `pnpm run repo:init`              | Creates a local Git repo if needed, then creates any missing course branches. Existing branches are skipped. | No change.                                                                                                             | If `origin` exists, pushes all four branches idempotently.           | No change.                                |
| `pnpm run repo:init <github-url>` | Creates a local Git repo if needed, creates missing branches, and configures `origin`.                       | No change.                                                                                                             | Adds or verifies `origin` and pushes all four branches idempotently. | No change.                                |
| `pnpm run deploy:testing`         | Generates `apps/shell/.env`, builds the UI, and uploads `apps/shell/dist`.                                         | Creates or updates testing infrastructure, Azure SQL, Entra app registration, App Configuration values, and static website files. | No change.                                                           | No change.                                |
| `pnpm run testing:migrate:azure`  | Runs EF Core migrations from your terminal.                                                                   | Applies pending EF Core migrations to the testing Azure SQL database.                                                   | No change.                                                           | No change.                                |
| `pnpm run release:testing`        | Runs Git commands locally.                                                                                   | Not directly. Azure changes later when GitHub Actions deploys.                                                         | Pushes `testing`, triggering GitHub Actions.                         | No change.                                |
| `pnpm run release:staging`        | Runs Git commands locally.                                                                                   | Not directly. Azure changes later when GitHub Actions deploys.                                                         | Pushes `staging`, triggering GitHub Actions.                         | No change.                                |
| `pnpm run release:production`     | Runs Git commands locally.                                                                                   | Not directly. Azure changes later when GitHub Actions deploys.                                                         | Pushes `production`, triggering GitHub Actions.                      | No change.                                |
| `pnpm run destroy:testing`        | Runs Azure CLI.                                                                                              | Deletes `all-checks-out-testing-rg`.                                                                                   | No change.                                                           | No change.                                |
| Cloudflare DNS change             | No local project change.                                                                                     | No change.                                                                                                             | No change.                                                           | Creates or updates public domain routing. |

The environment name matters:

- `*:testing` affects only testing.
- `*:staging` affects only staging.
- `*:production` affects only production.

For example:

```bash
pnpm run deploy:testing
```

changes Azure testing only. It does not change staging, production, GitHub, or Cloudflare.

## What Happens Once And What Happens Many Times?

| Journey event                                   | How often?                                                                            | Main command or action                                                                                              |
| ----------------------------------------------- | ------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| Install dependencies on your machine            | Once per machine, then again when dependencies change                                 | `pnpm install`                                                                                                      |
| Initialise, repair, or publish course branches  | Once per copied repo, or whenever one of the four local or remote branches is missing | `pnpm run repo:init`                                                                                                |
| Configure GitHub Actions access to Azure        | Once per GitHub repo, then again only if you need to replace the credential           | `REPO_PREFIX_CODE=azure06 APP_PREFIX="all-checks-out-$REPO_PREFIX_CODE-github-actions" pnpm run setup:github-azure` |
| Configure the Azure SQL password secret         | Once per GitHub Environment                                                           | Add `AZURE_SQL_ADMIN_PASSWORD` to the `testing`, `staging`, and `production` GitHub Environments                    |
| Deploy testing for the first time               | Once initially, then as needed                                                        | `pnpm run release:testing` or `pnpm run deploy:testing`                                                             |
| Configure testing registered domain             | Once, if it has not already been configured                                           | See [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md). |
| Promote tested work into staging                | Many times                                                                            | `pnpm run release:staging`                                                                                          |
| Configure staging registered domain             | Once, if it has not already been configured                                           | See [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md). |
| Promote approved work into production           | Many times, carefully                                                                 | `pnpm run release:production`                                                                                       |
| Configure production registered domain          | Once, if it has not already been configured                                           | See [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md). |
| Generate local frontend Entra config from Azure | Whenever you want to run the UI locally against an Azure environment                  | `DEPLOY_ENV=testing pnpm run shell:env`                                                                                |
| Preview Azure infrastructure changes            | Whenever useful                                                                       | `pnpm run whatif:testing`                                                                                           |
| Remove an Azure environment                     | Rarely                                                                                | `pnpm run destroy:testing`                                                                                          |

## Full Journey: From Fresh Clone To Production

Do Initial Setup first.

Then choose either Step 4a or Step 4b.

Run terminal commands from the repository root:

```bash
cd /Users/richardbray/src/azure06-asp-net-sql-server
```

## Initial Setup

Do these steps first for both deployment options.

## Step 1: Install Tools

On macOS, install Homebrew if it is missing:

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

Install the required tools.

`gh` is needed for the GitHub Actions deployment option.

```bash
brew install node pnpm azure-cli gh
```

Check the tools:

```bash
node --version
pnpm --version
az version
gh --version
git --version
```

## Step 2: Sign In To Azure

Sign in to Azure:

```bash
az login
```

Check the selected Azure subscription:

```bash
az account show --output table
```

If the wrong subscription is selected:

```bash
az account set --subscription "<subscription-id-or-name>"
```

## Step 3: Install The Project

Install packages:

```bash
pnpm install
```

Check TypeScript:

```bash
pnpm run type-check
```

Do not worry if `pnpm run shell:build` cannot build yet on a completely fresh machine. This lesson's UI needs Entra configuration values before Vite can build it. Those values are generated after the first infrastructure deployment.

## Choose A Deployment Option

After initial setup, choose one of these paths:

- Step 4a: local deployment from your machine.
- Step 4b: GitHub Actions CI/CD deployment.

Local deployment is the shortest path when you want to prove Azure06 works from your terminal.

GitHub Actions deployment is the course release path when you want branch promotion and remote CI/CD.

## Step 4a: Deployment Option 1 - Local Deployment

Use this option when you want your terminal to deploy Azure directly.

Local deployment uses your current working tree. It does not create or repair course branches, push to GitHub, or trigger GitHub Actions.

During local deployment, `apps/shell/.env` and `apps/shell/.env.generated.<environment>` are generated on your machine before the local Vite build runs.

### 4a.1 Deploy Testing

Run this from the Azure06 repository root:

```bash
cd /Users/richardbray/src/azure06-asp-net-sql-server
AZURE_SQL_ADMIN_PASSWORD="<choose-a-strong-password>" pnpm run deploy:testing
```

The correct Azure06 output includes:

```text
Ensuring Microsoft Entra app registration: All Checks Out Azure06 testing
```

It also runs this website deployment chain:

```text
shell:env
  -> shell:build
  -> shell:upload
```

If the output jumps straight from `infra:deploy` to `shell:build`, you are probably in an earlier repo such as Azure04.

### 4a.1b Apply Azure SQL Migrations

The deployment creates Azure SQL, but the ASP.NET Core API does not run EF Core migrations on startup. Apply migrations explicitly:

```bash
AZURE_SQL_ADMIN_PASSWORD="<the-same-password>" pnpm run testing:migrate:azure
```

This command reads the Azure SQL server and database names from the deployment outputs, temporarily allows your current public IP through the SQL firewall, and runs:

```bash
dotnet ef database update --project services/cases-api --startup-project services/cases-api
```

against Azure SQL. It does not touch the local Docker SQL Server.

### 4a.2 Test Testing

If no testing registered domain has been created yet, see the Azure04 registered-domain instructions in [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md).

Open:

```text
https://testing.all-checks-out.com
```

Sign in. The deployed testing site should redirect through Microsoft Entra and then back to:

```text
https://testing.all-checks-out.com/auth/callback
```

### 4a.3 Deploy Staging

Run:

```bash
pnpm run deploy:staging
```

During this local deployment, `apps/shell/.env` and `apps/shell/.env.generated.staging` are generated on your machine before the local Vite build runs.

### 4a.4 Test Staging

If no staging registered domain has been created yet, see the Azure04 registered-domain instructions in [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md).

Open:

```text
https://staging.all-checks-out.com
```

Sign in. The deployed staging site should redirect through Microsoft Entra and then back to:

```text
https://staging.all-checks-out.com/auth/callback
```

### 4a.5 Deploy Production

Run:

```bash
pnpm run deploy:production
```

During this local deployment, `apps/shell/.env` and `apps/shell/.env.generated.production` are generated on your machine before the local Vite build runs.

### 4a.6 Test Production

If no production registered domain has been created yet, see the Azure04 registered-domain instructions in [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md).

Open:

```text
https://www.all-checks-out.com
```

Sign in. The deployed production site should redirect through Microsoft Entra and then back to:

```text
https://www.all-checks-out.com/auth/callback
```

Production is configured to use `www.all-checks-out.com`. The root domain `all-checks-out.com` is not used by these production deployment commands. You can redirect it to `www` separately in Cloudflare later if you want.

## Step 4b: Deployment Option 2 - GitHub Actions Deployment

Use this option when you want branch promotion and remote CI/CD.

GitHub Actions deployment creates `apps/shell/.env` and `apps/shell/.env.generated.<environment>` on the temporary GitHub Actions runner. Those files are used by the remote Vite build, are not committed to Git, are not copied back to your machine, and disappear when the workflow runner is cleaned up.

This is one larger setup-and-release step because GitHub Actions needs branches, a GitHub remote, a GitHub secret, and committed code before it can deploy.

### 4b.1 Sign In To GitHub

Sign in to GitHub:

```bash
gh auth login
```

Check GitHub:

```bash
gh auth status
gh repo view
```

If `gh repo view` fails because this local folder does not have a GitHub remote yet, continue to `4b.2` and use `pnpm run repo:init <github-url>`.

### 4b.2 Create Or Repair The Course Branches

Run:

```bash
pnpm run repo:init
```

If the GitHub remote is missing, run this instead:

```bash
pnpm run repo:init <github-url>
```

Check the branches:

```bash
git branch --list
```

You should see:

```text
main
testing
staging
production
```

If `origin` is configured, `repo:init` also pushes all four branches to GitHub.

### 4b.3 Create The GitHub Actions Azure Credential

GitHub Actions needs permission to deploy into Azure.

Use this repo prefix code:

```bash
REPO_PREFIX_CODE=azure06
```

Run:

```bash
APP_PREFIX="all-checks-out-$REPO_PREFIX_CODE-github-actions" pnpm run setup:github-azure
```

If these instructions are copied into a later repo, change the prefix code.

Examples:

```text
azure04
azure06
azure06
```

Check the GitHub secret:

```bash
gh secret list
```

You should see:

```text
AZURE_CREDENTIALS
```

### 4b.4 Commit And Push Main

Check the files:

```bash
git status
```

Add the files:

```bash
git add .
```

Commit:

```bash
git commit -m "Prepare Azure06 phased delivery"
```

Push `main`:

```bash
git push -u origin main
```

### 4b.5 Release Testing

Run:

```bash
pnpm run release:testing
```

This promotes:

```text
main -> testing
```

Wait for GitHub Actions:

```bash
pnpm run testing:wait-for-deploy
```

GitHub Actions deploys the testing environment. During that deployment it creates or updates the testing Entra app registration, writes the Vite Entra settings into Azure App Configuration, generates `apps/shell/.env`, builds the UI, and uploads the result.

### 4b.6 Test Testing

If no testing registered domain has been created yet, see the Azure04 registered-domain instructions in [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md).

Open:

```text
https://testing.all-checks-out.com
```

Sign in. The deployed testing site should redirect through Microsoft Entra and then back to:

```text
https://testing.all-checks-out.com/auth/callback
```

### 4b.7 Release Staging

Run:

```bash
pnpm run release:staging
```

This promotes:

```text
testing -> staging
```

Wait for GitHub Actions:

```bash
pnpm run staging:wait-for-deploy
```

GitHub Actions deploys the staging environment. During that deployment, `apps/shell/.env` and `apps/shell/.env.generated.staging` are generated on the temporary GitHub Actions runner, used for the remote Vite build, and then discarded when the workflow runner is cleaned up.

### 4b.8 Test Staging

If no staging registered domain has been created yet, see the Azure04 registered-domain instructions in [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md).

Open:

```text
https://staging.all-checks-out.com
```

Sign in. The deployed staging site should redirect through Microsoft Entra and then back to:

```text
https://staging.all-checks-out.com/auth/callback
```

### 4b.9 Release Production

Run:

```bash
pnpm run release:production
```

This promotes:

```text
staging -> production
```

Wait for GitHub Actions:

```bash
pnpm run production:wait-for-deploy
```

GitHub Actions deploys the production environment. During that deployment, `apps/shell/.env` and `apps/shell/.env.generated.production` are generated on the temporary GitHub Actions runner, used for the remote Vite build, and then discarded when the workflow runner is cleaned up.

### 4b.10 Test Production

If no production registered domain has been created yet, see the Azure04 registered-domain instructions in [azure04-github-actions-phased-delivery/README.md](/Users/richardbray/src/azure04-github-actions-phased-delivery/README.md).

Open:

```text
https://www.all-checks-out.com
```

Sign in. The deployed production site should redirect through Microsoft Entra and then back to:

```text
https://www.all-checks-out.com/auth/callback
```

Production is configured to use `www.all-checks-out.com`. The root domain `all-checks-out.com` is not used by these production deployment commands. You can redirect it to `www` separately in Cloudflare later if you want.

## Normal Day-To-Day Release Flow

After first-time setup, the preferred GitHub Actions CI/CD flow is:

```bash
pnpm run release:testing
pnpm run testing:wait-for-deploy

pnpm run release:staging
pnpm run staging:wait-for-deploy

pnpm run release:production
pnpm run production:wait-for-deploy
```

Use them in order.

Do not deploy directly from `main`.

Do not deploy from feature branches.

The local deployment alternative is:

```bash
pnpm run deploy:testing
pnpm run deploy:staging
pnpm run deploy:production
```

Use local deployment when GitHub Actions is not ready yet, or when you deliberately want your terminal to deploy Azure directly.

Local deployment uses your current working tree. It does not promote branches, push to GitHub, or trigger GitHub Actions.

## Useful Check Commands

Check Azure:

```bash
az account show --output table
```

Check GitHub:

```bash
gh auth status
gh repo view
```

Check branches:

```bash
git branch --list
```

Check GitHub Actions runs:

```bash
gh run list --workflow deploy.yml
```

Preview Azure changes:

```bash
pnpm run whatif:testing
pnpm run whatif:staging
pnpm run whatif:production
```

Generate local frontend Entra config from testing:

```bash
DEPLOY_ENV=testing pnpm run shell:env
```

Run the UI locally after generating `apps/shell/.env`:

```bash
pnpm run shell:dev
```

The local Entra redirect URI is:

```text
http://localhost:5173/auth/callback
```

## Delete Azure Environments

Destroy commands delete Azure resource groups. They do not change GitHub or Cloudflare.

Testing:

```bash
pnpm run destroy:testing
```

Staging:

```bash
pnpm run destroy:staging
```

Production:

```bash
pnpm run destroy:production
```

For production, type this when asked:

```text
DELETE-PRODUCTION
```

## How The Deployment Works

The sections below explain what the deployment commands do. They are reference material, not extra setup steps.

The important term is deployment environment. Testing, staging, and production are three separate deployment environments. They share the same source code, but they do not share Azure resource groups, storage accounts, App Configuration stores, Entra app registrations, or public hostnames.

## Deployment Command Flow

In the normal release path, you run a command such as:

```bash
pnpm run release:testing
```

That promotes a Git branch and pushes it to GitHub. GitHub Actions then runs the environment-specific deployment command:

```bash
pnpm run deploy:testing
```

The deployment command expands into this chain:

```text
deploy:<environment>
  -> deploy-everything
  -> infra:deploy
  -> shell:env
  -> shell:build
  -> shell:upload
  -> shell:url
```

`infra:deploy` creates or updates the Azure infrastructure and the Entra app registration.

`shell:env` reads Azure App Configuration and writes the Vite `.env` file.

`shell:build` builds the React app.

`shell:upload` uploads the built files to the Azure Storage static website.

`shell:url` prints the Azure static website URL and the intended public URL.

## Environment Configuration

The environment-specific values live in:

```text
environments/testing.json
environments/staging.json
environments/production.json
```

Each file describes one deployed environment:

```json
{
  "environmentName": "testing",
  "resourceGroup": "all-checks-out-testing-rg",
  "location": "uksouth",
  "appName": "allcheckouttest",
  "deploymentName": "all-checks-out-testing",
  "domainName": "testing.all-checks-out.com"
}
```

`environmentName` is the logical environment name used by the scripts.

`resourceGroup` is the Azure resource group that receives the storage account and App Configuration store.

`location` is the Azure region.

`appName` is a short lowercase prefix used to build Azure resource names.

`deploymentName` is the Azure deployment record name used when scripts read Bicep outputs later.

`domainName` is the public hostname for this environment.

The environment is chosen by the commands in `package.json`:

```json
"deploy:testing": "DEPLOY_ENV=testing pnpm run deploy-everything",
"deploy:staging": "DEPLOY_ENV=staging pnpm run deploy-everything",
"deploy:production": "DEPLOY_ENV=production pnpm run deploy-everything"
```

Those commands only set `DEPLOY_ENV`. They do not manually pass every Azure value.

The shared script `scripts/config.sh` turns `DEPLOY_ENV` into concrete values:

```bash
AZURE_ENVIRONMENT="${AZURE_ENVIRONMENT:-${DEPLOY_ENV:-}}"
ENVIRONMENT_FILE="$MONOREPO_DIR/environments/$AZURE_ENVIRONMENT.json"
ENVIRONMENT_NAME="$(read_environment_value environmentName)"
AZURE_LOCATION="${AZURE_LOCATION:-$(read_environment_value location)}"
AZURE_RESOURCE_GROUP="${AZURE_RESOURCE_GROUP:-$(read_environment_value resourceGroup)}"
AZURE_DEPLOYMENT_NAME="${AZURE_DEPLOYMENT_NAME:-$(read_environment_value deploymentName)}"
AZURE_APP_NAME="${AZURE_APP_NAME:-$(read_environment_value appName)}"
AZURE_DOMAIN_NAME="${AZURE_DOMAIN_NAME:-$(read_environment_value domainName)}"
```

Most deployment scripts start with:

```bash
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh"
```

`SCRIPT_DIR` is set by the first line.

`${BASH_SOURCE[0]}` is the path of the script file that is currently running.

`dirname "${BASH_SOURCE[0]}"` gets the folder that contains that script.

`cd ... && pwd` turns that folder into an absolute path.

So when `scripts/deploy-infra.sh` runs, `SCRIPT_DIR` becomes the absolute path to the `scripts` folder. Then `source "$SCRIPT_DIR/config.sh"` loads `scripts/config.sh` from beside the running script.

This pattern lets the script work even if you launch it from a different current directory.

That is why `deploy-infra.sh`, `what-if-infra.sh`, `upload-shell.sh`, `show-url.sh`, `connect-custom-domain.sh`, `destroy-infra.sh`, and `generate-shell-env.sh` all agree on which environment they are working with.

`scripts/config.sh` also sets the Entra defaults:

```bash
ENTRA_APP_DISPLAY_NAME="${ENTRA_APP_DISPLAY_NAME:-All Checks Out Azure06 $ENVIRONMENT_NAME}"
ENTRA_API_SCOPE="${ENTRA_API_SCOPE:-}"
```

That gives each environment a separate app registration name, such as:

```text
All Checks Out Azure06 testing
All Checks Out Azure06 staging
All Checks Out Azure06 production
```

`ENTRA_API_SCOPE` stays empty while this lesson only uses sign-in. Later API lessons can set it to request an access token for a backend API.

## Infrastructure And Entra

The Bicep file for this lesson is:

```text
infra/bicep/main.bicep
```

It creates:

- the Azure Storage account for Blob static website hosting
- one Azure App Configuration store
- App Configuration key-values that the UI build converts into Vite environment variables

The Entra app registration is created by `scripts/deploy-infra.sh` before Bicep runs, because app registration management goes through Microsoft Graph rather than ordinary Bicep resource deployment in this lesson.

First, the script asks Azure which tenant is currently selected:

```bash
ENTRA_TENANT_ID=$(az account show \
  --query "tenantId" \
  --output tsv)
```

Then it looks for an existing app registration for the selected environment:

```bash
ENTRA_CLIENT_ID=$(az ad app list \
  --display-name "$ENTRA_APP_DISPLAY_NAME" \
  --query "[0].appId" \
  --output tsv)
```

If there is no existing app registration, the script creates one:

```bash
if [[ -z "$ENTRA_CLIENT_ID" ]]; then
  ENTRA_CLIENT_ID=$(az ad app create \
    --display-name "$ENTRA_APP_DISPLAY_NAME" \
    --sign-in-audience AzureADMyOrg \
    --query "appId" \
    --output tsv)
fi
```

`--sign-in-audience AzureADMyOrg` means accounts from this tenant only.

The script then finds the app registration object ID:

```bash
ENTRA_OBJECT_ID=$(az ad app show \
  --id "$ENTRA_CLIENT_ID" \
  --query "id" \
  --output tsv)
```

The client ID identifies the application to MSAL in the browser. The object ID identifies the app registration record in Microsoft Graph, which is needed to patch its SPA redirect URIs.

The redirect URI list contains one local URI and one deployed URI:

```json
[
  "http://localhost:5173/auth/callback",
  "https://testing.all-checks-out.com/auth/callback"
]
```

For staging and production, the second URI uses the staging or production domain instead.

The script sends the redirect URI update to Microsoft Graph:

```bash
az rest \
  --method PATCH \
  --uri "https://graph.microsoft.com/v1.0/applications/$ENTRA_OBJECT_ID" \
  --headers "Content-Type=application/json" \
  --body "$ENTRA_PATCH_BODY" \
  --output none
```

After that, the deploy script passes the generated identity values into Bicep:

```bash
az deployment group create \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --template-file "$BICEP_TEMPLATE_FILE" \
  --parameters \
    location="$AZURE_LOCATION" \
    appName="$AZURE_APP_NAME" \
    environmentName="$ENVIRONMENT_NAME" \
    domainName="$AZURE_DOMAIN_NAME" \
    entraClientId="$ENTRA_CLIENT_ID" \
    entraTenantId="$ENTRA_TENANT_ID" \
    entraApiScope="$ENTRA_API_SCOPE"
```

Bicep receives the generated client ID and tenant ID, then writes frontend build values into Azure App Configuration.

## Bicep Resources

`infra/bicep/main.bicep` starts with ordinary deployment parameters:

```bicep
param location string = resourceGroup().location
param appName string = 'azure02web'
param environmentName string
param domainName string
```

`location`, `appName`, `environmentName`, and `domainName` come from the selected `environments/<environment>.json` file.

The `azure02web` value is only the template fallback. Normal Azure06 deployments pass `allcheckouttest`, `allcheckoutstage`, or `allcheckoutprod` from the environment JSON files.

The Entra parameters are passed in by `scripts/deploy-infra.sh`:

```bicep
param entraClientId string
param entraTenantId string
param entraApiScope string = ''
```

The template builds stable names:

```bicep
var storageAccountName = take('${appName}${uniqueString(resourceGroup().id)}', 24)
var appConfigurationName = take('${appName}-cfg-${uniqueString(resourceGroup().id)}', 50)
var entraAuthority = uri(environment().authentication.loginEndpoint, entraTenantId)
```

`uniqueString(resourceGroup().id)` gives each environment resource group a stable unique suffix.

`take(..., 24)` keeps the storage account name within Azure's 24-character limit.

`take(..., 50)` keeps the App Configuration name within Azure's length limit.

`environment().authentication.loginEndpoint` avoids hardcoding `https://login.microsoftonline.com/`, which keeps the template compatible with Azure cloud environments.

The storage account is the same basic static website host used in earlier lessons:

```bicep
resource websiteStorage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: true
  }
}
```

The App Configuration store holds the values that must be known before the frontend is built:

```bicep
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigurationName
  location: location
  sku: {
    name: appConfigurationSku
  }
}
```

Bicep then writes ordinary environment information:

```bicep
name: 'APP_ENVIRONMENT'
value: environmentName
```

```bicep
name: 'APP_DOMAIN_NAME'
value: domainName
```

And it writes the Vite keys read by the React app:

```bicep
name: 'VITE_ENTRA_CLIENT_ID'
value: entraClientId
```

```bicep
name: 'VITE_ENTRA_AUTHORITY'
value: entraAuthority
```

```bicep
name: 'VITE_ENTRA_API_SCOPE'
value: entraApiScope
```

The final outputs let later scripts find the generated resources:

```bicep
output storageAccountName string = websiteStorage.name
output appConfigurationName string = appConfiguration.name
```

The Bicep file creates the Azure resources. One extra infrastructure step is still done by `scripts/deploy-infra.sh` after the Bicep deployment: it enables Blob static website hosting on the storage account.

## Frontend Build Configuration

This section explains how each environment's Azure and Entra configuration becomes available to the Vite UI at build time.

## Important: Where The Generated .env File Exists

In the normal release flow, the generated frontend environment files are created on the GitHub Actions runner:

```text
apps/shell/.env
apps/shell/.env.generated.<environment>
```

They are used by `vite build` on that temporary GitHub Actions machine.

They do not get copied back to your developer machine.

They are not committed to Git.

They disappear when the GitHub Actions job finishes.

You only get these files on your own machine if you deliberately run a local command that generates them, such as:

```bash
DEPLOY_ENV=testing pnpm run shell:env
```

or a local deployment command, such as:

```bash
pnpm run deploy:testing
```

The current UI reads Entra settings with Vite:

```ts
const clientId = import.meta.env.VITE_ENTRA_CLIENT_ID;
const authority = import.meta.env.VITE_ENTRA_AUTHORITY;
const apiScope = import.meta.env.VITE_ENTRA_API_SCOPE;
```

Those values must exist before either of these commands runs:

```bash
pnpm run shell:dev
pnpm run shell:build
```

For deployed builds, this happens automatically. For example:

```bash
pnpm run deploy:testing
```

runs:

```text
infra:deploy
  -> shell:env
  -> shell:build
  -> shell:upload
```

`pnpm run shell:env` runs `scripts/generate-shell-env.sh`.

The script finds the App Configuration store from the Bicep output:

```bash
APP_CONFIGURATION_NAME=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.appConfigurationName.value" \
  --output tsv)
```

Then it reads the key-values:

```bash
ENTRA_CLIENT_ID=$(read_config_value "VITE_ENTRA_CLIENT_ID")
ENTRA_AUTHORITY=$(read_config_value "VITE_ENTRA_AUTHORITY")
ENTRA_API_SCOPE=$(read_config_value "VITE_ENTRA_API_SCOPE")
```

Finally, it writes:

```text
apps/shell/.env
apps/shell/.env.generated.<environment>
```

For testing, that means:

```text
apps/shell/.env
apps/shell/.env.generated.testing
```

`apps/shell/.env` is the active file. Vite automatically reads this file when `vite dev` or `vite build` runs.

`apps/shell/.env.generated.<environment>` is a labelled copy of the same values. Vite does not read or process this file name. It exists only for human readers, so you can see which values were generated for testing, staging, or production without relying on the active `.env` file name.

The generated file looks like this:

```text
VITE_ENTRA_CLIENT_ID=<application-client-id>
VITE_ENTRA_AUTHORITY=https://login.microsoftonline.com/<tenant-id>
VITE_ENTRA_API_SCOPE=
```

The script deliberately does not write files named `.env.testing`, `.env.staging`, or `.env.production`. Those filenames have special meaning to Vite modes, and using them here would make it easier to accidentally load the wrong environment during a build.

For local development, generate `apps/shell/.env` from an already-deployed Azure environment:

```bash
DEPLOY_ENV=testing pnpm run shell:env
pnpm run shell:dev
```

That local UI will use the testing Entra app registration and the localhost redirect URI:

```text
http://localhost:5173/auth/callback
```

## App Configuration To Vite: The Whole Path

This is the main Azure06 pattern:

```text
environment JSON
  -> scripts/config.sh
  -> scripts/deploy-infra.sh
  -> Entra app registration
  -> Bicep parameters
  -> Azure App Configuration
  -> scripts/generate-shell-env.sh
  -> apps/shell/.env
  -> Vite build
  -> static files uploaded to Azure Storage
```

The goal is the same pattern as the AWS Cognito lesson:

```text
infrastructure creates identity values
deployment stores those values in a cloud configuration service
build script reads the configuration service
Vite embeds the values into the built UI
```

In AWS, the configuration service was SSM Parameter Store.

In this Azure lesson, the configuration service is Azure App Configuration.

The important detail is that Vite environment variables are build-time values. The browser does not read Azure App Configuration directly. The deployment reads App Configuration first, writes `apps/shell/.env`, and only then runs `vite build`.

That means each deployed static website contains the correct Entra values for the environment that built it:

| Build      | Generated values                  | Redirect URI                                       |
| ---------- | --------------------------------- | -------------------------------------------------- |
| Testing    | Testing Entra app registration    | `https://testing.all-checks-out.com/auth/callback` |
| Staging    | Staging Entra app registration    | `https://staging.all-checks-out.com/auth/callback` |
| Production | Production Entra app registration | `https://www.all-checks-out.com/auth/callback`     |

## GitHub Actions

The workflow is defined in:

```text
.github/workflows/deploy.yml
```

It runs on pushes to:

```text
testing
staging
production
```

It does not run on pushes to `main`.

The workflow signs in to Azure using the GitHub secret named:

```text
AZURE_CREDENTIALS
```

Then it chooses the deployment command from the branch name:

```yaml
- name: Deploy testing
  if: github.ref_name == 'testing'
  run: pnpm run deploy:testing

- name: Deploy staging
  if: github.ref_name == 'staging'
  run: pnpm run deploy:staging

- name: Deploy production
  if: github.ref_name == 'production'
  run: pnpm run deploy:production
```

That keeps the rule simple:

```text
testing branch    -> testing Azure environment
staging branch    -> staging Azure environment
production branch -> production Azure environment
```

## If You Just Ran deploy:testing

If you ran:

```bash
pnpm run deploy:testing
```

then you did affect Azure.

You changed only the testing environment:

```text
Resource group: all-checks-out-testing-rg
Public URL:     https://testing.all-checks-out.com
Azure URL:      the *.web.core.windows.net URL printed by the script
```

You did not push Git branches. You did not trigger GitHub Actions. You did not change staging or production.

The command performed these actions:

1. Created or updated the testing Azure resource group.
2. Created or updated the testing Entra app registration.
3. Set the testing SPA redirect URIs in Microsoft Graph.
4. Deployed the Bicep template.
5. Wrote the Entra build values into Azure App Configuration.
6. Enabled Azure Storage static website hosting.
7. Generated `apps/shell/.env` from Azure App Configuration.
8. Built the UI on your machine.
9. Uploaded the built UI files to Azure Storage.
10. Printed the Azure static website URL and the intended public URL.

The public Cloudflare URL works only after the matching Cloudflare DNS record exists and the custom domain has been connected in Azure.

## Previewing Azure Changes

Use What-If when you want to preview infrastructure changes:

```bash
pnpm run whatif:testing
pnpm run whatif:staging
pnpm run whatif:production
```

What-If does not build or upload the website.

The current script creates the resource group first if it is missing, because Azure group-level What-If needs a resource group to run against.

## Reference: Project Structure

```text
.
|-- .github
|   `-- workflows
|       `-- deploy.yml
|-- apps
|   `-- ui
|-- docs
|-- environments
|   |-- production.json
|   |-- staging.json
|   `-- testing.json
|-- infra
|   `-- main.bicep
|-- scripts
|-- package.json
|-- pnpm-lock.yaml
`-- pnpm-workspace.yaml
```

## Reference: Prerequisites

You need:

- Node.js
- pnpm
- Azure CLI
- GitHub CLI
- an Azure subscription
- a signed-in Azure CLI session for local deployments
- a registered domain managed in Cloudflare
- a GitHub repository with an `AZURE_CREDENTIALS` secret for GitHub Actions, created with a repo-specific `APP_PREFIX`

Check your Azure CLI account:

```bash
az account show --output table
```

Sign in if needed:

```bash
az login
```

Official GitHub CLI install page: <https://cli.github.com/>
