#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MONOREPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
DATABASE_SOURCE_DIR="$MONOREPO_DIR/services/functions-api/Data/Migrations"

echo ""
echo "Resetting EF Core database source."
echo "This changes repository files only. It does not delete Azure resources or Azure SQL databases."
echo ""

mkdir -p "$DATABASE_SOURCE_DIR"
find "$DATABASE_SOURCE_DIR" -maxdepth 1 -type f -name "*.cs" -delete

echo ""
echo "Current EF Core database source:"
echo ""

dotnet ef migrations list \
  --project "$MONOREPO_DIR/services/functions-api" \
  --startup-project "$MONOREPO_DIR/services/functions-api" \
  --no-connect

echo ""
echo "Database source reset complete."
echo ""
