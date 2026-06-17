#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh"

MIGRATION="${1:-}"

if [[ -z "${AZURE_SQL_CONNECTION_STRING:-}" ]]; then
  if [[ -z "${AZURE_SQL_ADMIN_PASSWORD:-}" ]]; then
    AZURE_SQL_ADMIN_PASSWORD="$("$SCRIPT_DIR/sql-password.sh" get)"
  fi

  SQL_SERVER_FQDN=$(az deployment group show \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_DEPLOYMENT_NAME" \
    --query "properties.outputs.sqlServerFullyQualifiedDomainName.value" \
    --output tsv)

  SQL_DATABASE_NAME=$(az deployment group show \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_DEPLOYMENT_NAME" \
    --query "properties.outputs.sqlDatabaseName.value" \
    --output tsv)

  SQL_ADMIN_LOGIN=$(az deployment group show \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_DEPLOYMENT_NAME" \
    --query "properties.outputs.sqlAdministratorLogin.value" \
    --output tsv)

  if [[ -z "$SQL_SERVER_FQDN" || -z "$SQL_DATABASE_NAME" || -z "$SQL_ADMIN_LOGIN" ]]; then
    echo "Azure SQL deployment outputs were not found."
    echo "Run: DEPLOY_ENV=$AZURE_ENVIRONMENT pnpm run infra:deploy"
    exit 1
  fi

  AZURE_SQL_CONNECTION_STRING="Server=tcp:$SQL_SERVER_FQDN,1433;Initial Catalog=$SQL_DATABASE_NAME;User ID=$SQL_ADMIN_LOGIN;Password=$AZURE_SQL_ADMIN_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
fi

if [[ "${AZURE_SQL_CONFIGURE_FIREWALL:-1}" == "1" ]]; then
  SQL_SERVER_NAME=$(az deployment group show \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "$AZURE_DEPLOYMENT_NAME" \
    --query "properties.outputs.sqlServerName.value" \
    --output tsv)

  if [[ -n "$SQL_SERVER_NAME" ]]; then
    CLIENT_IP=$(curl -fsS https://api.ipify.org)
    echo "Allowing current client IP on Azure SQL firewall: $CLIENT_IP"
    az sql server firewall-rule create \
      --resource-group "$AZURE_RESOURCE_GROUP" \
      --server "$SQL_SERVER_NAME" \
      --name "AllowMigrationClient" \
      --start-ip-address "$CLIENT_IP" \
      --end-ip-address "$CLIENT_IP" \
      --output none
  fi
fi

echo ""
echo "Running EF Core migrations against Azure SQL."
echo "Environment: $ENVIRONMENT_NAME"
echo ""

if [[ -n "$MIGRATION" ]]; then
  dotnet ef database update "$MIGRATION" \
    --project services/cases-api \
    --startup-project services/cases-api \
    --connection "$AZURE_SQL_CONNECTION_STRING"
else
  dotnet ef database update \
    --project services/cases-api \
    --startup-project services/cases-api \
    --connection "$AZURE_SQL_CONNECTION_STRING"
fi
