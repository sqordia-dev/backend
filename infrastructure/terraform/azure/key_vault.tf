# Key Vault
resource "azurerm_key_vault" "main" {
  name                = "${var.project_name}-${var.environment}-kv" # Must be globally unique
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = var.key_vault_sku

  # Network ACLs
  # TODO: Switch default_action to "Deny" once Container Apps and Functions
  # are deployed with VNet integration + private endpoints. Currently "Allow"
  # because Container Apps (non-VNet) can't reach a Deny-by-default vault
  # through bypass alone. Access is still protected by Entra ID + access policies.
  network_acls {
    default_action = "Allow"
    bypass         = "AzureServices"
  }

  # Enable soft delete (90 days for production safety + Law 25 audit compliance)
  soft_delete_retention_days = 90

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-kv"
    }
  )
}

# Get current Azure client configuration
data "azurerm_client_config" "current" {}

# Key Vault Access Policy - Current User
resource "azurerm_key_vault_access_policy" "current_user" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Recover",
    "Backup",
    "Restore"
  ]
}

# Key Vault Access Policy - Container Apps Identity
resource "azurerm_key_vault_access_policy" "container_apps" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.container_apps.principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# =============================================================================
# Secrets (ASP.NET Core naming: -- maps to : in configuration)
# =============================================================================

# Database Connection String
resource "azurerm_key_vault_secret" "database_connection" {
  name         = "database-connection-string"
  value        = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;"
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# ASP.NET Core config-compatible secret (loaded by Key Vault config provider)
resource "azurerm_key_vault_secret" "connection_string" {
  name         = "ConnectionStrings--DefaultConnection"
  value        = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;"
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# JWT
resource "azurerm_key_vault_secret" "jwt_secret" {
  name         = "JwtSettings--Secret"
  value        = var.jwt_secret
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# Google OAuth
resource "azurerm_key_vault_secret" "google_oauth_client_secret" {
  name         = "GoogleOAuth--ClientSecret"
  value        = var.google_oauth_client_secret
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# AI Provider Keys (ASP.NET Core config naming for Key Vault config provider)
# NOTE: kebab-case versions (openai-api-key, etc.) are in ai_functions.tf
# for Azure Functions @Microsoft.KeyVault references
resource "azurerm_key_vault_secret" "config_openai_api_key" {
  name         = "AI--OpenAI--ApiKey"
  value        = var.openai_api_key
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

resource "azurerm_key_vault_secret" "config_anthropic_api_key" {
  count        = var.anthropic_api_key != "" ? 1 : 0
  name         = "AI--Claude--ApiKey"
  value        = var.anthropic_api_key
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

resource "azurerm_key_vault_secret" "config_google_ai_api_key" {
  count        = var.google_ai_api_key != "" ? 1 : 0
  name         = "AI--Gemini--ApiKey"
  value        = var.google_ai_api_key
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

resource "azurerm_key_vault_secret" "config_ai_service_key" {
  name         = "AI--PythonService--ServiceKey"
  value        = var.ai_service_key
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# Stripe
resource "azurerm_key_vault_secret" "stripe_secret_key" {
  count        = var.stripe_secret_key != "" ? 1 : 0
  name         = "Stripe--SecretKey"
  value        = var.stripe_secret_key
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

resource "azurerm_key_vault_secret" "stripe_webhook_secret" {
  count        = var.stripe_webhook_secret != "" ? 1 : 0
  name         = "Stripe--WebhookSecret"
  value        = var.stripe_webhook_secret
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# Email (Resend)
resource "azurerm_key_vault_secret" "resend_api_key" {
  count        = var.resend_api_key != "" ? 1 : 0
  name         = "Resend--ApiKey"
  value        = var.resend_api_key
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# GitHub PAT (bug reporting integration)
resource "azurerm_key_vault_secret" "github_pat" {
  count        = var.github_pat != "" ? 1 : 0
  name         = "GitHub--PersonalAccessToken"
  value        = var.github_pat
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# NOTE: Legacy kebab-case secrets (openai-api-key, anthropic-api-key, etc.)
# are defined in ai_functions.tf and used by Azure Functions via
# @Microsoft.KeyVault(SecretUri=...) references. Do NOT duplicate them here.

# Azure Storage Connection String
resource "azurerm_key_vault_secret" "storage_connection_string" {
  name         = "AzureStorage--ConnectionString"
  value        = azurerm_storage_account.main.primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

# Azure Service Bus Connection String
resource "azurerm_key_vault_secret" "servicebus_connection_string" {
  name         = "AzureServiceBus--ConnectionString"
  value        = azurerm_servicebus_namespace.main.default_primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_key_vault_access_policy.current_user]
  tags         = var.common_tags
}

