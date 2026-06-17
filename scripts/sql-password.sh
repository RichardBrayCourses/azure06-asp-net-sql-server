#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh"

ensure_key_vault() {
  if az keyvault show \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_KEY_VAULT_NAME" \
    --query "name" \
    --output tsv >/dev/null 2>&1; then
    return
  fi

  if az keyvault list-deleted \
    --query "[?name=='$AZURE_KEY_VAULT_NAME'] | [0].name" \
    --output tsv | grep -q "^$AZURE_KEY_VAULT_NAME$"; then
    echo "Purging deleted Azure Key Vault before recreating it: $AZURE_KEY_VAULT_NAME" >&2
    az keyvault purge \
      --name "$AZURE_KEY_VAULT_NAME" \
      --location "$AZURE_LOCATION" \
      --output none
  fi

  echo "Creating Azure Key Vault: $AZURE_KEY_VAULT_NAME"
  az keyvault create \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_KEY_VAULT_NAME" \
    --location "$AZURE_LOCATION" \
    --enable-rbac-authorization false \
    --output none
}

key_vault_exists() {
  az keyvault show \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_KEY_VAULT_NAME" \
    --query "name" \
    --output tsv >/dev/null 2>&1
}

current_principal_object_id() {
  local account_type
  local account_name

  account_type="$(az account show --query "user.type" --output tsv)"
  account_name="$(az account show --query "user.name" --output tsv)"

  if [[ "$account_type" == "servicePrincipal" ]]; then
    az ad sp show --id "$account_name" --query "id" --output tsv
  else
    az ad signed-in-user show --query "id" --output tsv 2>/dev/null \
      || az ad user show --id "$account_name" --query "id" --output tsv
  fi
}

ensure_secret_permissions() {
  local object_id

  object_id="$(current_principal_object_id)"
  if [[ -z "$object_id" ]]; then
    echo "Could not determine current Azure principal object ID for Key Vault access." >&2
    exit 1
  fi

  az keyvault set-policy \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_KEY_VAULT_NAME" \
    --object-id "$object_id" \
    --secret-permissions get list set \
    --output none
}

generate_password() {
  node -e "const crypto = require('crypto'); const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#%+=_'; let value = ''; for (const byte of crypto.randomBytes(32)) value += chars[byte % chars.length]; process.stdout.write(value);"
}

get_secret_value() {
  az keyvault secret show \
    --vault-name "$AZURE_KEY_VAULT_NAME" \
    --name "$AZURE_SQL_ADMIN_PASSWORD_SECRET_NAME" \
    --query "value" \
    --output tsv 2>/dev/null || true
}

set_secret_value() {
  local value="$1"
  az keyvault secret set \
    --vault-name "$AZURE_KEY_VAULT_NAME" \
    --name "$AZURE_SQL_ADMIN_PASSWORD_SECRET_NAME" \
    --value "$value" \
    --output none
}

case "${1:-}" in
  ensure)
    az group create \
      --name "$AZURE_RESOURCE_GROUP" \
      --location "$AZURE_LOCATION" \
      --output none

    ensure_key_vault
    ensure_secret_permissions

    EXISTING_PASSWORD="$(get_secret_value)"
    if [[ -n "$EXISTING_PASSWORD" ]]; then
      echo "Using existing Azure SQL password secret: $AZURE_KEY_VAULT_NAME/$AZURE_SQL_ADMIN_PASSWORD_SECRET_NAME" >&2
      printf "%s" "$EXISTING_PASSWORD"
      exit 0
    fi

    if [[ -n "${AZURE_SQL_ADMIN_PASSWORD:-}" ]]; then
      NEW_PASSWORD="$AZURE_SQL_ADMIN_PASSWORD"
      echo "Saving AZURE_SQL_ADMIN_PASSWORD into Key Vault secret: $AZURE_KEY_VAULT_NAME/$AZURE_SQL_ADMIN_PASSWORD_SECRET_NAME" >&2
    else
      NEW_PASSWORD="$(generate_password)"
      echo "Generated Azure SQL password and saved it into Key Vault secret: $AZURE_KEY_VAULT_NAME/$AZURE_SQL_ADMIN_PASSWORD_SECRET_NAME" >&2
    fi

    set_secret_value "$NEW_PASSWORD"
    printf "%s" "$NEW_PASSWORD"
    ;;
  get)
    if ! key_vault_exists; then
      echo "Azure Key Vault was not found: $AZURE_KEY_VAULT_NAME" >&2
      echo "Run: DEPLOY_ENV=$AZURE_ENVIRONMENT pnpm run infra:deploy" >&2
      exit 1
    fi

    ensure_secret_permissions

    EXISTING_PASSWORD="$(get_secret_value)"
    if [[ -z "$EXISTING_PASSWORD" ]]; then
      echo "Azure SQL password secret was not found: $AZURE_KEY_VAULT_NAME/$AZURE_SQL_ADMIN_PASSWORD_SECRET_NAME" >&2
      echo "Run: DEPLOY_ENV=$AZURE_ENVIRONMENT pnpm run infra:deploy" >&2
      exit 1
    fi

    printf "%s" "$EXISTING_PASSWORD"
    ;;
  *)
    echo "Usage: DEPLOY_ENV=<environment> scripts/sql-password.sh ensure|get" >&2
    exit 1
    ;;
esac
