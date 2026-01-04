#!/bin/bash
# Import Existing GCP Resources into Terraform State
# This script imports resources that already exist in GCP but aren't in Terraform state

set -e

PROJECT_ID="${1:-}"
REGION="${2:-northamerica-northeast2}"
PROJECT_NAME="${3:-sqordia}"
ENVIRONMENT="${4:-production}"

echo ""
echo "=== Importing Existing GCP Resources ==="
echo "Project ID: $PROJECT_ID"
echo "Region: $REGION"
echo "Project Name: $PROJECT_NAME"
echo "Environment: $ENVIRONMENT"
echo ""

if [ -z "$PROJECT_ID" ]; then
    echo "❌ Project ID is required"
    exit 1
fi

# Change to terraform directory
TERRAFORM_DIR="infrastructure/terraform/gcp"
if [ ! -d "$TERRAFORM_DIR" ]; then
    echo "❌ Terraform directory not found: $TERRAFORM_DIR"
    exit 1
fi

cd "$TERRAFORM_DIR"

# Initialize Terraform if needed
if [ ! -d ".terraform" ]; then
    echo "Initializing Terraform..."
    terraform init
    if [ $? -ne 0 ]; then
        echo "❌ Terraform init failed"
        exit 1
    fi
fi

echo ""
echo "Importing resources..."
echo ""

SUCCESS_COUNT=0
ERROR_COUNT=0
SKIPPED_COUNT=0

# Function to import a resource
import_resource() {
    local RESOURCE=$1
    local RESOURCE_ID=$2
    
    echo "Importing: $RESOURCE"
    echo "  ID: $RESOURCE_ID"
    
    # Check if resource is already in state
    if terraform state show "$RESOURCE" > /dev/null 2>&1; then
        echo "  [SKIP] Already in state"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
        return 0
    fi
    
    # Try to import
    export TF_VAR_gcp_project_id="$PROJECT_ID"
    export TF_VAR_gcp_region="$REGION"
    export TF_VAR_project_name="$PROJECT_NAME"
    export TF_VAR_environment="$ENVIRONMENT"
    
    if terraform import "$RESOURCE" "$RESOURCE_ID" 2>&1; then
        echo "  [OK] Successfully imported"
        SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
        return 0
    else
        echo "  [FAIL] Import failed (resource may not exist or ID is incorrect)"
        ERROR_COUNT=$((ERROR_COUNT + 1))
        return 1
    fi
}

# Service Accounts
import_resource "google_service_account.cloud_functions_sa" "projects/$PROJECT_ID/serviceAccounts/${PROJECT_NAME}-${ENVIRONMENT}-functions@${PROJECT_ID}.iam.gserviceaccount.com" || true
import_resource "google_service_account.cloud_run_sa" "projects/$PROJECT_ID/serviceAccounts/${PROJECT_NAME}-${ENVIRONMENT}-run@${PROJECT_ID}.iam.gserviceaccount.com" || true

# Cloud SQL Instance
import_resource "google_sql_database_instance.postgresql" "$PROJECT_ID/${PROJECT_NAME}-${ENVIRONMENT}-db" || true

# Pub/Sub Topics
import_resource "google_pubsub_topic.email_topic" "projects/$PROJECT_ID/topics/${PROJECT_NAME}-${ENVIRONMENT}-email" || true
import_resource "google_pubsub_topic.ai_generation_topic" "projects/$PROJECT_ID/topics/${PROJECT_NAME}-${ENVIRONMENT}-ai-generation" || true
import_resource "google_pubsub_topic.export_topic" "projects/$PROJECT_ID/topics/${PROJECT_NAME}-${ENVIRONMENT}-export" || true

# Secret Manager
import_resource "google_secret_manager_secret.database_connection" "projects/$PROJECT_ID/secrets/${PROJECT_NAME}-${ENVIRONMENT}-db-connection" || true

# Logging Metrics
import_resource "google_logging_metric.api_errors" "projects/$PROJECT_ID/metrics/${PROJECT_NAME}-${ENVIRONMENT}-api-errors" || true
import_resource "google_logging_metric.api_requests" "projects/$PROJECT_ID/metrics/${PROJECT_NAME}-${ENVIRONMENT}-api-requests" || true

# Artifact Registry
import_resource "google_artifact_registry_repository.container_repo" "projects/$PROJECT_ID/locations/$REGION/repositories/${PROJECT_NAME}-${ENVIRONMENT}-repo" || true

# Storage Buckets (may have different names due to global uniqueness)
echo ""
echo "Checking Storage Buckets..."
echo "Note: Bucket names are globally unique and may differ from expected names"
echo ""

# List all buckets in the project
ALL_BUCKETS=$(gcloud storage buckets list --project="$PROJECT_ID" --format="value(name)" 2>/dev/null || echo "")

# Try to import documents bucket
DOCUMENTS_BUCKET="${PROJECT_NAME}-${ENVIRONMENT}-documents"
if echo "$ALL_BUCKETS" | grep -q "$DOCUMENTS_BUCKET"; then
    import_resource "google_storage_bucket.documents" "$DOCUMENTS_BUCKET" || true
else
    # Try to find a bucket matching the pattern
    MATCHING_BUCKET=$(echo "$ALL_BUCKETS" | grep -i "documents" | head -1)
    if [ -n "$MATCHING_BUCKET" ]; then
        echo "Found bucket with different name: $MATCHING_BUCKET"
        import_resource "google_storage_bucket.documents" "$MATCHING_BUCKET" || true
    else
        echo "  [SKIP] Documents bucket not found"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Try to import functions-source bucket
FUNCTIONS_BUCKET="${PROJECT_NAME}-${ENVIRONMENT}-functions-source"
if echo "$ALL_BUCKETS" | grep -q "$FUNCTIONS_BUCKET"; then
    import_resource "google_storage_bucket.functions_source" "$FUNCTIONS_BUCKET" || true
else
    # Try to find a bucket matching the pattern
    MATCHING_BUCKET=$(echo "$ALL_BUCKETS" | grep -i "functions-source\|functions_source" | head -1)
    if [ -n "$MATCHING_BUCKET" ]; then
        echo "Found bucket with different name: $MATCHING_BUCKET"
        import_resource "google_storage_bucket.functions_source" "$MATCHING_BUCKET" || true
    else
        echo "  [SKIP] Functions source bucket not found"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

echo ""
echo "=== Import Summary ==="
echo "  Successfully imported: $SUCCESS_COUNT"
if [ $ERROR_COUNT -gt 0 ]; then
    echo "  Failed: $ERROR_COUNT"
else
    echo "  Failed: $ERROR_COUNT"
fi
echo "  Skipped (already in state): $SKIPPED_COUNT"
echo ""

if [ $ERROR_COUNT -gt 0 ]; then
    echo "⚠️  Some resources failed to import. This may be because:"
    echo "  1. The resource doesn't exist with the expected name"
    echo "  2. The resource ID format is incorrect"
    echo "  3. You don't have permission to access the resource"
    echo ""
fi

if [ $SUCCESS_COUNT -gt 0 ] || [ $SKIPPED_COUNT -gt 0 ]; then
    echo "✅ Next steps:"
    echo "  1. Run 'terraform plan' to see if there are any differences"
    echo "  2. If there are differences, review and update your Terraform configuration"
    echo "  3. Run 'terraform apply' to sync any configuration changes"
    echo ""
fi

cd - > /dev/null

