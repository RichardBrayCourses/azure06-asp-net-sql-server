#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh" "${1:-}"

generate_password() {
  node -e "const crypto = require('crypto'); const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#%+=_'; let value = ''; for (const byte of crypto.randomBytes(32)) value += chars[byte % chars.length]; process.stdout.write(value);"
}

current_principal_json() {
  local account_type
  local account_name

  account_type="$(az account show --query "user.type" --output tsv)"
  account_name="$(az account show --query "user.name" --output tsv)"

  if [[ "$account_type" == "servicePrincipal" ]]; then
    node -e "process.stdout.write(JSON.stringify({ displayName: process.argv[1], sid: process.argv[1], type: 'Application' }));" "$account_name"
  else
    az ad signed-in-user show \
      --query "{displayName:userPrincipalName,sid:id,type:'User'}" \
      --output json 2>/dev/null \
      || az ad user show \
        --id "$account_name" \
        --query "{displayName:userPrincipalName,sid:id,type:'User'}" \
        --output json
  fi
}

ensure_sql_entra_admin() {
  local sql_server_name="$1"
  local principal_json="$2"
  local principal_display_name
  local principal_sid
  local existing_admin_sid

  principal_display_name="$(node -e "const value = JSON.parse(process.argv[1]); process.stdout.write(value.displayName || value.sid);" "$principal_json")"
  principal_sid="$(node -e "const value = JSON.parse(process.argv[1]); process.stdout.write(value.sid || '');" "$principal_json")"

  if [[ -z "$principal_sid" ]]; then
    echo "Could not determine the current Azure principal for SQL Microsoft Entra admin setup." >&2
    exit 1
  fi

  existing_admin_sid="$(az sql server ad-admin list \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --server "$sql_server_name" \
    --query "[0].sid" \
    --output tsv)"

  echo ""
  echo "Setting Azure SQL Microsoft Entra admin: $principal_display_name"
  echo ""

  if [[ -n "$existing_admin_sid" ]]; then
    az sql server ad-admin update \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --server "$sql_server_name" \
      --display-name "$principal_display_name" \
      --object-id "$principal_sid" \
      --output none
  else
    az sql server ad-admin create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --server "$sql_server_name" \
      --display-name "$principal_display_name" \
      --object-id "$principal_sid" \
      --output none
  fi
}

echo ""
echo "Deploying infrastructure for environment: $ENVIRONMENT_NAME"
echo "Creating resource group: $AZURE_RESOURCE_GROUP"
echo ""

az group create \
  --name "$AZURE_RESOURCE_GROUP" \
  --location "$AZURE_LOCATION" \
  --output table

AZURE_SQL_ADMIN_PASSWORD="$(generate_password)"

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

if [[ -z "$ENTRA_API_SCOPE" ]]; then
  ENTRA_API_SCOPE="api://$ENTRA_CLIENT_ID/access_as_user"
fi

ENTRA_API_AUDIENCE="${ENTRA_API_SCOPE%/access_as_user}"
ENTRA_API_SCOPE_ID=$(node -e "const crypto = require('crypto'); const hex = crypto.createHash('sha256').update(process.argv[1]).digest('hex').slice(0, 32); process.stdout.write([hex.slice(0, 8), hex.slice(8, 12), '4' + hex.slice(13, 16), ((parseInt(hex.slice(16, 18), 16) & 0x3f) | 0x80).toString(16).padStart(2, '0') + hex.slice(18, 20), hex.slice(20, 32)].join('-'));" "$ENTRA_API_SCOPE")

ENTRA_REDIRECT_URIS_JSON=$(node -e "process.stdout.write(JSON.stringify(['http://localhost:5173/auth/callback', 'https://' + process.argv[1] + '/auth/callback']))" "$AZURE_DOMAIN_NAME")
ENTRA_PATCH_BODY=$(node -e "process.stdout.write(JSON.stringify({ identifierUris: [process.argv[2]], spa: { redirectUris: JSON.parse(process.argv[1]) }, api: { requestedAccessTokenVersion: 2, oauth2PermissionScopes: [{ id: process.argv[3], adminConsentDescription: 'Allow the All Checks Out shell to call the All Checks Out API as the signed-in user.', adminConsentDisplayName: 'Access All Checks Out API', isEnabled: true, type: 'User', userConsentDescription: 'Allow the All Checks Out shell to call the All Checks Out API as you.', userConsentDisplayName: 'Access All Checks Out API', value: 'access_as_user' }] } }))" "$ENTRA_REDIRECT_URIS_JSON" "$ENTRA_API_AUDIENCE" "$ENTRA_API_SCOPE_ID")

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
    entraAudience="$ENTRA_API_AUDIENCE" \
    demoSignInKey="$DEMO_SIGN_IN_KEY" \
    sqlAdministratorLogin="$AZURE_SQL_ADMIN_LOGIN" \
    sqlAdministratorPassword="$AZURE_SQL_ADMIN_PASSWORD" \
    sqlDatabaseName="$AZURE_SQL_DATABASE_NAME" \
  --output table

SQL_SERVER_NAME=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.sqlServerName.value" \
  --output tsv)

ensure_sql_entra_admin "$SQL_SERVER_NAME" "$(current_principal_json)"

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
echo "API URL: $(az deployment group show --resource-group "$AZURE_RESOURCE_GROUP" --name "$AZURE_DEPLOYMENT_NAME" --query "properties.outputs.apiBaseUrl.value" --output tsv)"
echo "Entra client ID: $ENTRA_CLIENT_ID"
echo "Entra API scope: $ENTRA_API_SCOPE"
echo "Azure SQL server: $(az deployment group show --resource-group "$AZURE_RESOURCE_GROUP" --name "$AZURE_DEPLOYMENT_NAME" --query "properties.outputs.sqlServerFullyQualifiedDomainName.value" --output tsv)"
echo "Azure SQL database: $(az deployment group show --resource-group "$AZURE_RESOURCE_GROUP" --name "$AZURE_DEPLOYMENT_NAME" --query "properties.outputs.sqlDatabaseName.value" --output tsv)"
echo ""
