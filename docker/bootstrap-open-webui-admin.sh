#!/bin/sh
set -eu

: "${OPENWEBUI_URL:?OPENWEBUI_URL is required}"
: "${OPENWEBUI_ADMIN_EMAIL:?OPENWEBUI_ADMIN_EMAIL is required}"
: "${OPENWEBUI_ADMIN_PASSWORD:?OPENWEBUI_ADMIN_PASSWORD is required}"
ADMIN_NAME="${OPENWEBUI_ADMIN_NAME:-SecureHome Admin}"

printf 'Waiting for Open WebUI at %s...' "$OPENWEBUI_URL"
attempt=1
while [ "$attempt" -le 60 ]; do
  if curl -sf "${OPENWEBUI_URL}/health" >/dev/null 2>&1; then
    printf ' ready!\n'
    break
  fi
  printf '.'
  sleep 2
  attempt=$((attempt + 1))
  if [ "$attempt" -gt 60 ]; then
    printf '\nTimed out waiting for Open WebUI to become ready.\n' >&2
    exit 1
  fi
done

signup_payload=$(printf '{"email":"%s","password":"%s","name":"%s"}' \
  "$OPENWEBUI_ADMIN_EMAIL" "$OPENWEBUI_ADMIN_PASSWORD" "$ADMIN_NAME")

signup_status=$(curl -s -w "%{http_code}" -o /tmp/signup.json \
  -X POST "${OPENWEBUI_URL}/api/v1/auths/signup" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d "$signup_payload")

case "$signup_status" in
  200)
    printf 'Created default admin account for %s.\n' "$OPENWEBUI_ADMIN_EMAIL"
    ;;
  400)
    if grep -qi "already registered" /tmp/signup.json; then
      printf 'Admin account %s already exists, verifying credentials.\n' "$OPENWEBUI_ADMIN_EMAIL"
    else
      printf 'Unexpected validation failure while creating admin account (status %s).\n' "$signup_status" >&2
      cat /tmp/signup.json >&2
      exit 1
    fi
    ;;
  *)
    printf 'Failed to create admin account, signup returned status %s.\n' "$signup_status" >&2
    cat /tmp/signup.json >&2
    exit 1
    ;;
esac

signin_payload=$(printf '{"email":"%s","password":"%s"}' \
  "$OPENWEBUI_ADMIN_EMAIL" "$OPENWEBUI_ADMIN_PASSWORD")

signin_status=$(curl -s -w "%{http_code}" -o /tmp/signin.json \
  -X POST "${OPENWEBUI_URL}/api/v1/auths/signin" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d "$signin_payload")

if [ "$signin_status" -ne 200 ]; then
  printf 'Admin credential verification failed (status %s).\n' "$signin_status" >&2
  cat /tmp/signin.json >&2
  exit 1
fi

printf 'Default admin credentials verified successfully.\n'
