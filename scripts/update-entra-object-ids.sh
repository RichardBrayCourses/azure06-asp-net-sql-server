#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/config.sh" "${1:-testing}"

MAPPING_FILE="${2:-$MONOREPO_DIR/tmp/entra-test-users/$ENVIRONMENT_NAME-users.jsonl}"
WORK_DIR="$MONOREPO_DIR/tmp/entra-test-users/db-update"

if [[ ! -f "$MAPPING_FILE" ]]; then
  echo "Mapping file not found: $MAPPING_FILE" >&2
  exit 1
fi

SQL_SERVER_FQDN=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$AZURE_DEPLOYMENT_NAME" \
  --query "properties.outputs.sqlServerFullyQualifiedDomainName.value" \
  --output tsv)

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

if [[ -z "$SQL_SERVER_FQDN" || -z "$SQL_SERVER_NAME" || -z "$SQL_DATABASE_NAME" ]]; then
  echo "Azure SQL deployment outputs were not found." >&2
  echo "Run: pnpm run deploy:$ENVIRONMENT_NAME" >&2
  exit 1
fi

if [[ "${AZURE_SQL_CONFIGURE_FIREWALL:-1}" == "1" ]]; then
  CLIENT_IP=$(curl -fsS https://api.ipify.org)
  echo "Allowing current client IP on Azure SQL firewall: $CLIENT_IP"
  az sql server firewall-rule create \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --server "$SQL_SERVER_NAME" \
    --name "AllowEntraObjectIdUpdateClient" \
    --start-ip-address "$CLIENT_IP" \
    --end-ip-address "$CLIENT_IP" \
    --output none
fi

mkdir -p "$WORK_DIR"

cat > "$WORK_DIR/UpdateEntraObjectIds.csproj" <<'PROJECT'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.3" />
  </ItemGroup>
</Project>
PROJECT

cat > "$WORK_DIR/Program.cs" <<'CS'
using System.Text.Json;
using Microsoft.Data.SqlClient;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: UpdateEntraObjectIds <connection-string> <mapping-file>");
    return 1;
}

var connectionString = args[0];
var mappingFile = args[1];

await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

var updated = 0;
foreach (var line in File.ReadLines(mappingFile))
{
    if (string.IsNullOrWhiteSpace(line))
    {
        continue;
    }

    using var document = JsonDocument.Parse(line);
    var root = document.RootElement;
    var userAccountId = root.GetProperty("userAccountId").GetString();
    var email = root.GetProperty("userPrincipalName").GetString();
    var entraObjectId = root.GetProperty("entraObjectId").GetString();

    if (string.IsNullOrWhiteSpace(userAccountId) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(entraObjectId))
    {
        throw new InvalidOperationException("Each mapping row must include userAccountId, userPrincipalName, and entraObjectId.");
    }

    await using var command = connection.CreateCommand();
    command.CommandText = """
        UPDATE [cases].[UserAccounts]
        SET [EntraObjectId] = @entraObjectId,
            [Email] = LOWER(@email),
            [UpdatedAt] = SYSDATETIMEOFFSET()
        WHERE [Id] = @userAccountId;
        """;
    command.Parameters.AddWithValue("@userAccountId", userAccountId);
    command.Parameters.AddWithValue("@email", email);
    command.Parameters.AddWithValue("@entraObjectId", entraObjectId);

    var rowCount = await command.ExecuteNonQueryAsync();
    if (rowCount != 1)
    {
        throw new InvalidOperationException($"Expected to update one row for {userAccountId}, but updated {rowCount}.");
    }

    updated++;
}

Console.WriteLine($"Updated {updated} user account Entra object IDs and email addresses.");
return 0;
CS

CONNECTION_STRING="Server=tcp:$SQL_SERVER_FQDN,1433;Initial Catalog=$SQL_DATABASE_NAME;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connect Timeout=60;ConnectRetryCount=3;ConnectRetryInterval=10;"

dotnet run \
  --project "$WORK_DIR/UpdateEntraObjectIds.csproj" \
  -- "$CONNECTION_STRING" "$MAPPING_FILE"
