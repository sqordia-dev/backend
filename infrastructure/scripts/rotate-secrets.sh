#!/usr/bin/env bash
# =============================================================================
# Secret Rotation Script for Sqordia Production
# =============================================================================
# Run this after deploying Key Vault integration to rotate all secrets
# that were previously exposed as plaintext environment variables.
#
# Prerequisites:
#   - az CLI logged in with Key Vault Set permission
#   - openssl installed (for key generation)
#
# Usage:
#   chmod +x rotate-secrets.sh
#   ./rotate-secrets.sh [--dry-run]
# =============================================================================

set -euo pipefail

VAULT_NAME="${VAULT_NAME:-sqordia-production-kv}"
DRY_RUN=false

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=true ;;
    --vault=*) VAULT_NAME="${arg#--vault=}" ;;
  esac
done

if [[ "$DRY_RUN" == true ]]; then
  echo "=== DRY RUN MODE — no changes will be made ==="
fi

rotate_secret() {
  local name="$1"
  local value="$2"
  local description="$3"

  echo "  Rotating: $name ($description)"
  if [[ "$DRY_RUN" == false ]]; then
    az keyvault secret set \
      --vault-name "$VAULT_NAME" \
      --name "$name" \
      --value "$value" \
      --output none
    echo "    ✓ Updated"
  else
    echo "    [dry-run] Would update with new value"
  fi
}

# Derive environment from vault name (sqordia-<env>-kv -> <env>)
ENV_NAME=$(echo "$VAULT_NAME" | sed 's/sqordia-\(.*\)-kv/\1/')
RG_NAME="sqordia-${ENV_NAME}-rg"
DB_NAME="sqordia-${ENV_NAME}-postgres"
API_NAME="sqordia-${ENV_NAME}-api"

echo ""
echo "=== Sqordia Secret Rotation ==="
echo "Vault:       $VAULT_NAME"
echo "Environment: $ENV_NAME"
echo "Resource RG: $RG_NAME"
echo ""

# ---------------------------------------------------------------
# 1. PostgreSQL Admin Password
# ---------------------------------------------------------------
echo "[1/7] PostgreSQL Admin Password"
NEW_DB_PASSWORD=$(openssl rand -base64 32 | tr -d '=/+' | head -c 32)
NEW_DB_CONN="Host=\$(az postgres flexible-server show --name ${DB_NAME} --resource-group ${RG_NAME} --query fullyQualifiedDomainName -o tsv);Port=5432;Database=SqordiaDb;Username=sqordia_admin;Password=${NEW_DB_PASSWORD};SSL Mode=Require;"

if [[ "$DRY_RUN" == false ]]; then
  echo "  Updating PostgreSQL password..."
  az postgres flexible-server update \
    --resource-group ${RG_NAME} \
    --name ${DB_NAME} \
    --admin-password "$NEW_DB_PASSWORD" \
    --output none
  echo "    ✓ PostgreSQL password updated"

  # Update connection string secrets in Key Vault
  DB_FQDN=$(az postgres flexible-server show --name ${DB_NAME} --resource-group ${RG_NAME} --query fullyQualifiedDomainName -o tsv)
  NEW_CONN="Host=${DB_FQDN};Port=5432;Database=SqordiaDb;Username=sqordia_admin;Password=${NEW_DB_PASSWORD};SSL Mode=Require;"
  rotate_secret "database-connection-string" "$NEW_CONN" "legacy format"
  rotate_secret "ConnectionStrings--DefaultConnection" "$NEW_CONN" "ASP.NET Core format"
else
  echo "  [dry-run] Would reset PostgreSQL password and update Key Vault"
fi

# ---------------------------------------------------------------
# 2. JWT Secret
# ---------------------------------------------------------------
echo ""
echo "[2/7] JWT Secret"
NEW_JWT=$(openssl rand -base64 64 | tr -d '=/+\n' | head -c 64)
rotate_secret "JwtSettings--Secret" "$NEW_JWT" "JWT signing key"
echo "  ⚠ WARNING: All existing user sessions will be invalidated"

# ---------------------------------------------------------------
# 3. AI Service Key (.NET ↔ Python shared secret)
# ---------------------------------------------------------------
echo ""
echo "[3/7] AI Service Key"
NEW_AI_KEY=$(openssl rand -base64 48)
rotate_secret "AI--PythonService--ServiceKey" "$NEW_AI_KEY" "ASP.NET Core format"
rotate_secret "ai-service-key" "$NEW_AI_KEY" "legacy format for Python Functions"
echo "  ⚠ NOTE: Restart both .NET API and Python AI service after rotation"

# ---------------------------------------------------------------
# 4. API Keys (rotate in provider dashboards first, then update KV)
# ---------------------------------------------------------------
echo ""
echo "[4/7] AI Provider API Keys"
echo "  These must be rotated in each provider's dashboard first:"
echo "    - OpenAI:    https://platform.openai.com/api-keys"
echo "    - Anthropic: https://console.anthropic.com/settings/keys"
echo "    - Google AI: https://aistudio.google.com/app/apikey"
if [[ "$DRY_RUN" == true ]]; then
  echo ""
  echo "  [dry-run] Would prompt for new API keys (OpenAI, Anthropic, Gemini)"
else
  echo ""
  read -rp "  Enter new OpenAI API key (or press Enter to skip): " NEW_OPENAI_KEY
  if [[ -n "$NEW_OPENAI_KEY" ]]; then
    rotate_secret "AI--OpenAI--ApiKey" "$NEW_OPENAI_KEY" "ASP.NET Core format"
    rotate_secret "openai-api-key" "$NEW_OPENAI_KEY" "legacy format for Python Functions"
  fi

  read -rp "  Enter new Anthropic API key (or press Enter to skip): " NEW_ANTHROPIC_KEY
  if [[ -n "$NEW_ANTHROPIC_KEY" ]]; then
    rotate_secret "AI--Claude--ApiKey" "$NEW_ANTHROPIC_KEY" "ASP.NET Core format"
    rotate_secret "anthropic-api-key" "$NEW_ANTHROPIC_KEY" "legacy format for Python Functions"
  fi

  read -rp "  Enter new Google AI API key (or press Enter to skip): " NEW_GOOGLE_KEY
  if [[ -n "$NEW_GOOGLE_KEY" ]]; then
    rotate_secret "AI--Gemini--ApiKey" "$NEW_GOOGLE_KEY" "ASP.NET Core format"
    rotate_secret "google-ai-api-key" "$NEW_GOOGLE_KEY" "legacy format for Python Functions"
  fi
fi

# ---------------------------------------------------------------
# 5. Google OAuth Client Secret
# ---------------------------------------------------------------
echo ""
echo "[5/7] Google OAuth Client Secret"
echo "  Rotate at: https://console.cloud.google.com/apis/credentials"
if [[ "$DRY_RUN" == true ]]; then
  echo "  [dry-run] Would prompt for new client secret"
else
  read -rp "  Enter new client secret (or press Enter to skip): " NEW_OAUTH_SECRET
  if [[ -n "$NEW_OAUTH_SECRET" ]]; then
    rotate_secret "GoogleOAuth--ClientSecret" "$NEW_OAUTH_SECRET" "OAuth client secret"
  fi
fi

# ---------------------------------------------------------------
# 6. Stripe Keys
# ---------------------------------------------------------------
echo ""
echo "[6/7] Stripe Keys"
echo "  Rotate at: https://dashboard.stripe.com/apikeys"
if [[ "$DRY_RUN" == true ]]; then
  echo "  [dry-run] Would prompt for new Stripe keys"
else
  read -rp "  Enter new Stripe secret key (or press Enter to skip): " NEW_STRIPE_KEY
  if [[ -n "$NEW_STRIPE_KEY" ]]; then
    rotate_secret "Stripe--SecretKey" "$NEW_STRIPE_KEY" "Stripe secret key"
  fi

  read -rp "  Enter new Stripe webhook secret (or press Enter to skip): " NEW_STRIPE_WEBHOOK
  if [[ -n "$NEW_STRIPE_WEBHOOK" ]]; then
    rotate_secret "Stripe--WebhookSecret" "$NEW_STRIPE_WEBHOOK" "Stripe webhook secret"
  fi
fi

# ---------------------------------------------------------------
# 7. Restart services to pick up new secrets
# ---------------------------------------------------------------
echo ""
echo "[7/7] Restart Services"
if [[ "$DRY_RUN" == false ]]; then
  echo "  Restarting Container App..."
  az containerapp revision restart \
    --name ${API_NAME} \
    --resource-group ${RG_NAME} \
    --revision "$(az containerapp revision list --name ${API_NAME} --resource-group ${RG_NAME} --query '[0].name' -o tsv)" \
    --output none 2>/dev/null || echo "  ⚠ Restart manually: az containerapp update --name ${API_NAME} -g ${RG_NAME}"

  echo "  Restarting Function Apps..."
  az functionapp restart --name sqordia-${ENV_NAME}-ai-generation-handler --resource-group ${RG_NAME} --output none 2>/dev/null || true
  az functionapp restart --name sqordia-${ENV_NAME}-export-handler --resource-group ${RG_NAME} --output none 2>/dev/null || true
  az functionapp restart --name sqordia-${ENV_NAME}-ai-service --resource-group ${RG_NAME} --output none 2>/dev/null || true
  echo "    ✓ Services restarted"
else
  echo "  [dry-run] Would restart Container App and Function Apps"
fi

echo ""
echo "=== Rotation Complete ==="
echo ""
echo "IMPORTANT: Update terraform.tfvars with the new values to keep Terraform in sync."
echo "  postgresql_admin_password = \"${NEW_DB_PASSWORD:-<not rotated>}\""
echo "  jwt_secret               = \"${NEW_JWT:-<not rotated>}\""
echo "  ai_service_key           = \"${NEW_AI_KEY:-<not rotated>}\""
echo ""
echo "Then run: terraform plan (should show no changes if values match)"
