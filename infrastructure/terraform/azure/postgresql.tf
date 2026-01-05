# PostgreSQL Server
resource "azurerm_postgresql_flexible_server" "main" {
  name                   = "${var.project_name}-${var.environment}-postgres"
  resource_group_name    = azurerm_resource_group.main.name
  location               = var.azure_location
  version                = var.postgresql_version
  delegated_subnet_id    = null # Public access for now, can be changed to private endpoint
  private_dns_zone_id    = null
  public_network_access_enabled = true

  administrator_login    = var.postgresql_admin_username
  administrator_password = var.postgresql_admin_password

  sku_name   = var.postgresql_sku_name
  storage_mb = var.postgresql_storage_mb

  backup_retention_days        = 7
  geo_redundant_backup_enabled = false # Set to true for production HA

  maintenance_window {
    day_of_week  = 0 # Sunday
    start_hour   = 2
    start_minute = 0
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-postgres"
    }
  )

  lifecycle {
    prevent_destroy = false # Set to true in production
    ignore_changes = [
      zone # Zone is set automatically by Azure and cannot be changed after creation
    ]
  }
}

# PostgreSQL Database
resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.postgresql_database_name
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# PostgreSQL Firewall Rule - Allow Azure Services
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# PostgreSQL Firewall Rule - Allow All (for development, restrict in production)
# Remove this in production and add specific IP ranges
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_all" {
  name             = "AllowAll"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

