# Container Registry
resource "azurerm_container_registry" "main" {
  name                = "${var.project_name}${var.environment}acr" # Must be globally unique, lowercase, alphanumeric
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  sku                 = var.container_registry_sku
  admin_enabled       = false # Use Managed Identity instead

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-acr"
    }
  )
}

