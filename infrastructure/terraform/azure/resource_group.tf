# Import existing resource group into Terraform state
import {
  to = azurerm_resource_group.main
  id = "/subscriptions/83deee7a-77ea-4add-8c10-c93199c5ec54/resourceGroups/sqordia-production-rg"
}

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

