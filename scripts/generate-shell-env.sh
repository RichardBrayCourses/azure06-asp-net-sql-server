#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh" "${1:-}"

OUTPUT_FILE="$MONOREPO_DIR/apps/shell/.env"
ENVIRONMENT_OUTPUT_FILE="$MONOREPO_DIR/apps/shell/.env.generated.$ENVIRONMENT_NAME"

echo ""
echo "Reading shell deployment configuration from Azure App Configuration..."
echo ""

APP_CONFIGURATION_NAME=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.appConfigurationName.value" \
  --output tsv)

if [[ -z "$APP_CONFIGURATION_NAME" ]]; then
  echo "App Configuration store was not found in deployment outputs."
  echo "Run: pnpm run deploy:$ENVIRONMENT_NAME"
  exit 1
fi

az config set extension.use_dynamic_install=yes_without_prompt >/dev/null

APP_CONFIGURATION_CONNECTION_STRING=$(az appconfig credential list \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$APP_CONFIGURATION_NAME" \
  --query "[0].connectionString" \
  --output tsv)

read_config_value() {
  local key="$1"
  az appconfig kv show \
    --connection-string "$APP_CONFIGURATION_CONNECTION_STRING" \
    --key "$key" \
    --query "value" \
    --output tsv
}

ENTRA_CLIENT_ID=$(read_config_value "VITE_ENTRA_CLIENT_ID")
ENTRA_AUTHORITY=$(read_config_value "VITE_ENTRA_AUTHORITY")
ENTRA_API_SCOPE=$(read_config_value "VITE_ENTRA_API_SCOPE")

echo "Generating $OUTPUT_FILE"
echo "Generating $ENVIRONMENT_OUTPUT_FILE"

cat > "$OUTPUT_FILE" <<EOF
VITE_ENTRA_CLIENT_ID=$ENTRA_CLIENT_ID
VITE_ENTRA_AUTHORITY=$ENTRA_AUTHORITY
VITE_ENTRA_API_SCOPE=$ENTRA_API_SCOPE
EOF

cp "$OUTPUT_FILE" "$ENVIRONMENT_OUTPUT_FILE"

echo ""
echo "Generated:"
echo "$OUTPUT_FILE"
echo "$ENVIRONMENT_OUTPUT_FILE"
echo ""
cat "$OUTPUT_FILE"
echo ""
