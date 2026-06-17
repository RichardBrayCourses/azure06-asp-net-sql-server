#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MONOREPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

SELECTED_ENVIRONMENT="${1:-}"

if [[ -z "$SELECTED_ENVIRONMENT" ]]; then
  echo "No environment selected."
  echo "Use one of: testing, staging, production."
  echo "Example: bash scripts/deploy-infra.sh testing"
  exit 1
fi

case "$SELECTED_ENVIRONMENT" in
  testing | staging | production) ;;
  *)
    echo "Unknown environment: $SELECTED_ENVIRONMENT"
    echo "Use one of: testing, staging, production."
    exit 1
    ;;
esac

ENVIRONMENT_FILE="$MONOREPO_DIR/environments/$SELECTED_ENVIRONMENT.json"

if [[ ! -f "$ENVIRONMENT_FILE" ]]; then
  echo "Environment configuration not found: $ENVIRONMENT_FILE"
  exit 1
fi

read_environment_value() {
  local key="$1"
  node -e "const fs = require('fs'); const config = JSON.parse(fs.readFileSync(process.argv[1], 'utf8')); const value = config[process.argv[2]]; if (value === undefined || value === null || value === '') process.exit(1); process.stdout.write(String(value));" "$ENVIRONMENT_FILE" "$key"
}

read_optional_environment_value() {
  local key="$1"
  node -e "const fs = require('fs'); const config = JSON.parse(fs.readFileSync(process.argv[1], 'utf8')); const value = config[process.argv[2]]; if (value === undefined || value === null || value === '') process.exit(0); process.stdout.write(String(value));" "$ENVIRONMENT_FILE" "$key"
}

ENVIRONMENT_NAME="$(read_environment_value environmentName)"
AZURE_LOCATION="${AZURE_LOCATION:-$(read_environment_value location)}"
AZURE_SQL_LOCATION="${AZURE_SQL_LOCATION:-$(read_optional_environment_value sqlLocation)}"
AZURE_SQL_LOCATION="${AZURE_SQL_LOCATION:-$AZURE_LOCATION}"
AZURE_RESOURCE_GROUP="${AZURE_RESOURCE_GROUP:-$(read_environment_value resourceGroup)}"
AZURE_DEPLOYMENT_NAME="${AZURE_DEPLOYMENT_NAME:-$(read_environment_value deploymentName)}"
AZURE_APP_NAME="${AZURE_APP_NAME:-$(read_environment_value appName)}"
AZURE_DOMAIN_NAME="${AZURE_DOMAIN_NAME:-$(read_environment_value domainName)}"
AZURE_STORAGE_AUTH_MODE="${AZURE_STORAGE_AUTH_MODE:-key}"
ENTRA_APP_DISPLAY_NAME="${ENTRA_APP_DISPLAY_NAME:-All Checks Out Azure06 $ENVIRONMENT_NAME}"
ENTRA_API_SCOPE="${ENTRA_API_SCOPE:-}"
AZURE_SQL_ADMIN_LOGIN="${AZURE_SQL_ADMIN_LOGIN:-allchecksoutadmin}"
AZURE_SQL_DATABASE_NAME="${AZURE_SQL_DATABASE_NAME:-AllChecksOut}"
SHELL_DIST_DIR="${SHELL_DIST_DIR:-$MONOREPO_DIR/apps/shell/dist}"
BICEP_TEMPLATE_FILE="${BICEP_TEMPLATE_FILE:-$MONOREPO_DIR/infra/bicep/main.bicep}"
