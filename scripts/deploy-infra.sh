#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh"

echo ""
echo "Deploying infrastructure for environment: $ENVIRONMENT_NAME"
echo "Creating resource group: $AZURE_RESOURCE_GROUP"
echo ""

az group create \
  --name "$AZURE_RESOURCE_GROUP" \
  --location "$AZURE_LOCATION" \
  --output table

AZURE_SQL_ADMIN_PASSWORD="$("$SCRIPT_DIR/sql-password.sh" ensure)"

echo ""
echo "Ensuring Microsoft Entra app registration: $ENTRA_APP_DISPLAY_NAME"
echo ""

ENTRA_TENANT_ID=$(az account show \
  --query "tenantId" \
  --output tsv)

ENTRA_CLIENT_ID=$(az ad app list \
  --display-name "$ENTRA_APP_DISPLAY_NAME" \
  --query "[0].appId" \
  --output tsv)

if [[ -z "$ENTRA_CLIENT_ID" ]]; then
  ENTRA_CLIENT_ID=$(az ad app create \
    --display-name "$ENTRA_APP_DISPLAY_NAME" \
    --sign-in-audience AzureADMyOrg \
    --query "appId" \
    --output tsv)
fi

ENTRA_OBJECT_ID=$(az ad app show \
  --id "$ENTRA_CLIENT_ID" \
  --query "id" \
  --output tsv)

ENTRA_REDIRECT_URIS_JSON=$(node -e "process.stdout.write(JSON.stringify(['http://localhost:5173/auth/callback', 'https://' + process.argv[1] + '/auth/callback']))" "$AZURE_DOMAIN_NAME")
ENTRA_PATCH_BODY=$(node -e "process.stdout.write(JSON.stringify({ spa: { redirectUris: JSON.parse(process.argv[1]) } }))" "$ENTRA_REDIRECT_URIS_JSON")

az rest \
  --method PATCH \
  --uri "https://graph.microsoft.com/v1.0/applications/$ENTRA_OBJECT_ID" \
  --headers "Content-Type=application/json" \
  --body "$ENTRA_PATCH_BODY" \
  --output none

echo ""
echo "Deploying Bicep template..."
echo ""

az deployment group create \
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
    sqlAdministratorLogin="$AZURE_SQL_ADMIN_LOGIN" \
    sqlAdministratorPassword="$AZURE_SQL_ADMIN_PASSWORD" \
    sqlDatabaseName="$AZURE_SQL_DATABASE_NAME" \
  --output table

STORAGE_ACCOUNT_NAME=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.storageAccountName.value" \
  --output tsv)

if [[ "$AZURE_STORAGE_AUTH_MODE" == "key" ]]; then
  STORAGE_ACCOUNT_KEY=$(az storage account keys list \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --account-name "$STORAGE_ACCOUNT_NAME" \
    --query "[0].value" \
    --output tsv)
  STORAGE_AUTH_ARGS=(--account-key "$STORAGE_ACCOUNT_KEY")
else
  STORAGE_AUTH_ARGS=(--auth-mode "$AZURE_STORAGE_AUTH_MODE")
fi

echo ""
echo "Enabling static website hosting on: $STORAGE_ACCOUNT_NAME"
echo ""

az storage blob service-properties update \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --static-website \
  --index-document index.html \
  --404-document index.html \
  "${STORAGE_AUTH_ARGS[@]}" \
  --output table

echo ""
echo "Infrastructure deployment complete."
echo "Environment: $ENVIRONMENT_NAME"
echo "Domain: https://$AZURE_DOMAIN_NAME"
echo "Entra client ID: $ENTRA_CLIENT_ID"
echo "Azure SQL server: $(az deployment group show --resource-group "$AZURE_RESOURCE_GROUP" --name "$AZURE_DEPLOYMENT_NAME" --query "properties.outputs.sqlServerFullyQualifiedDomainName.value" --output tsv)"
echo "Azure SQL database: $(az deployment group show --resource-group "$AZURE_RESOURCE_GROUP" --name "$AZURE_DEPLOYMENT_NAME" --query "properties.outputs.sqlDatabaseName.value" --output tsv)"
echo ""
