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
