# Entra Test Users With all-checks-out.com Domains

## Purpose

This document explains how to create demo sign-ins without Gmail, without Gmail `+` aliases, and without exposing a personal `onmicrosoft.com` tenant name.

The plan is to use your real domain:

```text
all-checks-out.com
```

and create environment-specific Microsoft Entra sign-in domains:

```text
testing.all-checks-out.com
staging.all-checks-out.com
production.all-checks-out.com
```

Then Jonathan Price can sign in as:

```text
jonathan.price@testing.all-checks-out.com
```

## Terms

A Microsoft Entra tenant is the Microsoft identity directory that contains the users who sign in to this application.

A custom domain is a domain you own and add to Microsoft Entra so it can be used in usernames.

A user principal name is the username a Microsoft Entra user types when signing in.

An object ID is the stable unique ID that Microsoft Entra assigns to a user.

The `EntraObjectId` column in the application database connects an application user, such as Jonathan Price, to a Microsoft Entra user.

## Step 1: Add The Custom Domains To Microsoft Entra

Open this URL in your browser:

```text
https://entra.microsoft.com
```

Use the search box at the top of the page to search for:

```text
Domain names
```

Open the result called `Domain names`.

Add these custom domains:

```text
testing.all-checks-out.com
staging.all-checks-out.com
production.all-checks-out.com
```

For each domain, Microsoft Entra will show a DNS `TXT` record.

Add that `TXT` record in Cloudflare for `all-checks-out.com`.

Return to Microsoft Entra and verify the domain.

You do not need to buy TLS certificates for this. Microsoft only needs the DNS `TXT` record to prove that you control the domain.

## Step 2: Create The Test Users

### Automated Approach

Run this from the repository root after signing in with Azure CLI:

```bash
az login
```

Set the sign-in domain for the environment you want:

```bash
SIGN_IN_DOMAIN="testing.all-checks-out.com"
OUTPUT_FILE=".entra-test-users.testing.jsonl"
```

Then run this script in the same terminal:

```bash
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

The output file contains the Microsoft Entra object IDs needed in Step 3. It may also contain temporary passwords, so keep it private.

For staging, use:

```bash
SIGN_IN_DOMAIN="staging.all-checks-out.com"
OUTPUT_FILE=".entra-test-users.staging.jsonl"
```

For production, use:

```bash
SIGN_IN_DOMAIN="production.all-checks-out.com"
OUTPUT_FILE=".entra-test-users.production.jsonl"
```

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

For Jonathan Price in testing, use:

```text
User principal name: jonathan.price@testing.all-checks-out.com
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

For Jonathan Price, the application database row should change from this:

```text
UserAccounts.Id: user-jonathan-price
UserAccounts.EntraObjectId: entra-user-jonathan-price
```

to this:

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
jonathan.price@testing.all-checks-out.com
```

Use the temporary password from Microsoft Entra.

Microsoft may ask you to change the password on first sign-in.

## What Success Looks Like

The Microsoft login screen accepts the username.

The login returns to the testing site.

The application recognises the signed-in user as the matching seeded application user.

## What To Check If It Fails

If Microsoft says the account does not exist, check that the user was created in the same Microsoft Entra tenant used by the testing application.

If Microsoft accepts the login but the application does not recognise the user, check the `EntraObjectId` value in the application database.

If the application recognises the wrong user, check that the object ID was copied into the correct `UserAccounts` row.

## Short Version

Add `testing.all-checks-out.com`, `staging.all-checks-out.com`, and `production.all-checks-out.com` as Microsoft Entra custom domains.

Create real Microsoft Entra users under those domains.

Copy each user's Microsoft Entra object ID.

Put each real object ID into the matching application database user row.
