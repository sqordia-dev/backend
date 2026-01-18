# Azure Communication Services for Email
# This service enables sending transactional emails from the application

# Communication Services Account
resource "azurerm_communication_service" "main" {
  name                = "${var.project_name}-${var.environment}-email-communication"
  resource_group_name = data.azurerm_resource_group.main.name
  data_location       = "United States" # Email is only available in US data location

  tags = merge(
    var.common_tags,
    {
      Name = "Communication Services"
    }
  )
}

# Email Services - Domain Configuration
resource "azurerm_email_communication_service" "main" {
  name                = "${var.project_name}-${var.environment}-email"
  resource_group_name = data.azurerm_resource_group.main.name
  data_location       = "United States" # Email is only available in US data location

  tags = merge(
    var.common_tags,
    {
      Name = "Email Communication Service"
    }
  )
}

# Azure Managed Domain for Email (free tier - for development/testing)
# This provides a subdomain like "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net"
resource "azurerm_email_communication_service_domain" "azure_managed" {
  name             = "AzureManagedDomain"
  email_service_id = azurerm_email_communication_service.main.id

  domain_management = "AzureManaged"

  tags = merge(
    var.common_tags,
    {
      Name = "Azure Managed Email Domain"
    }
  )
}

# Custom Domain for Email (sqordia.app)
# Requires DNS verification - see outputs for DNS records to add
resource "azurerm_email_communication_service_domain" "custom" {
  count            = var.email_from_address != "" && !strcontains(var.email_from_address, "azurecomm.net") ? 1 : 0
  name             = split("@", var.email_from_address)[1] # Extract domain from email address
  email_service_id = azurerm_email_communication_service.main.id

  domain_management = "CustomerManaged"

  tags = merge(
    var.common_tags,
    {
      Name = "Custom Email Domain"
    }
  )
}

# Link Email Service to Communication Service (Azure Managed Domain)
resource "azurerm_communication_service_email_domain_association" "azure_managed" {
  communication_service_id = azurerm_communication_service.main.id
  email_service_domain_id  = azurerm_email_communication_service_domain.azure_managed.id
}

# Link Email Service to Communication Service (Custom Domain)
# NOTE: This requires the custom domain to be verified via DNS first.
# Steps:
# 1. Apply Terraform to create the domain resource (without this association)
# 2. Get DNS verification records: terraform output email_domain_verification_records
# 3. Add the TXT records to your DNS provider
# 4. Wait for verification (can take up to 24 hours)
# 5. Uncomment this resource and apply again
#
# For now, comment this out and use the Azure managed domain, or verify the domain first.
# resource "azurerm_communication_service_email_domain_association" "custom" {
#   count                    = var.email_from_address != "" && !strcontains(var.email_from_address, "azurecomm.net") ? 1 : 0
#   communication_service_id = azurerm_communication_service.main.id
#   email_service_domain_id  = azurerm_email_communication_service_domain.custom[0].id
#   
#   # This will fail if domain is not verified - uncomment after DNS verification
#   depends_on = [azurerm_email_communication_service_domain.custom]
# }

# Store connection string in Key Vault
resource "azurerm_key_vault_secret" "communication_services_connection_string" {
  name         = "AzureCommunicationServices--ConnectionString"
  value        = azurerm_communication_service.main.primary_connection_string
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [
    azurerm_key_vault.main,
    azurerm_key_vault_access_policy.current_user
  ]

  tags = merge(
    var.common_tags,
    {
      Name = "Communication Services Connection String"
    }
  )
}

# Store email from address in Key Vault
resource "azurerm_key_vault_secret" "email_from_address" {
  name         = "Email--FromAddress"
  value        = var.email_from_address != "" ? var.email_from_address : "DoNotReply@${azurerm_email_communication_service_domain.azure_managed.from_sender_domain}"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [
    azurerm_key_vault.main,
    azurerm_key_vault_access_policy.current_user,
    azurerm_email_communication_service_domain.azure_managed
  ]

  tags = merge(
    var.common_tags,
    {
      Name = "Email From Address"
    }
  )
}

# Store email from name in Key Vault
resource "azurerm_key_vault_secret" "email_from_name" {
  name         = "Email--FromName"
  value        = var.email_from_name
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [
    azurerm_key_vault.main,
    azurerm_key_vault_access_policy.current_user
  ]

  tags = merge(
    var.common_tags,
    {
      Name = "Email From Name"
    }
  )
}
