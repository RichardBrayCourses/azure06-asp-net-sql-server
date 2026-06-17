#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MONOREPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
MIGRATIONS_DIR="$MONOREPO_DIR/services/cases-api/Data/Migrations"

echo ""
echo "Resetting EF Core migrations."
echo "This changes repository files only. It does not delete Azure resources or databases."
echo ""

mkdir -p "$MIGRATIONS_DIR"
find "$MIGRATIONS_DIR" -maxdepth 1 -type f -name "*.cs" -delete

echo ""
echo "Current EF Core migrations:"
echo ""

dotnet ef migrations list \
  --project "$MONOREPO_DIR/services/cases-api" \
  --startup-project "$MONOREPO_DIR/services/cases-api" \
  --no-connect

echo ""
echo "Migration reset complete."
echo ""
