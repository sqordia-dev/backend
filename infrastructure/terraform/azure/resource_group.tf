# Reference existing resource group (managed outside Terraform)
data "azurerm_resource_group" "main" {
  name = "${var.project_name}-${var.environment}-rg"
}

