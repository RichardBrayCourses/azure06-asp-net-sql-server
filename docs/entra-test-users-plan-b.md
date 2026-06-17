# Entra Test Users With The Existing Tenant

## Purpose

This document explains how to create demo sign-ins without Gmail and without Gmail `+` aliases.

The plan is to use the existing Microsoft Entra tenant domain:

```text
artyuptickgmail.onmicrosoft.com
```

Then Jonathan Price can sign in as:

```text
jonathan.price@artyuptickgmail.onmicrosoft.com
```

This does not change the website URL:

```text
https://testing.all-checks-out.com
```

## Terms

A Microsoft Entra tenant is the Microsoft identity directory that contains the users who sign in to this application.

An `onmicrosoft.com` domain is a Microsoft-managed domain that can be used for Microsoft Entra usernames.

A user principal name is the username a Microsoft Entra user types when signing in.

An object ID is the stable unique ID that Microsoft Entra assigns to a user.

The `EntraObjectId` column in the application database connects an application user, such as Jonathan Price, to a Microsoft Entra user.

## Step 1: Verify The Existing Tenant Domain

### Azure Portal

Open this URL in your browser:

```text
https://portal.azure.com
```

Use the search box at the top to search for:

```text
Microsoft Entra ID
```

Open `Microsoft Entra ID`.

On the overview page, read:

```text
Primary domain
```

For this project, the expected value is:

```text
artyuptickgmail.onmicrosoft.com
```

The same overview page also shows the tenant ID. For this project, it is:

```text
f9868488-77db-4814-89b8-73242cf9f108
```

### Command Line

Run:

```bash
az login --tenant f9868488-77db-4814-89b8-73242cf9f108
```

When Azure CLI asks:

```text
Select a subscription and tenant (Type a number or Enter for no changes):
```

press Enter.

Then run:

```bash
az rest --method GET --uri "https://graph.microsoft.com/v1.0/domains" --query "value[].id" --output table
```

The expected value is:

```text
artyuptickgmail.onmicrosoft.com
```

## Step 2: Create The Test Users

### Automated Approach

Run this from the repository root. If you already ran `az login` in Step 1, do not run it again.

Create this file:

```text
scripts/create-entra-test-users.sh
```

with this content:

```bash
#!/usr/bin/env bash

set -euo pipefail

SIGN_IN_DOMAIN="${SIGN_IN_DOMAIN:-artyuptickgmail.onmicrosoft.com}"
OUTPUT_FILE="${OUTPUT_FILE:-tmp/entra-test-users/testing-users.jsonl}"
OUTPUT_DIR="$(dirname "$OUTPUT_FILE")"

mkdir -p "$OUTPUT_DIR"

generate_password() {
  node -e "const crypto = require('crypto'); const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789#%+=_'; let value = 'Aco!'; for (const byte of crypto.randomBytes(28)) value += chars[byte % chars.length]; process.stdout.write(value);"
}

create_user() {
  local user_account_id="$1"
  local display_name="$2"
  local user_name="$3"
  local upn="$user_name@$SIGN_IN_DOMAIN"
  local password
  local object_id

  echo "Creating or reusing $display_name <$upn>"

  object_id="$(az ad user show --id "$upn" --query "id" --output tsv 2>/dev/null || true)"

  if [[ -z "$object_id" ]]; then
    password="$(generate_password)"
    object_id="$(az ad user create \
      --display-name "$display_name" \
      --user-principal-name "$upn" \
      --mail-nickname "${user_name//./-}" \
      --password "$password" \
      --force-change-password-next-sign-in true \
      --query "id" \
      --output tsv)"
  else
    password=""
  fi

  node -e "process.stdout.write(JSON.stringify({ userAccountId: process.argv[1], displayName: process.argv[2], userPrincipalName: process.argv[3], entraObjectId: process.argv[4], temporaryPassword: process.argv[5] || undefined }) + '\n')" \
    "$user_account_id" "$display_name" "$upn" "$object_id" "$password" >> "$OUTPUT_FILE"
}

: > "$OUTPUT_FILE"

create_user "user-jonathan-price" "Jonathan Price" "jonathan.price"
create_user "user-amara-singh" "Amara Singh" "amara.singh"
create_user "user-aisha-khan" "Aisha Khan" "aisha.khan"
create_user "user-michael-reeves" "Michael Reeves" "michael.reeves"
create_user "user-lewis-green" "Lewis Green" "lewis.green"
create_user "user-amelia-wright" "Amelia Wright" "amelia.wright"
create_user "user-maya-patel" "Maya Patel" "maya.patel"
create_user "user-owen-clarke" "Owen Clarke" "owen.clarke"
create_user "user-rachel-morgan" "Rachel Morgan" "rachel.morgan"
create_user "user-peter-walsh" "Peter Walsh" "peter.walsh"
create_user "user-sophie-turner" "Sophie Turner" "sophie.turner"
create_user "user-benjamin-foster" "Benjamin Foster" "benjamin.foster"
create_user "user-priya-shah" "Priya Shah" "priya.shah"
create_user "user-george-evans" "George Evans" "george.evans"
create_user "user-ellen-brooks" "Ellen Brooks" "ellen.brooks"
create_user "user-nadia-cole" "Nadia Cole" "nadia.cole"

echo "Saved user mapping to $OUTPUT_FILE"
```

Run it:

```bash
bash scripts/create-entra-test-users.sh
```

The output file is:

```text
tmp/entra-test-users/testing-users.jsonl
```

It contains the Microsoft Entra object IDs needed in Step 3. It may also contain temporary passwords, so keep it private. The `tmp/` folder is ignored by git.

### Manual Approach

Open this URL in your browser:

```text
https://entra.microsoft.com
```

Use the search box at the top of the page to search for:

```text
Users
```

Open the result called `Users`.

Create one user for each test persona.

For Jonathan Price, use:

```text
User principal name: jonathan.price@artyuptickgmail.onmicrosoft.com
Display name: Jonathan Price
Password: let Microsoft generate a temporary password
Account enabled: yes
```

Repeat this for each seeded user.

## Step 3: Update The Application Database

The application database must use the real Microsoft Entra object IDs.

The seeded data currently contains placeholder values like this:

```text
entra-user-jonathan-price
```

Those placeholder values work in automated tests, but they do not work with real Microsoft Entra login.

Replace each placeholder `EntraObjectId` with the real Microsoft Entra object ID for that user.

### Automated Approach

Create this file:

```text
scripts/update-entra-object-ids.sh
```

with this content:

```bash
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
    var entraObjectId = root.GetProperty("entraObjectId").GetString();

    if (string.IsNullOrWhiteSpace(userAccountId) || string.IsNullOrWhiteSpace(entraObjectId))
    {
        throw new InvalidOperationException("Each mapping row must include userAccountId and entraObjectId.");
    }

    await using var command = connection.CreateCommand();
    command.CommandText = """
        UPDATE [cases].[UserAccounts]
        SET [EntraObjectId] = @entraObjectId,
            [UpdatedAt] = SYSDATETIMEOFFSET()
        WHERE [Id] = @userAccountId;
        """;
    command.Parameters.AddWithValue("@userAccountId", userAccountId);
    command.Parameters.AddWithValue("@entraObjectId", entraObjectId);

    var rowCount = await command.ExecuteNonQueryAsync();
    if (rowCount != 1)
    {
        throw new InvalidOperationException($"Expected to update one row for {userAccountId}, but updated {rowCount}.");
    }

    updated++;
}

Console.WriteLine($"Updated {updated} user account Entra object IDs.");
return 0;
CS

CONNECTION_STRING="Server=tcp:$SQL_SERVER_FQDN,1433;Initial Catalog=$SQL_DATABASE_NAME;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connect Timeout=60;ConnectRetryCount=3;ConnectRetryInterval=10;"

dotnet run \
  --project "$WORK_DIR/UpdateEntraObjectIds.csproj" \
  -- "$CONNECTION_STRING" "$MAPPING_FILE"
```

Run it:

```bash
bash scripts/update-entra-object-ids.sh testing
```

The script reads:

```text
tmp/entra-test-users/testing-users.jsonl
```

and updates the testing Azure SQL database.

### Manual Approach

Open the testing Azure SQL database.

For each row in:

```text
tmp/entra-test-users/testing-users.jsonl
```

copy the `entraObjectId` value into the matching `UserAccounts.EntraObjectId` column.

For Jonathan Price, the database row changes from:

```text
UserAccounts.Id: user-jonathan-price
UserAccounts.EntraObjectId: entra-user-jonathan-price
```

to:

```text
UserAccounts.Id: user-jonathan-price
UserAccounts.EntraObjectId: 11111111-2222-3333-4444-555555555555
```

Use Jonathan's real object ID, not the example value above.

## Step 4: Sign In As A Test User

Open the testing site:

```text
https://testing.all-checks-out.com
```

Sign in as:

```text
jonathan.price@artyuptickgmail.onmicrosoft.com
```

Use the temporary password from Microsoft Entra.

Microsoft may ask you to change the password on first sign-in.

## What Success Looks Like

The Microsoft login screen accepts the username.

The login returns to the testing site.

The application recognises the signed-in user as the matching seeded application user.

## What To Check If It Fails

If Microsoft says the account does not exist, check that the user exists in Microsoft Entra.

If Microsoft accepts the login but the application does not recognise the user, check the `EntraObjectId` value in the application database.

If the application recognises the wrong user, check that the object ID was copied into the correct `UserAccounts` row.

## Short Version

Create real Microsoft Entra users under:

```text
artyuptickgmail.onmicrosoft.com
```

Copy each user's Microsoft Entra object ID.

Put each real object ID into the matching application database user row.
