terraform {
  required_version = ">= 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }

  # Remote state in Azure Storage (encrypted at rest, versioned for rollback)
  backend "azurerm" {
    resource_group_name  = "sqordia-production-rg"
    storage_account_name = "sqordiaterraformstate"
    container_name       = "tfstate"
    key                  = "production.tfstate"
  }
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

