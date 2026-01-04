# Cloud Run Outputs
output "cloud_run_service_url" {
  description = "Cloud Run service URL"
  value       = google_cloud_run_v2_service.api_service.uri
}

output "cloud_run_service_name" {
  description = "Cloud Run service name"
  value       = google_cloud_run_v2_service.api_service.name
}

# Cloud SQL Outputs
output "cloud_sql_connection_name" {
  description = "Cloud SQL connection name (for Cloud SQL Proxy)"
  value       = google_sql_database_instance.postgresql.connection_name
}

output "cloud_sql_public_ip" {
  description = "Cloud SQL public IP address"
  value       = google_sql_database_instance.postgresql.public_ip_address
}

output "cloud_sql_private_ip" {
  description = "Cloud SQL private IP address"
  value       = google_sql_database_instance.postgresql.private_ip_address
}

output "cloud_sql_database_name" {
  description = "Cloud SQL database name"
  value       = google_sql_database.postgresql_db.name
}

output "cloud_sql_user" {
  description = "Cloud SQL master username"
  value       = var.cloud_sql_user
}

# Cloud Storage Outputs
output "cloud_storage_bucket_name" {
  description = "Cloud Storage bucket name"
  value       = google_storage_bucket.documents.name
}

output "cloud_storage_bucket_url" {
  description = "Cloud Storage bucket URL"
  value       = google_storage_bucket.documents.url
}

# Cloud Pub/Sub Outputs
output "email_topic_name" {
  description = "Email Pub/Sub topic name"
  value       = google_pubsub_topic.email_topic.name
}

output "ai_generation_topic_name" {
  description = "AI generation Pub/Sub topic name"
  value       = google_pubsub_topic.ai_generation_topic.name
}

output "export_topic_name" {
  description = "Export Pub/Sub topic name"
  value       = google_pubsub_topic.export_topic.name
}

# Cloud Functions Outputs
output "email_function_name" {
  description = "Email handler Cloud Function name"
  value       = google_cloudfunctions2_function.email_handler.name
}

output "ai_generation_function_name" {
  description = "AI generation handler Cloud Function name"
  value       = google_cloudfunctions2_function.ai_generation_handler.name
}

output "export_function_name" {
  description = "Export handler Cloud Function name"
  value       = google_cloudfunctions2_function.export_handler.name
}

# Secret Manager Outputs
output "database_secret_name" {
  description = "Database connection string secret name"
  value       = google_secret_manager_secret.database_connection.secret_id
}

# Artifact Registry Outputs
output "artifact_registry_repository_name" {
  description = "Artifact Registry repository name"
  value       = google_artifact_registry_repository.container_repo.name
}

output "artifact_registry_repository_url" {
  description = "Artifact Registry repository URL"
  value       = google_artifact_registry_repository.container_repo.name
}

