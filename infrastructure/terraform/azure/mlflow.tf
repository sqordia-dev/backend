# =============================================================================
# MLflow Tracking Server — Production Deployment
#
# Runs as an Azure Container App alongside the .NET API.
# Uses the existing PostgreSQL server for backend store and Azure Blob for artifacts.
# Accessible internally by the Python AI Functions + optionally via public URL for UI.
# =============================================================================

# --- MLflow Database on existing PostgreSQL ---

resource "azurerm_postgresql_flexible_server_database" "mlflow" {
  name      = "mlflow"
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# --- Blob Storage container for MLflow artifacts ---

resource "azurerm_storage_container" "mlflow_artifacts" {
  name                  = "mlflow-artifacts"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# --- Key Vault secret for MLflow DB connection ---

resource "azurerm_key_vault_secret" "mlflow_db_connection" {
  name         = "mlflow-db-connection"
  value        = "postgresql://${var.postgresql_admin_username}:${var.postgresql_admin_password}@${azurerm_postgresql_flexible_server.main.fqdn}:5432/mlflow?sslmode=require"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
  tags       = var.common_tags
}

# --- MLflow Container App ---

resource "azurerm_container_app" "mlflow" {
  name                         = "${var.project_name}-${var.environment}-mlflow"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"

  template {
    min_replicas = var.mlflow_min_replicas
    max_replicas = 1 # MLflow is a single-instance tracking server

    container {
      name   = "mlflow"
      image  = "ghcr.io/mlflow/mlflow:v2.19.0"
      cpu    = var.mlflow_cpu
      memory = "${var.mlflow_memory}Gi"

      command = [
        "mlflow", "server",
        "--host", "0.0.0.0",
        "--port", "5050",
        "--backend-store-uri", "postgresql://${var.postgresql_admin_username}:${var.postgresql_admin_password}@${azurerm_postgresql_flexible_server.main.fqdn}:5432/mlflow?sslmode=require",
        "--default-artifact-root", "wasbs://mlflow-artifacts@${azurerm_storage_account.main.name}.blob.core.windows.net/",
        "--serve-artifacts",
      ]

      env {
        name  = "AZURE_STORAGE_CONNECTION_STRING"
        value = azurerm_storage_account.main.primary_connection_string
      }

      # Liveness probe
      liveness_probe {
        transport = "HTTP"
        port      = 5050
        path      = "/health"

        initial_delay           = 10
        interval_seconds        = 30
        timeout                 = 5
        failure_count_threshold = 3
      }

      # Readiness probe
      readiness_probe {
        transport = "HTTP"
        port      = 5050
        path      = "/health"

        interval_seconds        = 10
        timeout                 = 5
        failure_count_threshold = 3
      }
    }
  }

  ingress {
    external_enabled = var.mlflow_public_access
    target_port      = 5050
    transport        = "auto"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  tags = merge(
    var.common_tags,
    {
      Name    = "${var.project_name}-${var.environment}-mlflow"
      Service = "mlflow"
    }
  )
}
