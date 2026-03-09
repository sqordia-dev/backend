# =============================================================================
# Azure Functions — Python AI Microservice (LangChain, MLflow, RAGAS)
#
# Deploys the Python AI service as Azure Functions (Consumption plan).
# The .NET backend calls these functions via HTTP triggers.
# AI provider API keys are stored in Key Vault and referenced as app settings.
# =============================================================================

# --- Service Plan (dedicated for Python AI — separate from .NET functions) ---
# Python Functions require Linux with a Python stack, which can't share the
# .NET function plan. Using Flex Consumption for scale-to-zero + fast cold starts.

resource "azurerm_service_plan" "ai_functions" {
  name                = "${var.project_name}-${var.environment}-ai-functions-plan"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "Y1" # Consumption plan (pay-per-execution)

  tags = merge(
    var.common_tags,
    {
      Name    = "${var.project_name}-${var.environment}-ai-functions-plan"
      Service = "ai-service"
    }
  )
}

# --- Storage Account for AI Functions ---

resource "azurerm_storage_account" "ai_functions" {
  name                     = "${var.project_name}${var.environment}aifunc"
  resource_group_name      = data.azurerm_resource_group.main.name
  location                 = data.azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  tags = merge(
    var.common_tags,
    {
      Name    = "${var.project_name}-${var.environment}-ai-functions-storage"
      Service = "ai-service"
    }
  )
}

# --- Application Insights for AI Functions ---

resource "azurerm_application_insights" "ai_functions" {
  name                = "${var.project_name}-${var.environment}-ai-insights"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  tags = merge(
    var.common_tags,
    {
      Name    = "${var.project_name}-${var.environment}-ai-insights"
      Service = "ai-service"
    }
  )
}

# --- Key Vault Secrets for AI Provider Keys ---

resource "azurerm_key_vault_secret" "anthropic_api_key" {
  name         = "anthropic-api-key"
  value        = var.anthropic_api_key
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
  tags       = var.common_tags
}

resource "azurerm_key_vault_secret" "openai_api_key" {
  name         = "openai-api-key"
  value        = var.openai_api_key
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
  tags       = var.common_tags
}

resource "azurerm_key_vault_secret" "google_ai_api_key" {
  name         = "google-ai-api-key"
  value        = var.google_ai_api_key
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
  tags       = var.common_tags
}

resource "azurerm_key_vault_secret" "ai_service_key" {
  name         = "ai-service-key"
  value        = var.ai_service_key
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
  tags       = var.common_tags
}

# --- User Assigned Identity for AI Functions (Key Vault access) ---

resource "azurerm_user_assigned_identity" "ai_functions" {
  name                = "${var.project_name}-${var.environment}-ai-functions-identity"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name

  tags = var.common_tags
}

resource "azurerm_key_vault_access_policy" "ai_functions" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.ai_functions.principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# --- Python AI Function App ---

resource "azurerm_linux_function_app" "ai_service" {
  name                 = "${var.project_name}-${var.environment}-ai-service"
  resource_group_name  = data.azurerm_resource_group.main.name
  location             = data.azurerm_resource_group.main.location
  service_plan_id      = azurerm_service_plan.ai_functions.id
  storage_account_name = azurerm_storage_account.ai_functions.name

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.ai_functions.id]
  }

  site_config {
    application_stack {
      python_version = "3.11"
    }

    # CORS — only allow .NET backend and frontend
    cors {
      allowed_origins = [
        "https://${var.project_name}-${var.environment}-api.${data.azurerm_resource_group.main.location}.azurecontainerapps.io",
        var.frontend_base_url,
      ]
    }

    # Health check
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5

    # Timeouts for long-running AI calls
    app_command_line = ""
  }

  app_settings = {
    # Runtime
    FUNCTIONS_WORKER_RUNTIME = "python"
    AzureWebJobsFeatureFlags = "EnableWorkerIndexing"

    # Application Insights
    APPINSIGHTS_INSTRUMENTATIONKEY        = azurerm_application_insights.ai_functions.instrumentation_key
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.ai_functions.connection_string

    # AI Provider Keys (from Key Vault)
    ANTHROPIC_API_KEY = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.anthropic_api_key.id})"
    OPENAI_API_KEY    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.openai_api_key.id})"
    GOOGLE_API_KEY    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.google_ai_api_key.id})"

    # AI Provider Configuration
    AI_ACTIVE_PROVIDER    = var.ai_active_provider
    AI_FALLBACK_PROVIDERS = jsonencode(var.ai_fallback_providers)
    ANTHROPIC_MODEL       = var.ai_anthropic_model
    OPENAI_MODEL          = var.openai_model
    GOOGLE_MODEL          = var.ai_google_model

    # Service-to-service auth key (shared with .NET backend)
    AI_SERVICE_KEY = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.ai_service_key.id})"

    # MLflow tracking — points to the MLflow Container App
    MLFLOW_TRACKING_URI = "https://${azurerm_container_app.mlflow.ingress[0].fqdn}"

    # Logging
    LOG_LEVEL = var.ai_service_log_level

    # Storage
    WEBSITE_CONTENTSHARE                     = "${var.project_name}-${var.environment}-ai-service-content"
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING = azurerm_storage_account.ai_functions.primary_connection_string

    # Scale & performance
    FUNCTIONS_WORKER_PROCESS_COUNT = "1"

    # Canada data residency (Law 25 compliance)
    WEBSITE_RUN_FROM_PACKAGE = "1"
  }

  tags = merge(
    var.common_tags,
    {
      Name    = "${var.project_name}-${var.environment}-ai-service"
      Service = "ai-service"
    }
  )

  depends_on = [
    azurerm_key_vault_access_policy.ai_functions,
  ]
}

# --- Wire .NET backend to call AI Functions ---
# Add the AI service URL to the Container App environment variables

resource "azurerm_key_vault_secret" "ai_service_url" {
  name         = "ai-service-base-url"
  value        = "https://${azurerm_linux_function_app.ai_service.default_hostname}"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [
    azurerm_key_vault_access_policy.current_user,
    azurerm_linux_function_app.ai_service,
  ]

  tags = var.common_tags
}
