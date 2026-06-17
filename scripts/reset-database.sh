#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh" "${1:-}"

SQL_SERVER_NAME=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.sqlServerName.value" \
  --output tsv)

SQL_DATABASE_NAME=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.sqlDatabaseName.value" \
  --output tsv)

if [[ -z "$SQL_SERVER_NAME" || -z "$SQL_DATABASE_NAME" || "$SQL_SERVER_NAME" == "null" || "$SQL_DATABASE_NAME" == "null" ]]; then
  echo ""
  echo "Azure SQL deployment outputs were not found."
  echo "Run first: pnpm run deploy:$ENVIRONMENT_NAME"
  echo ""
  exit 1
fi

if [[ "$ENVIRONMENT_NAME" == "production" ]]; then
  echo ""
  echo "Production database reset requested."
  echo "Resource group: $AZURE_RESOURCE_GROUP"
  echo "SQL server:     $SQL_SERVER_NAME"
  echo "Database:       $SQL_DATABASE_NAME"
  echo ""
  read -r -p "Type RESET-PRODUCTION-DATABASE to continue: " CONFIRMATION

  if [[ "$CONFIRMATION" != "RESET-PRODUCTION-DATABASE" ]]; then
    echo "Production database reset cancelled."
    exit 1
  fi
fi

echo ""
echo "Deleting Azure SQL database for environment: $ENVIRONMENT_NAME"
echo "Resource group: $AZURE_RESOURCE_GROUP"
echo "SQL server:     $SQL_SERVER_NAME"
echo "Database:       $SQL_DATABASE_NAME"
echo ""

if az sql db show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DATABASE_NAME" \
  --output none 2>/dev/null; then
  az sql db delete \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --server "$SQL_SERVER_NAME" \
    --name "$SQL_DATABASE_NAME" \
    --yes
else
  echo "Database does not exist. Continuing with redeploy."
fi

echo ""
echo "Redeploying infrastructure so Azure SQL database is created again."
echo ""

"$SCRIPT_DIR/deploy-infra.sh" "$ENVIRONMENT_NAME"

echo ""
echo "Applying database source to fresh Azure SQL database."
echo ""

"$SCRIPT_DIR/update-database.sh" "$ENVIRONMENT_NAME"

echo ""
echo "Database reset complete for environment: $ENVIRONMENT_NAME"
echo ""
