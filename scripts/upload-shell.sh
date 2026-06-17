#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh" "${1:-}"

if [ ! -d "$SHELL_DIST_DIR" ]; then
  echo "Build output not found at $SHELL_DIST_DIR"
  echo "Run: pnpm -C apps/shell run build"
  exit 1
fi

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
echo "Uploading $SHELL_DIST_DIR to the $ENVIRONMENT_NAME static website container in $STORAGE_ACCOUNT_NAME"
echo ""

az storage blob upload-batch \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --destination '$web' \
  --source "$SHELL_DIST_DIR" \
  --overwrite true \
  "${STORAGE_AUTH_ARGS[@]}" \
  --output table

echo ""
echo "Upload complete."
echo "Domain: https://$AZURE_DOMAIN_NAME"
echo ""
