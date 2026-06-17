#!/usr/bin/env bash

set -euo pipefail

EXPECTED_TENANT_ID="${EXPECTED_TENANT_ID:-f9868488-77db-4814-89b8-73242cf9f108}"
GRAPH_URI="https://graph.microsoft.com/v1.0/policies/identitySecurityDefaultsEnforcementPolicy"

portal_fallback() {
  cat <<EOF
Could not disable Security Defaults automatically.

Use the portal fallback:
  1. Open https://entra.microsoft.com
  2. Go to Identity > Overview > Properties
  3. Select Manage security defaults
  4. Set Security defaults to Disabled
  5. Save

Microsoft says this requires at least the Conditional Access Administrator role.

For a scriptable path, install PowerShell 7 and Microsoft Graph PowerShell, then
rerun this command:
  pnpm run entra:disable-security-defaults
EOF
}

read_with_azure_cli() {
  az rest --method GET --uri "$GRAPH_URI" --output json 2>/dev/null
}

patch_with_azure_cli() {
  az rest \
    --method PATCH \
    --uri "$GRAPH_URI" \
    --headers "Content-Type=application/json" \
    --body '{"isEnabled":false}' \
    --output none
}

disable_with_azure_cli_if_possible() {
  local current_policy
  local current_enabled
  local updated_policy
  local updated_enabled

  if ! current_policy="$(read_with_azure_cli)"; then
    return 1
  fi

  current_enabled="$(node -e "const policy = JSON.parse(process.argv[1]); process.stdout.write(String(policy.isEnabled));" "$current_policy")"
  echo "Current Security Defaults isEnabled: $current_enabled"

  if [[ "$current_enabled" == "false" ]]; then
    echo "Security Defaults are already disabled."
    return 0
  fi

  patch_with_azure_cli

  updated_policy="$(read_with_azure_cli)"
  updated_enabled="$(node -e "const policy = JSON.parse(process.argv[1]); process.stdout.write(String(policy.isEnabled));" "$updated_policy")"

  echo "Updated Security Defaults isEnabled: $updated_enabled"

  [[ "$updated_enabled" == "false" ]]
}

disable_with_graph_powershell() {
  pwsh -NoProfile -ExecutionPolicy Bypass -Command @'
$ErrorActionPreference = "Stop"

$expectedTenantId = $env:EXPECTED_TENANT_ID

if (-not (Get-Module -ListAvailable Microsoft.Graph.Identity.SignIns)) {
  Write-Host "Installing Microsoft.Graph.Identity.SignIns for current user..."
  Install-Module Microsoft.Graph.Identity.SignIns -Scope CurrentUser -Force -AllowClobber
}

Import-Module Microsoft.Graph.Identity.SignIns

Write-Host "Opening Microsoft Graph sign-in for tenant: $expectedTenantId"
Connect-MgGraph -TenantId $expectedTenantId -Scopes "Policy.Read.All","Policy.ReadWrite.SecurityDefaults" -NoWelcome

$context = Get-MgContext
if ($context.TenantId -ne $expectedTenantId) {
  throw "Refusing to modify Security Defaults for tenant '$($context.TenantId)'. Expected '$expectedTenantId'."
}

$policy = Get-MgPolicyIdentitySecurityDefaultEnforcementPolicy
Write-Host "Current Security Defaults isEnabled: $($policy.IsEnabled)"

if ($policy.IsEnabled -eq $false) {
  Write-Host "Security Defaults are already disabled."
  return
}

Update-MgPolicyIdentitySecurityDefaultEnforcementPolicy -BodyParameter @{ isEnabled = $false }

$updated = Get-MgPolicyIdentitySecurityDefaultEnforcementPolicy
Write-Host "Updated Security Defaults isEnabled: $($updated.IsEnabled)"

if ($updated.IsEnabled -ne $false) {
  throw "Security Defaults still appear to be enabled. Check the Entra admin center."
}

Write-Host "Security Defaults are disabled for the demo tenant."
'@
}

current_tenant_id="$(az account show --query "tenantId" --output tsv 2>/dev/null || true)"

if [[ -z "$current_tenant_id" ]]; then
  echo "Azure CLI is not signed in."
  echo "Run: az login --tenant $EXPECTED_TENANT_ID --allow-no-subscriptions"
  exit 1
fi

if [[ "$current_tenant_id" != "$EXPECTED_TENANT_ID" ]]; then
  echo "Refusing to modify Security Defaults for the current tenant."
  echo "Expected tenant: $EXPECTED_TENANT_ID"
  echo "Current tenant:  $current_tenant_id"
  echo ""
  echo "Sign in to the expected demo tenant:"
  echo "  az login --tenant $EXPECTED_TENANT_ID --allow-no-subscriptions"
  exit 1
fi

echo "Disabling Microsoft Entra Security Defaults for demo tenant: $EXPECTED_TENANT_ID"

if disable_with_azure_cli_if_possible; then
  echo "Security Defaults are disabled for the demo tenant."
  exit 0
fi

echo "Azure CLI cannot manage this Security Defaults policy with the current token."
echo "Falling back to Microsoft Graph PowerShell..."
echo ""

if command -v pwsh >/dev/null 2>&1; then
  disable_with_graph_powershell
  exit 0
fi

echo "PowerShell 7 was not found on this machine."
echo ""
portal_fallback
exit 1
