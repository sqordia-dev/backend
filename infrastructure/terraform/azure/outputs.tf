# Resource Group Outputs
output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_resource_group.main.name
}

output "resource_group_location" {
  description = "Resource group location"
  value       = azurerm_resource_group.main.location
}

# Container Apps Outputs
output "container_app_url" {
  description = "Container App URL"
  value       = azurerm_container_app.api.latest_revision_fqdn
}

output "container_app_name" {
  description = "Container App name"
  value       = azurerm_container_app.api.name
}

# PostgreSQL Outputs
output "postgresql_fqdn" {
  description = "PostgreSQL fully qualified domain name"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "postgresql_server_name" {
  description = "PostgreSQL server name"
  value       = azurerm_postgresql_flexible_server.main.name
}

output "postgresql_database_name" {
  description = "PostgreSQL database name"
  value       = azurerm_postgresql_flexible_server_database.main.name
}

output "postgresql_admin_username" {
  description = "PostgreSQL admin username"
  value       = var.postgresql_admin_username
  sensitive   = true
}

# Blob Storage Outputs
output "storage_account_name" {
  description = "Storage account name"
  value       = azurerm_storage_account.main.name
}

output "storage_account_primary_endpoint" {
  description = "Storage account primary endpoint"
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

output "blob_container_name" {
  description = "Blob container name"
  value       = azurerm_storage_container.documents.name
}

# Service Bus Outputs
output "service_bus_namespace" {
  description = "Service Bus namespace"
  value       = azurerm_servicebus_namespace.main.name
}

output "service_bus_email_topic" {
  description = "Email Service Bus topic name"
  value       = azurerm_servicebus_topic.email.name
}

output "service_bus_ai_generation_topic" {
  description = "AI Generation Service Bus topic name"
  value       = azurerm_servicebus_topic.ai_generation.name
}

output "service_bus_export_topic" {
  description = "Export Service Bus topic name"
  value       = azurerm_servicebus_topic.export.name
}

# Azure Functions Outputs
output "email_function_app_name" {
  description = "Email handler Function App name"
  value       = azurerm_linux_function_app.email_handler.name
}

output "ai_generation_function_app_name" {
  description = "AI Generation handler Function App name"
  value       = azurerm_linux_function_app.ai_generation_handler.name
}

output "export_function_app_name" {
  description = "Export handler Function App name"
  value       = azurerm_linux_function_app.export_handler.name
}

# Container Registry Outputs
output "container_registry_login_server" {
  description = "Container Registry login server"
  value       = azurerm_container_registry.main.login_server
}

output "container_registry_name" {
  description = "Container Registry name"
  value       = azurerm_container_registry.main.name
}

# Key Vault Outputs
output "key_vault_name" {
  description = "Key Vault name"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = azurerm_key_vault.main.vault_uri
}

output "database_secret_name" {
  description = "Database connection string secret name"
  value       = azurerm_key_vault_secret.database_connection.name
}

# Log Analytics Outputs
output "log_analytics_workspace_id" {
  description = "Log Analytics workspace ID"
  value       = azurerm_log_analytics_workspace.main.workspace_id
}

output "log_analytics_workspace_name" {
  description = "Log Analytics workspace name"
  value       = azurerm_log_analytics_workspace.main.name
}

