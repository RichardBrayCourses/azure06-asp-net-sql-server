#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh" "${1:-}"

PUBLISH_DIR="$(mktemp -d "${TMPDIR:-/tmp}/all-checks-out-api-publish.XXXXXX")"
PACKAGE_DIR="$(mktemp -d "${TMPDIR:-/tmp}/all-checks-out-api-package.XXXXXX")"
PACKAGE_FILE="$PACKAGE_DIR/all-checks-out-api.zip"

cleanup() {
  rm -rf "$PUBLISH_DIR"
  rm -rf "$PACKAGE_DIR"
  rm -f "$PACKAGE_FILE"
}
trap cleanup EXIT

FUNCTION_APP_NAME=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.functionAppName.value" \
  --output tsv)

API_BASE_URL=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.apiBaseUrl.value" \
  --output tsv)

if [[ -z "$FUNCTION_APP_NAME" || -z "$API_BASE_URL" ]]; then
  echo "Function API deployment outputs were not found."
  echo "Run: pnpm run deploy:$ENVIRONMENT_NAME"
  exit 1
fi

echo ""
echo "Publishing Functions API for environment: $ENVIRONMENT_NAME"
echo ""

dotnet publish "$MONOREPO_DIR/services/functions-api/AllChecksOut.FunctionsApi.csproj" \
  --configuration Release \
  --output "$PUBLISH_DIR"

(
  cd "$PUBLISH_DIR"
  zip -qr "$PACKAGE_FILE" .
)

echo ""
echo "Deploying Functions API package to: $FUNCTION_APP_NAME"
echo ""

az functionapp deployment source config-zip \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$FUNCTION_APP_NAME" \
  --src "$PACKAGE_FILE" \
  --output table

echo ""
echo "Waiting for API health endpoint: $API_BASE_URL/health"
echo ""

for attempt in 1 2 3 4 5 6; do
  if curl -fsS "$API_BASE_URL/health" >/dev/null; then
    echo "API deployment complete."
    echo "API URL: $API_BASE_URL"
    echo ""
    exit 0
  fi

  echo "API health check attempt $attempt failed. Waiting 10 seconds before retrying..."
  sleep 10
done

echo "API health check failed: $API_BASE_URL/health"
exit 1
