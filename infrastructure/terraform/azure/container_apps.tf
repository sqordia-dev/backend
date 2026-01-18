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

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;"
      }

      env {
        name  = "AzureStorage__AccountName"
        value = azurerm_storage_account.main.name
      }

      env {
        name  = "AzureStorage__ConnectionString"
        value = azurerm_storage_account.main.primary_connection_string
      }

      env {
        name  = "AzureStorage__ContainerName"
        value = azurerm_storage_container.documents.name
      }

      env {
        name  = "AzureServiceBus__ConnectionString"
        value = azurerm_servicebus_namespace.main.default_primary_connection_string
      }

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

      env {
        name  = "AzureKeyVault__VaultUrl"
        value = azurerm_key_vault.main.vault_uri
      }

      env {
        name  = "GOOGLE_OAUTH_CLIENT_ID"
        value = var.google_oauth_client_id
      }

      env {
        name  = "GOOGLE_OAUTH_CLIENT_SECRET"
        value = var.google_oauth_client_secret
      }

      env {
        name  = "GoogleOAuth__ClientId"
        value = var.google_oauth_client_id
      }

      env {
        name  = "GoogleOAuth__ClientSecret"
        value = var.google_oauth_client_secret
      }

      env {
        name  = "GoogleOAuth__RedirectUri"
        value = var.google_oauth_redirect_uri != "" ? var.google_oauth_redirect_uri : "https://${var.project_name}.app/api/v1/auth/google/callback"
      }

      env {
        name  = "JWT_SECRET"
        value = var.jwt_secret
      }

      env {
        name  = "JwtSettings__Secret"
        value = var.jwt_secret
      }

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

      # OpenAI Configuration
      env {
        name  = "OPENAI_API_KEY"
        value = var.openai_api_key
      }

      env {
        name  = "OpenAI__ApiKey"
        value = var.openai_api_key
      }

      env {
        name  = "AI__OpenAI__ApiKey"
        value = var.openai_api_key
      }

      env {
        name  = "OPENAI_MODEL"
        value = var.openai_model
      }

      env {
        name  = "OpenAI__Model"
        value = var.openai_model
      }

      env {
        name  = "AI__OpenAI__Model"
        value = var.openai_model
      }

      # Frontend Base URL
      env {
        name  = "FRONTEND_BASE_URL"
        value = var.frontend_base_url
      }

      env {
        name  = "Frontend__BaseUrl"
        value = var.frontend_base_url
      }

      # Azure Communication Services Email
      env {
        name  = "AzureCommunicationServices__ConnectionString"
        value = azurerm_communication_service.main.primary_connection_string
      }

      env {
        name  = "AzureCommunicationServices__FromEmail"
        value = var.email_from_address != "" ? var.email_from_address : "DoNotReply@${try(azurerm_email_communication_service_domain.azure_managed.from_sender_domain, "azurecomm.net")}"
      }

      env {
        name  = "AzureCommunicationServices__FromName"
        value = var.email_from_name
      }

      # Stripe Configuration (optional - for subscription management)
      env {
        name  = "Stripe__SecretKey"
        value = var.stripe_secret_key != "" ? var.stripe_secret_key : ""
      }

      env {
        name  = "Stripe__WebhookSecret"
        value = var.stripe_webhook_secret != "" ? var.stripe_webhook_secret : ""
      }

      env {
        name  = "Stripe__PublishableKey"
        value = var.stripe_publishable_key != "" ? var.stripe_publishable_key : ""
      }

      # Stripe Price IDs
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

      # Security Configuration (optional - has defaults)
      env {
        name  = "Security__MaxFailedLoginAttempts"
        value = var.security_max_failed_login_attempts > 0 ? tostring(var.security_max_failed_login_attempts) : ""
      }

      env {
        name  = "Security__LockoutDurationMinutes"
        value = var.security_lockout_duration_minutes > 0 ? tostring(var.security_lockout_duration_minutes) : ""
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

