terraform {
  required_version = ">= 1.0"

  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 5.0"
    }
  }

  # Optional: Use GCS backend for remote state (recommended for team collaboration)
  # Uncomment and configure when ready
  # backend "gcs" {
  #   bucket = "sqordia-terraform-state"
  #   prefix = "terraform/state"
  # }
}

provider "google" {
  project = var.gcp_project_id
  region  = var.gcp_region

  default_labels = {
    project     = "sqordia"
    environment = var.environment
    managed_by  = "terraform"
  }
}

