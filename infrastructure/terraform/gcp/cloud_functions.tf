# Cloud Functions Service Account
resource "google_service_account" "cloud_functions_sa" {
  account_id   = "${var.project_name}-${var.environment}-functions"
  display_name = "Cloud Functions Service Account for ${var.project_name} ${var.environment}"
  description  = "Service account for Cloud Functions"
}

# Grant Cloud Functions SA access to Cloud SQL
resource "google_project_iam_member" "cloud_functions_sql_client" {
  project = var.gcp_project_id
  role    = "roles/cloudsql.client"
  member  = "serviceAccount:${google_service_account.cloud_functions_sa.email}"
}

# Grant Cloud Functions SA access to Secret Manager
resource "google_project_iam_member" "cloud_functions_secret_accessor" {
  project = var.gcp_project_id
  role    = "roles/secretmanager.secretAccessor"
  member  = "serviceAccount:${google_service_account.cloud_functions_sa.email}"
}

# Grant Cloud Functions SA access to Cloud Storage
resource "google_project_iam_member" "cloud_functions_storage_access" {
  project = var.gcp_project_id
  role    = "roles/storage.objectAdmin"
  member  = "serviceAccount:${google_service_account.cloud_functions_sa.email}"
}

# Grant Cloud Functions SA access to Pub/Sub
resource "google_project_iam_member" "cloud_functions_pubsub_subscriber" {
  project = var.gcp_project_id
  role    = "roles/pubsub.subscriber"
  member  = "serviceAccount:${google_service_account.cloud_functions_sa.email}"
}

# Email Handler Cloud Function
resource "google_cloudfunctions2_function" "email_handler" {
  name        = "${var.project_name}-${var.environment}-email-handler"
  location    = var.gcp_region
  description = "Email handler function for ${var.project_name}"

  build_config {
    runtime     = "dotnet8"
    entry_point = "Sqordia.Functions.EmailHandler.Function"
    source {
      storage_source {
        bucket = google_storage_bucket.functions_source.name
        object = "email-handler.zip"
      }
    }
  }

  service_config {
    max_instance_count    = 10
    min_instance_count    = 0
    available_memory      = "${var.cloud_functions_memory}M"
    timeout_seconds       = var.cloud_functions_timeout
    service_account_email = google_service_account.cloud_functions_sa.email

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = var.environment == "production" ? "Production" : "Development"
      ConnectionStrings__SqordiaDb = "Host=${google_sql_database_instance.postgresql.public_ip_address};Port=5432;Database=${var.cloud_sql_database_name};Username=${var.cloud_sql_user};Password=${var.cloud_sql_password};SSL Mode=Require;Trust Server Certificate=true"
      Email__FromAddress = var.ses_from_email
    }
  }

  event_trigger {
    trigger_region = var.gcp_region
    event_type     = "google.cloud.pubsub.topic.v1.messagePublished"
    pubsub_topic   = google_pubsub_topic.email_topic.id
    retry_policy   = "RETRY_POLICY_RETRY"
  }
}

# AI Generation Handler Cloud Function
resource "google_cloudfunctions2_function" "ai_generation_handler" {
  name        = "${var.project_name}-${var.environment}-ai-generation-handler"
  location    = var.gcp_region
  description = "AI generation handler function for ${var.project_name}"

  build_config {
    runtime     = "dotnet8"
    entry_point = "Sqordia.Functions.AIGenerationHandler.Function"
    source {
      storage_source {
        bucket = google_storage_bucket.functions_source.name
        object = "ai-generation-handler.zip"
      }
    }
  }

  service_config {
    max_instance_count    = 10
    min_instance_count    = 0
    available_memory      = "${var.cloud_functions_memory}M"
    timeout_seconds       = var.cloud_functions_timeout
    service_account_email = google_service_account.cloud_functions_sa.email

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = var.environment == "production" ? "Production" : "Development"
      ConnectionStrings__SqordiaDb = "Host=${google_sql_database_instance.postgresql.public_ip_address};Port=5432;Database=${var.cloud_sql_database_name};Username=${var.cloud_sql_user};Password=${var.cloud_sql_password};SSL Mode=Require;Trust Server Certificate=true"
    }
  }

  event_trigger {
    trigger_region = var.gcp_region
    event_type     = "google.cloud.pubsub.topic.v1.messagePublished"
    pubsub_topic   = google_pubsub_topic.ai_generation_topic.id
    retry_policy   = "RETRY_POLICY_RETRY"
  }
}

# Export Handler Cloud Function
resource "google_cloudfunctions2_function" "export_handler" {
  name        = "${var.project_name}-${var.environment}-export-handler"
  location    = var.gcp_region
  description = "Export handler function for ${var.project_name}"

  build_config {
    runtime     = "dotnet8"
    entry_point = "Sqordia.Functions.ExportHandler.Function"
    source {
      storage_source {
        bucket = google_storage_bucket.functions_source.name
        object = "export-handler.zip"
      }
    }
  }

  service_config {
    max_instance_count    = 10
    min_instance_count    = 0
    available_memory      = "${var.cloud_functions_memory}M"
    timeout_seconds       = var.cloud_functions_timeout
    service_account_email = google_service_account.cloud_functions_sa.email

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = var.environment == "production" ? "Production" : "Development"
      ConnectionStrings__SqordiaDb = "Host=${google_sql_database_instance.postgresql.private_ip_address};Port=5432;Database=${var.cloud_sql_database_name};Username=${var.cloud_sql_user};Password=${var.cloud_sql_password}"
      CloudStorage__BucketName = google_storage_bucket.documents.name
    }

    vpc_connector                 = null # Add VPC connector if needed for private Cloud SQL access
    vpc_connector_egress_settings = "PRIVATE_RANGES_ONLY"
  }

  event_trigger {
    trigger_region = var.gcp_region
    event_type     = "google.cloud.pubsub.topic.v1.messagePublished"
    pubsub_topic   = google_pubsub_topic.export_topic.id
    retry_policy   = "RETRY_POLICY_RETRY"
  }
}

# Cloud Storage Bucket for Function Source Code
resource "google_storage_bucket" "functions_source" {
  name          = "${var.project_name}-${var.environment}-functions-source"
  location      = var.cloud_storage_location
  storage_class = "STANDARD"
  force_destroy = true # Can be destroyed for function updates

  uniform_bucket_level_access = true
}

