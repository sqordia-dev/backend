variable "azure_subscription_id" {
  description = "Azure Subscription ID"
  type        = string
}

variable "azure_location" {
  description = "Azure region for resources"
  type        = string
  default     = "canadacentral" # Canada Central for CAD pricing
}

variable "environment" {
  description = "Environment name (dev, staging, production)"
  type        = string
  default     = "production"
}

variable "project_name" {
  description = "Project name for resource naming"
  type        = string
  default     = "sqordia"
}

# PostgreSQL Configuration
variable "postgresql_sku_name" {
  description = "PostgreSQL SKU name (B_Standard_B1ms for Basic tier, GP_Standard_D2s_v3 for General Purpose)"
  type        = string
  default     = "B_Standard_B1ms" # Basic tier, Standard, B1ms (1 vCore, 2GB RAM)
}

variable "postgresql_storage_mb" {
  description = "PostgreSQL storage in MB (minimum 32768 = 32GB)"
  type        = number
  default     = 32768 # 32GB (minimum for Flexible Server)
}

variable "postgresql_database_name" {
  description = "PostgreSQL database name"
  type        = string
  default     = "SqordiaDb"
}

variable "postgresql_admin_username" {
  description = "PostgreSQL admin username"
  type        = string
  default     = "sqordia_admin"
}

variable "postgresql_admin_password" {
  description = "PostgreSQL admin password (set via terraform.tfvars or TF_VAR_postgresql_admin_password)"
  type        = string
  sensitive   = true
  # No default - must be provided
}

variable "postgresql_version" {
  description = "PostgreSQL version"
  type        = string
  default     = "15"
}

# Container Apps Configuration
variable "container_apps_cpu" {
  description = "Container Apps CPU allocation (0.25, 0.5, 1.0, 2.0, etc.)"
  type        = number
  default     = 0.5
}

variable "container_apps_memory" {
  description = "Container Apps memory in GB (0.5, 1.0, 2.0, etc.)"
  type        = string
  default     = "1.0"
}

variable "container_apps_min_replicas" {
  description = "Minimum number of Container Apps replicas (0 for cost optimization, 1 to avoid cold starts)"
  type        = number
  default     = 0
}

variable "container_apps_max_replicas" {
  description = "Maximum number of Container Apps replicas"
  type        = number
  default     = 10
}

# Azure Functions Configuration
variable "functions_timeout" {
  description = "Azure Functions timeout in seconds"
  type        = number
  default     = 300 # 5 minutes
}

variable "functions_memory_mb" {
  description = "Azure Functions memory in MB"
  type        = number
  default     = 512
}

# Blob Storage Configuration
variable "blob_storage_account_tier" {
  description = "Blob Storage account tier (Standard, Premium)"
  type        = string
  default     = "Standard"
}

variable "blob_storage_account_replication_type" {
  description = "Blob Storage replication type (LRS, GRS, RAGRS, ZRS)"
  type        = string
  default     = "LRS" # Locally Redundant Storage for cost optimization
}

variable "blob_storage_access_tier" {
  description = "Blob Storage access tier (Hot, Cool, Archive)"
  type        = string
  default     = "Hot"
}

# Service Bus Configuration
variable "service_bus_sku" {
  description = "Service Bus SKU (Basic, Standard, Premium)"
  type        = string
  default     = "Basic" # Free tier eligible
}

# Container Registry Configuration
variable "container_registry_sku" {
  description = "Container Registry SKU (Basic, Standard, Premium)"
  type        = string
  default     = "Basic"
}

# Key Vault Configuration
variable "key_vault_sku" {
  description = "Key Vault SKU (standard, premium)"
  type        = string
  default     = "standard"
}

# Log Analytics Configuration
variable "log_analytics_retention_days" {
  description = "Log Analytics retention in days (30, 60, 90, 120, 180, 240, 365)"
  type        = number
  default     = 7
}

# Google OAuth Configuration
variable "google_oauth_client_id" {
  description = "Google OAuth Client ID"
  type        = string
  sensitive   = false
  # No default - must be provided
}

variable "google_oauth_client_secret" {
  description = "Google OAuth Client Secret"
  type        = string
  sensitive   = true
  # No default - must be provided
}

variable "google_oauth_redirect_uri" {
  description = "Google OAuth Redirect URI"
  type        = string
  default     = ""
}

# JWT Configuration
variable "jwt_secret" {
  description = "JWT Secret for token signing"
  type        = string
  sensitive   = true
  # No default - must be provided
}

variable "jwt_issuer" {
  description = "JWT Issuer"
  type        = string
  default     = "Sqordia"
}

variable "jwt_audience" {
  description = "JWT Audience"
  type        = string
  default     = "SqordiaUsers"
}

variable "jwt_expiration_minutes" {
  description = "JWT expiration time in minutes"
  type        = number
  default     = 60
}

# Tags
variable "common_tags" {
  description = "Common tags to apply to all resources"
  type        = map(string)
  default = {
    Project     = "Sqordia"
    ManagedBy   = "Terraform"
    Environment = "production"
  }
}

