# Secret Manager Secret for Database Connection String
resource "google_secret_manager_secret" "database_connection" {
  secret_id = "${var.project_name}-${var.environment}-db-connection"

  replication {
    auto {}
  }

  labels = {
    environment = var.environment
    service     = "database"
  }
}

# Secret Version (contains the actual connection string)
resource "google_secret_manager_secret_version" "database_connection" {
  secret = google_secret_manager_secret.database_connection.id

  secret_data = "Host=${google_sql_database_instance.postgresql.private_ip_address};Port=5432;Database=${var.cloud_sql_database_name};Username=${var.cloud_sql_user};Password=${var.cloud_sql_password}"
}

