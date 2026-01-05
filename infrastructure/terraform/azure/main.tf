terraform {
  required_version = ">= 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }

  # Optional: Use Azure Storage backend for remote state (recommended for team collaboration)
  # Uncomment and configure when ready
  # backend "azurerm" {
  #   resource_group_name  = "sqordia-terraform-state"
  #   storage_account_name = "sqordiaterraformstate"
  #   container_name       = "terraform-state"
  #   key                  = "terraform.tfstate"
  # }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }

  # Subscription ID, Client ID, Client Secret, and Tenant ID can be set via environment variables:
  # ARM_SUBSCRIPTION_ID, ARM_CLIENT_ID, ARM_CLIENT_SECRET, ARM_TENANT_ID
  # Or via Azure CLI: az login
}

