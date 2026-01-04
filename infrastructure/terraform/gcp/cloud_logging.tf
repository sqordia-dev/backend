# Cloud Logging Sink (optional - for log export to Cloud Storage)
# This is optional and can be used for long-term log storage

# Log-based metric for error tracking
resource "google_logging_metric" "api_errors" {
  name   = "${var.project_name}-${var.environment}-api-errors"
  filter = "resource.type=cloud_run_revision AND severity>=ERROR"
  metric_descriptor {
    metric_kind = "DELTA"
    value_type  = "INT64"
  }
}

# Log-based metric for request count
resource "google_logging_metric" "api_requests" {
  name   = "${var.project_name}-${var.environment}-api-requests"
  filter = "resource.type=cloud_run_revision AND httpRequest.requestMethod!=\"\""
  metric_descriptor {
    metric_kind = "DELTA"
    value_type  = "INT64"
  }
}

# Note: Log retention is configured at the project level or via organization policy
# Default retention is 30 days, but can be reduced to 3 days for cost optimization
# This is typically done via gcloud or console, not Terraform

