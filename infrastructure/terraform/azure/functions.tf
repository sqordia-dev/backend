# Storage Account for Functions (required)
resource "azurerm_storage_account" "functions" {
  name                     = "${var.project_name}${var.environment}func" # Must be globally unique
  resource_group_name      = data.azurerm_resource_group.main.name
  location                 = data.azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-functions-storage"
    }
  )
}

# App Service Plan for Functions (Consumption plan)
resource "azurerm_service_plan" "functions" {
  name                = "${var.project_name}-${var.environment}-functions-plan"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "Y1" # Consumption plan

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-functions-plan"
    }
  )
}

# Email Handler Function App
resource "azurerm_linux_function_app" "email_handler" {
  name                 = "${var.project_name}-${var.environment}-email-handler"
  resource_group_name  = data.azurerm_resource_group.main.name
  location             = data.azurerm_resource_group.main.location
  service_plan_id      = azurerm_service_plan.functions.id
  storage_account_name = azurerm_storage_account.functions.name

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME                        = "dotnet-isolated"
    ConnectionStrings__SqordiaDb                    = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;"
    AzureServiceBus__ConnectionString               = azurerm_servicebus_namespace.main.default_primary_connection_string
    AzureServiceBus__EmailTopic                     = azurerm_servicebus_topic.email.name
    AzureServiceBus__EmailSubscription              = azurerm_servicebus_subscription.email.name
    AzureCommunicationServices__ConnectionString    = azurerm_communication_service.main.primary_connection_string
    AzureCommunicationServices__FromEmail           = var.email_from_address != "" ? var.email_from_address : azurerm_email_communication_service_domain.azure_managed.from_sender_domain
    AzureCommunicationServices__FromName            = var.email_from_name
    WEBSITE_CONTENTSHARE                            = "${var.project_name}-${var.environment}-email-handler-content"
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING        = azurerm_storage_account.functions.primary_connection_string
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-email-handler"
    }
  )
}

# AI Generation Handler Function App
resource "azurerm_linux_function_app" "ai_generation_handler" {
  name                 = "${var.project_name}-${var.environment}-ai-generation-handler"
  resource_group_name  = data.azurerm_resource_group.main.name
  location             = data.azurerm_resource_group.main.location
  service_plan_id      = azurerm_service_plan.functions.id
  storage_account_name = azurerm_storage_account.functions.name

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME                  = "dotnet-isolated"
    ConnectionStrings__SqordiaDb              = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;"
    AzureServiceBus__ConnectionString         = azurerm_servicebus_namespace.main.default_primary_connection_string
    AzureServiceBus__AiGenerationTopic        = azurerm_servicebus_topic.ai_generation.name
    AzureServiceBus__AiGenerationSubscription = azurerm_servicebus_subscription.ai_generation.name
    AzureKeyVault__VaultUrl                   = azurerm_key_vault.main.vault_uri
    OPENAI_API_KEY                            = var.openai_api_key
    OpenAI__ApiKey                            = var.openai_api_key
    AI__OpenAI__ApiKey                        = var.openai_api_key
    OPENAI_MODEL                              = var.openai_model
    OpenAI__Model                             = var.openai_model
    AI__OpenAI__Model                         = var.openai_model
    WEBSITE_CONTENTSHARE                      = "${var.project_name}-${var.environment}-ai-generation-handler-content"
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING  = azurerm_storage_account.functions.primary_connection_string
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-ai-generation-handler"
    }
  )
}

# Export Handler Function App
resource "azurerm_linux_function_app" "export_handler" {
  name                 = "${var.project_name}-${var.environment}-export-handler"
  resource_group_name  = data.azurerm_resource_group.main.name
  location             = data.azurerm_resource_group.main.location
  service_plan_id      = azurerm_service_plan.functions.id
  storage_account_name = azurerm_storage_account.functions.name

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME                 = "dotnet-isolated"
    ConnectionStrings__SqordiaDb             = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;"
    AzureServiceBus__ConnectionString        = azurerm_servicebus_namespace.main.default_primary_connection_string
    AzureServiceBus__ExportTopic             = azurerm_servicebus_topic.export.name
    AzureServiceBus__ExportSubscription      = azurerm_servicebus_subscription.export.name
    AzureStorage__AccountName                = azurerm_storage_account.main.name
    AzureStorage__ConnectionString           = azurerm_storage_account.main.primary_connection_string
    AzureStorage__ContainerName              = azurerm_storage_container.documents.name
    WEBSITE_CONTENTSHARE                     = "${var.project_name}-${var.environment}-export-handler-content"
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING = azurerm_storage_account.functions.primary_connection_string
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-export-handler"
    }
  )
}

