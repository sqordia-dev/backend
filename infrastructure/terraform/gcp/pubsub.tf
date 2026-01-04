# Pub/Sub Topics
resource "google_pubsub_topic" "email_topic" {
  name = "${var.project_name}-${var.environment}-email"
}

resource "google_pubsub_topic" "ai_generation_topic" {
  name = "${var.project_name}-${var.environment}-ai-generation"
}

resource "google_pubsub_topic" "export_topic" {
  name = "${var.project_name}-${var.environment}-export"
}

# Pub/Sub Subscriptions (for Cloud Functions)
resource "google_pubsub_subscription" "email_subscription" {
  name  = "${var.project_name}-${var.environment}-email-sub"
  topic = google_pubsub_topic.email_topic.name

  ack_deadline_seconds = 60

  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  expiration_policy {
    ttl = "" # Never expire
  }
}

resource "google_pubsub_subscription" "ai_generation_subscription" {
  name  = "${var.project_name}-${var.environment}-ai-generation-sub"
  topic = google_pubsub_topic.ai_generation_topic.name

  ack_deadline_seconds = 300 # 5 minutes for AI generation

  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  expiration_policy {
    ttl = "" # Never expire
  }
}

resource "google_pubsub_subscription" "export_subscription" {
  name  = "${var.project_name}-${var.environment}-export-sub"
  topic = google_pubsub_topic.export_topic.name

  ack_deadline_seconds = 60

  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  expiration_policy {
    ttl = "" # Never expire
  }
}

