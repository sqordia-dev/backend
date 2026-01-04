# Cloud Storage Bucket for Document Exports
resource "google_storage_bucket" "documents" {
  name          = "${var.project_name}-${var.environment}-documents"
  location      = var.cloud_storage_location
  storage_class = var.cloud_storage_class
  force_destroy = false # Set to true for testing, false for production

  uniform_bucket_level_access = true

  versioning {
    enabled = true
  }

  lifecycle_rule {
    condition {
      age = 90 # Delete objects older than 90 days
    }
    action {
      type = "Delete"
    }
  }

  cors {
    origin          = ["*"] # Configure with specific origins in production
    method          = ["GET", "HEAD", "PUT", "POST", "DELETE"]
    response_header = ["*"]
    max_age_seconds = 3600
  }
}

# IAM binding for Cloud Run service account
resource "google_storage_bucket_iam_member" "cloud_run_storage_access" {
  bucket = google_storage_bucket.documents.name
  role   = "roles/storage.objectAdmin"
  member = "serviceAccount:${google_service_account.cloud_run_sa.email}"
}

