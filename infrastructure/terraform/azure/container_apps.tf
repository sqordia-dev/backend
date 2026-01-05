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
  retention_in_days    = max(var.log_analytics_retention_days, 30)  # Minimum 30 days for PerGB2018 SKU

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

