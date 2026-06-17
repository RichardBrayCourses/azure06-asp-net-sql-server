#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh"

if [[ -z "${AZURE_SQL_ADMIN_PASSWORD:-}" ]]; then
  echo "AZURE_SQL_ADMIN_PASSWORD is required to preview Azure SQL changes."
  echo "Example: AZURE_SQL_ADMIN_PASSWORD='choose-a-strong-password' DEPLOY_ENV=$AZURE_ENVIRONMENT pnpm run infra:what-if"
  exit 1
fi

az group create \
  --name "$AZURE_RESOURCE_GROUP" \
  --location "$AZURE_LOCATION" \
  --output none

ENTRA_TENANT_ID=$(az account show \
  --query "tenantId" \
  --output tsv)

ENTRA_CLIENT_ID=$(az ad app list \
  --display-name "$ENTRA_APP_DISPLAY_NAME" \
  --query "[0].appId" \
  --output tsv)

az deployment group what-if \
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
    entraApiScope="$ENTRA_API_SCOPE" \
    sqlAdministratorLogin="$AZURE_SQL_ADMIN_LOGIN" \
    sqlAdministratorPassword="$AZURE_SQL_ADMIN_PASSWORD" \
    sqlDatabaseName="$AZURE_SQL_DATABASE_NAME"
