# Artifact Registry Repository (for container images)
resource "google_artifact_registry_repository" "container_repo" {
  location      = var.gcp_region
  repository_id = "${var.project_name}-${var.environment}-repo"
  description   = "Container repository for ${var.project_name} ${var.environment}"
  format        = "DOCKER"
}

# Cloud Run Service Account (for Cloud Run to access other GCP services)
resource "google_service_account" "cloud_run_sa" {
  account_id   = "${var.project_name}-${var.environment}-run"
  display_name = "Cloud Run Service Account for ${var.project_name} ${var.environment}"
  description  = "Service account for Cloud Run service"
}

# Grant Cloud Run SA access to Cloud SQL
resource "google_project_iam_member" "cloud_run_sql_client" {
  project = var.gcp_project_id
  role    = "roles/cloudsql.client"
  member  = "serviceAccount:${google_service_account.cloud_run_sa.email}"
}

# Grant Cloud Run SA access to Secret Manager
resource "google_project_iam_member" "cloud_run_secret_accessor" {
  project = var.gcp_project_id
  role    = "roles/secretmanager.secretAccessor"
  member  = "serviceAccount:${google_service_account.cloud_run_sa.email}"
}

# Grant Cloud Run SA access to Cloud Storage
resource "google_project_iam_member" "cloud_run_storage_access" {
  project = var.gcp_project_id
  role    = "roles/storage.objectUser"
  member  = "serviceAccount:${google_service_account.cloud_run_sa.email}"
}

# Grant Cloud Run SA access to Pub/Sub
resource "google_project_iam_member" "cloud_run_pubsub_publisher" {
  project = var.gcp_project_id
  role    = "roles/pubsub.publisher"
  member  = "serviceAccount:${google_service_account.cloud_run_sa.email}"
}

# Cloud Run Service
resource "google_cloud_run_v2_service" "api_service" {
  name     = "${var.project_name}-${var.environment}-api"
  location = var.gcp_region

  template {
    service_account = google_service_account.cloud_run_sa.email

    scaling {
      min_instance_count = var.cloud_run_min_instances
      max_instance_count = var.cloud_run_max_instances
    }

    timeout = "${var.cloud_run_timeout}s"

    containers {
      image = "${var.gcp_region}-docker.pkg.dev/${var.gcp_project_id}/${google_artifact_registry_repository.container_repo.repository_id}/api:latest"

      resources {
        limits = {
          cpu    = var.cloud_run_cpu
          memory = var.cloud_run_memory
        }
        cpu_idle = true  # Enable CPU throttling to allow < 1 CPU
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment == "production" ? "Production" : "Development"
      }

      env {
        name  = "CloudProvider"
        value = "GCP"
      }

      env {
        name  = "GCP__ProjectId"
        value = var.gcp_project_id
      }

      env {
        name  = "ConnectionStrings__SqordiaDb"
        value = "Host=${google_sql_database_instance.postgresql.public_ip_address};Port=5432;Database=${var.cloud_sql_database_name};Username=${var.cloud_sql_user};Password=${var.cloud_sql_password};SSL Mode=Require;Trust Server Certificate=true"
      }

      env {
        name = "CloudStorage__BucketName"
        value = google_storage_bucket.documents.name
      }

      env {
        name  = "PubSub__EmailTopic"
        value = google_pubsub_topic.email_topic.name
      }

      env {
        name  = "PubSub__AIGenerationTopic"
        value = google_pubsub_topic.ai_generation_topic.name
      }

      env {
        name  = "PubSub__ExportTopic"
        value = google_pubsub_topic.export_topic.name
      }

      env {
        name  = "Email__FromAddress"
        value = var.ses_from_email
      }
    }
  }

  traffic {
    percent = 100
    type    = "TRAFFIC_TARGET_ALLOCATION_TYPE_LATEST"
  }
}

# Allow unauthenticated access to Cloud Run (or use IAM for authenticated access)
resource "google_cloud_run_v2_service_iam_member" "public_access" {
  name     = google_cloud_run_v2_service.api_service.name
  location = google_cloud_run_v2_service.api_service.location
  role     = "roles/run.invoker"
  member   = "allUsers" # Change to specific users/service accounts for production
}

