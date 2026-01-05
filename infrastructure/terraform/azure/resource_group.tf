# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "${var.project_name}-${var.environment}-rg"
  location = var.azure_location

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-rg"
    }
  )
}

