variable "gcp_project_id" {
  description = "GCP Project ID"
  type        = string
}

variable "gcp_region" {
  description = "GCP region for resources"
  type        = string
  default     = "us-central1"
}

variable "environment" {
  description = "Environment name (dev, staging, production)"
  type        = string
  default     = "production"
}

variable "project_name" {
  description = "Project name for resource naming"
  type        = string
  default     = "sqordia"
}

# Cloud SQL Configuration
variable "cloud_sql_tier" {
  description = "Cloud SQL instance tier (db-f1-micro for free tier, or db-shared-core for cost optimization)"
  type        = string
  default     = "db-f1-micro"
}

variable "cloud_sql_disk_size" {
  description = "Cloud SQL disk size in GB (10GB for cost optimization, 20GB default)"
  type        = number
  default     = 10
}

variable "cloud_sql_database_name" {
  description = "Cloud SQL database name"
  type        = string
  default     = "SqordiaDb"
}

variable "cloud_sql_user" {
  description = "Cloud SQL master username"
  type        = string
  default     = "postgres"
}

variable "cloud_sql_password" {
  description = "Cloud SQL master password (set via terraform.tfvars or TF_VAR_cloud_sql_password)"
  type        = string
  sensitive   = true
  # No default - must be provided
}

# Cloud Run Configuration
variable "cloud_run_cpu" {
  description = "Cloud Run CPU allocation (0.5, 1, 2, 4, etc.)"
  type        = string
  default     = "0.5"
}

variable "cloud_run_memory" {
  description = "Cloud Run memory allocation (e.g., 1Gi, 2Gi)"
  type        = string
  default     = "1Gi"
}

variable "cloud_run_min_instances" {
  description = "Minimum number of Cloud Run instances (0 for cost optimization, 1 to avoid cold starts)"
  type        = number
  default     = 0
}

variable "cloud_run_max_instances" {
  description = "Maximum number of Cloud Run instances"
  type        = number
  default     = 10
}

variable "cloud_run_timeout" {
  description = "Cloud Run request timeout in seconds"
  type        = number
  default     = 300
}

# Cloud Functions Configuration
variable "cloud_functions_timeout" {
  description = "Cloud Functions timeout in seconds"
  type        = number
  default     = 300 # 5 minutes for AI generation
}

variable "cloud_functions_memory" {
  description = "Cloud Functions memory in MB"
  type        = number
  default     = 512
}

# Cloud Storage Configuration
variable "cloud_storage_location" {
  description = "Cloud Storage bucket location"
  type        = string
  default     = "US"
}

variable "cloud_storage_class" {
  description = "Cloud Storage storage class (STANDARD, NEARLINE, COLDLINE, ARCHIVE)"
  type        = string
  default     = "STANDARD"
}

# Cloud Logging Configuration
variable "cloud_logging_retention_days" {
  description = "Cloud Logging retention in days (3 for cost optimization, 7 default)"
  type        = number
  default     = 3
}

# Email Configuration
variable "ses_from_email" {
  description = "Email sender address (for SendGrid or other email service)"
  type        = string
  default     = "noreply@sqordia.com"
}

# Domain Configuration (optional - skipped for cost optimization)
variable "domain_name" {
  description = "Custom domain name for the API. Leave empty to use Cloud Run default URL."
  type        = string
  default     = ""
}

