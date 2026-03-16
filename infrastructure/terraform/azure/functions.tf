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

# User Assigned Identity for .NET Functions (Key Vault access)
resource "azurerm_user_assigned_identity" "dotnet_functions" {
  name                = "${var.project_name}-${var.environment}-dotnet-functions-identity"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name

  tags = var.common_tags
}

resource "azurerm_key_vault_access_policy" "dotnet_functions" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.dotnet_functions.principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# AI Generation Handler Function App
resource "azurerm_linux_function_app" "ai_generation_handler" {
  name                 = "${var.project_name}-${var.environment}-ai-generation-handler"
  resource_group_name  = data.azurerm_resource_group.main.name
  location             = data.azurerm_resource_group.main.location
  service_plan_id      = azurerm_service_plan.functions.id
  storage_account_name = azurerm_storage_account.functions.name

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.dotnet_functions.id]
  }

  key_vault_reference_identity_id = azurerm_user_assigned_identity.dotnet_functions.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"

    # Secrets from Key Vault
    ConnectionStrings__SqordiaDb      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.database_connection.versionless_id})"
    AzureServiceBus__ConnectionString = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.servicebus_connection_string.versionless_id})"
    OPENAI_API_KEY                    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.openai_api_key.versionless_id})"
    OpenAI__ApiKey                    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.openai_api_key.versionless_id})"
    AI__OpenAI__ApiKey                = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.openai_api_key.versionless_id})"

    # Non-secret configuration
    AzureServiceBus__AiGenerationTopic        = azurerm_servicebus_topic.ai_generation.name
    AzureServiceBus__AiGenerationSubscription = azurerm_servicebus_subscription.ai_generation.name
    AzureKeyVault__VaultUrl                   = azurerm_key_vault.main.vault_uri
    OPENAI_MODEL                              = var.openai_model
    OpenAI__Model                             = var.openai_model
    AI__OpenAI__Model                         = var.openai_model
    WEBSITE_CONTENTSHARE                      = "${var.project_name}-${var.environment}-ai-generation-handler-content"
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING   = azurerm_storage_account.functions.primary_connection_string
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-ai-generation-handler"
    }
  )

  depends_on = [
    azurerm_key_vault_access_policy.dotnet_functions,
  ]
}

# Export Handler Function App
resource "azurerm_linux_function_app" "export_handler" {
  name                 = "${var.project_name}-${var.environment}-export-handler"
  resource_group_name  = data.azurerm_resource_group.main.name
  location             = data.azurerm_resource_group.main.location
  service_plan_id      = azurerm_service_plan.functions.id
  storage_account_name = azurerm_storage_account.functions.name

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.dotnet_functions.id]
  }

  key_vault_reference_identity_id = azurerm_user_assigned_identity.dotnet_functions.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"

    # Secrets from Key Vault
    ConnectionStrings__SqordiaDb      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.database_connection.versionless_id})"
    AzureServiceBus__ConnectionString = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.servicebus_connection_string.versionless_id})"
    AzureStorage__ConnectionString    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.storage_connection_string.versionless_id})"

    # Non-secret configuration
    AzureServiceBus__ExportTopic        = azurerm_servicebus_topic.export.name
    AzureServiceBus__ExportSubscription = azurerm_servicebus_subscription.export.name
    AzureStorage__AccountName           = azurerm_storage_account.main.name
    AzureStorage__ContainerName         = azurerm_storage_container.documents.name
    WEBSITE_CONTENTSHARE                = "${var.project_name}-${var.environment}-export-handler-content"
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING = azurerm_storage_account.functions.primary_connection_string
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-export-handler"
    }
  )

  depends_on = [
    azurerm_key_vault_access_policy.dotnet_functions,
  ]
}

