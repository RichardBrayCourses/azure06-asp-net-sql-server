#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh" "${1:-}"
AZURE_SQL_ADMIN_PASSWORD="${AZURE_SQL_ADMIN_PASSWORD:-WhatIfOnly_ChangeMe12345%}"

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

if [[ -z "$ENTRA_CLIENT_ID" ]]; then
  ENTRA_CLIENT_ID="00000000-0000-0000-0000-000000000000"
fi

if [[ -z "$ENTRA_API_SCOPE" ]]; then
  ENTRA_API_SCOPE="api://$ENTRA_CLIENT_ID/access_as_user"
fi

ENTRA_API_AUDIENCE="${ENTRA_API_SCOPE%/access_as_user}"

az deployment group what-if \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --template-file "$BICEP_TEMPLATE_FILE" \
  --parameters \
    location="$AZURE_LOCATION" \
    sqlLocation="$AZURE_SQL_LOCATION" \
    appName="$AZURE_APP_NAME" \
    environmentName="$ENVIRONMENT_NAME" \
    domainName="$AZURE_DOMAIN_NAME" \
    entraClientId="$ENTRA_CLIENT_ID" \
    entraTenantId="$ENTRA_TENANT_ID" \
    entraApiScope="$ENTRA_API_SCOPE" \
    entraAudience="$ENTRA_API_AUDIENCE" \
    demoSignInKey="$DEMO_SIGN_IN_KEY" \
    sqlAdministratorLogin="$AZURE_SQL_ADMIN_LOGIN" \
    sqlAdministratorPassword="$AZURE_SQL_ADMIN_PASSWORD" \
    sqlDatabaseName="$AZURE_SQL_DATABASE_NAME"
