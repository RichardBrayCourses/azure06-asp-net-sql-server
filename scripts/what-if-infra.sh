#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh"

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
    entraApiScope="$ENTRA_API_SCOPE"
