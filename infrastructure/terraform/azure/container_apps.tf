# User Assigned Identity for Container Apps (defined first for dependencies)
resource "azurerm_user_assigned_identity" "container_apps" {
  name                = "${var.project_name}-${var.environment}-container-apps-identity"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name

  tags = var.common_tags
}

# Log Analytics Workspace (required for Container Apps)
resource "azurerm_log_analytics_workspace" "main" {
  name                = "${var.project_name}-${var.environment}-logs"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = max(var.log_analytics_retention_days, 30) # Minimum 30 days for PerGB2018 SKU

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-logs"
    }
  )
}

# Container Apps Environment
resource "azurerm_container_app_environment" "main" {
  name                       = "${var.project_name}-${var.environment}-env"
  location                   = data.azurerm_resource_group.main.location
  resource_group_name        = data.azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-env"
    }
  )
}

# Container App (Main API)
resource "azurerm_container_app" "api" {
  name                         = "${var.project_name}-${var.environment}-api"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"

  template {
    min_replicas = var.container_apps_min_replicas
    max_replicas = var.container_apps_max_replicas

    container {
      name   = "api"
      image  = "${azurerm_container_registry.main.login_server}/api:latest"
      cpu    = var.container_apps_cpu
      memory = "${var.container_apps_memory}Gi"

      # ================================================================
      # Non-sensitive configuration (safe as env vars)
      # ================================================================
      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      # Key Vault URL — app loads all secrets from here at startup
      env {
        name  = "AzureKeyVault__VaultUrl"
        value = azurerm_key_vault.main.vault_uri
      }

      # Managed Identity client ID for Key Vault authentication
      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.container_apps.client_id
      }

      # Azure Storage (connection string loaded from Key Vault)
      env {
        name  = "AzureStorage__AccountName"
        value = azurerm_storage_account.main.name
      }

      env {
        name  = "AzureStorage__ContainerName"
        value = azurerm_storage_container.documents.name
      }

      # Service Bus (connection string loaded from Key Vault)
      env {
        name  = "AzureServiceBus__EmailTopic"
        value = azurerm_servicebus_topic.email.name
      }

      env {
        name  = "AzureServiceBus__AiGenerationTopic"
        value = azurerm_servicebus_topic.ai_generation.name
      }

      env {
        name  = "AzureServiceBus__ExportTopic"
        value = azurerm_servicebus_topic.export.name
      }

      # OAuth (client ID is public, client secret loaded from Key Vault)
      env {
        name  = "GoogleOAuth__ClientId"
        value = var.google_oauth_client_id
      }

      env {
        name  = "GoogleOAuth__RedirectUri"
        value = var.google_oauth_redirect_uri != "" ? var.google_oauth_redirect_uri : "https://${var.project_name}.app/api/v1/auth/google/callback"
      }

      # JWT (non-secret settings, secret loaded from Key Vault)
      env {
        name  = "JwtSettings__Issuer"
        value = var.jwt_issuer
      }

      env {
        name  = "JwtSettings__Audience"
        value = var.jwt_audience
      }

      env {
        name  = "JwtSettings__ExpirationInMinutes"
        value = tostring(var.jwt_expiration_minutes)
      }

      # AI models (non-secret configuration)
      env {
        name  = "AI__DefaultProvider"
        value = var.ai_active_provider
      }

      env {
        name  = "AI__OpenAI__Model"
        value = var.openai_model
      }

      env {
        name  = "AI__Claude__Model"
        value = var.ai_anthropic_model
      }

      env {
        name  = "AI__Gemini__Model"
        value = var.ai_google_model
      }

      # Frontend
      env {
        name  = "FRONTEND_BASE_URL"
        value = var.frontend_base_url
      }

      env {
        name  = "Frontend__BaseUrl"
        value = var.frontend_base_url
      }

      # Stripe (non-secret: publishable key + price IDs)
      env {
        name  = "Stripe__PublishableKey"
        value = var.stripe_publishable_key != "" ? var.stripe_publishable_key : ""
      }

      env {
        name  = "Stripe__PriceIds__Free__Monthly"
        value = var.stripe_price_id_free_monthly != "" ? var.stripe_price_id_free_monthly : ""
      }

      env {
        name  = "Stripe__PriceIds__Free__Yearly"
        value = var.stripe_price_id_free_yearly != "" ? var.stripe_price_id_free_yearly : ""
      }

      env {
        name  = "Stripe__PriceIds__Pro__Monthly"
        value = var.stripe_price_id_pro_monthly != "" ? var.stripe_price_id_pro_monthly : ""
      }

      env {
        name  = "Stripe__PriceIds__Pro__Yearly"
        value = var.stripe_price_id_pro_yearly != "" ? var.stripe_price_id_pro_yearly : ""
      }

      env {
        name  = "Stripe__PriceIds__Enterprise__Monthly"
        value = var.stripe_price_id_enterprise_monthly != "" ? var.stripe_price_id_enterprise_monthly : ""
      }

      env {
        name  = "Stripe__PriceIds__Enterprise__Yearly"
        value = var.stripe_price_id_enterprise_yearly != "" ? var.stripe_price_id_enterprise_yearly : ""
      }

      # Security (optional, has defaults)
      env {
        name  = "Security__MaxFailedLoginAttempts"
        value = var.security_max_failed_login_attempts > 0 ? tostring(var.security_max_failed_login_attempts) : ""
      }

      env {
        name  = "Security__LockoutDurationMinutes"
        value = var.security_lockout_duration_minutes > 0 ? tostring(var.security_lockout_duration_minutes) : ""
      }

      # Python AI Service URL (non-secret, service key loaded from Key Vault)
      env {
        name  = "AI__PythonService__BaseUrl"
        value = "https://${azurerm_linux_function_app.ai_service.default_hostname}"
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-api"
    }
  )
}

# Role Assignment: Container Apps Identity -> ACR Pull
resource "azurerm_role_assignment" "container_apps_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.container_apps.principal_id
}

