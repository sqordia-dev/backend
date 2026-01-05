# Resource Group
# Note: This resource group already exists in Azure and is managed outside of Terraform creation
# Using lifecycle rules to prevent recreation attempts
resource "azurerm_resource_group" "main" {
  name     = "${var.project_name}-${var.environment}-rg"
  location = var.azure_location

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-rg"
    }
  )

  lifecycle {
    prevent_destroy = true
    ignore_changes  = all
  }
}

