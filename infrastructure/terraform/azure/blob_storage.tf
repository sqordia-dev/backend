# Storage Account
resource "azurerm_storage_account" "main" {
  name                     = "${var.project_name}${var.environment}storage" # Storage account names must be globally unique and lowercase
  resource_group_name      = data.azurerm_resource_group.main.name
  location                 = data.azurerm_resource_group.main.location
  account_tier             = var.blob_storage_account_tier
  account_replication_type = var.blob_storage_account_replication_type
  account_kind             = "StorageV2"

  # Enable blob versioning
  blob_properties {
    versioning_enabled = true
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-storage"
    }
  )
}

# Blob Container for Documents
resource "azurerm_storage_container" "documents" {
  name                  = "documents"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# Storage Account Access Key (for application access)
# Note: In production, use Managed Identity instead of access keys
data "azurerm_storage_account" "main" {
  name                = azurerm_storage_account.main.name
  resource_group_name = data.azurerm_resource_group.main.name
}

