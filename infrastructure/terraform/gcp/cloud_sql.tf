# Cloud SQL PostgreSQL Instance
resource "google_sql_database_instance" "postgresql" {
  name             = "${var.project_name}-${var.environment}-db"
  database_version = "POSTGRES_15"
  region           = var.gcp_region
  deletion_protection = false  # Set to true in production

  settings {
    tier                        = var.cloud_sql_tier
    availability_type           = "ZONAL" # SINGLE_ZONE for cost, REGIONAL for HA

    disk_size    = var.cloud_sql_disk_size
    disk_type    = "PD_SSD"  # PD_SSD for SSD, or remove for default
    disk_autoresize = true

    backup_configuration {
      enabled                        = true
      start_time                     = "03:00"
      point_in_time_recovery_enabled = true
      transaction_log_retention_days = 7
    }

    ip_configuration {
      ipv4_enabled = true
      # authorized_networks block can be added here if needed for external access
    }

    database_flags {
      name  = "max_connections"
      value = "100"
    }

    insights_config {
      query_insights_enabled  = true
      query_string_length     = 1024
      record_application_tags = true
      record_client_address   = true
    }
  }

  lifecycle {
    prevent_destroy = false # Set to true in production
  }
}

# Cloud SQL Database
resource "google_sql_database" "postgresql_db" {
  name     = var.cloud_sql_database_name
  instance = google_sql_database_instance.postgresql.name
}

# Cloud SQL User
resource "google_sql_user" "postgresql_user" {
  name     = var.cloud_sql_user
  instance = google_sql_database_instance.postgresql.name
  password  = var.cloud_sql_password
}

