#!/bin/bash
# Prepare GCP Resources for Terraform
# This script either imports existing resources or cleans them up if they can be safely recreated

set +e  # Don't exit on error

PROJECT_ID="${1:-}"
REGION="${2:-northamerica-northeast2}"
PROJECT_NAME="${3:-sqordia}"
ENVIRONMENT="${4:-production}"

echo ""
echo "=== Preparing GCP Resources for Terraform ==="
echo "Project ID: $PROJECT_ID"
echo "Region: $REGION"
echo "Project Name: $PROJECT_NAME"
echo "Environment: $ENVIRONMENT"
echo ""

if [ -z "$PROJECT_ID" ]; then
    echo "❌ Project ID is required"
    exit 1
fi

TERRAFORM_DIR="infrastructure/terraform/gcp"
if [ ! -d "$TERRAFORM_DIR" ]; then
    echo "❌ Terraform directory not found: $TERRAFORM_DIR"
    exit 1
fi

cd "$TERRAFORM_DIR" || exit 1

# Initialize Terraform if needed
if [ ! -d ".terraform" ]; then
    echo "Initializing Terraform..."
    terraform init > /dev/null 2>&1
fi

# Set environment variables
export TF_VAR_gcp_project_id="$PROJECT_ID"
export TF_VAR_gcp_region="$REGION"
export TF_VAR_project_name="$PROJECT_NAME"
export TF_VAR_environment="$ENVIRONMENT"

IMPORTED_COUNT=0
DELETED_COUNT=0
SKIPPED_COUNT=0
ERROR_COUNT=0

# Function to check if resource exists in Terraform state
in_state() {
    terraform state show "$1" > /dev/null 2>&1
}

# Function to try importing a resource
try_import() {
    local RESOURCE=$1
    local RESOURCE_ID=$2
    
    if in_state "$RESOURCE"; then
        echo "  [SKIP] Already in Terraform state"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
        return 0
    fi
    
    echo "  Attempting to import..."
    if terraform import "$RESOURCE" "$RESOURCE_ID" > /dev/null 2>&1; then
        echo "  [OK] Successfully imported into Terraform state"
        IMPORTED_COUNT=$((IMPORTED_COUNT + 1))
        return 0
    else
        return 1
    fi
}

# Function to check if resource exists in GCP and handle it
handle_resource() {
    local RESOURCE_TYPE=$1
    local RESOURCE_NAME=$2
    local TERRAFORM_RESOURCE=$3
    local IMPORT_ID=$4
    local CAN_DELETE=${5:-false}
    
    echo ""
    echo "Checking: $RESOURCE_NAME"
    
    # First, check if it's already in Terraform state
    if in_state "$TERRAFORM_RESOURCE"; then
        echo "  [SKIP] Already managed by Terraform"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
        return 0
    fi
    
    # Try to import it
    if [ -n "$IMPORT_ID" ]; then
        if try_import "$TERRAFORM_RESOURCE" "$IMPORT_ID"; then
            return 0
        fi
    fi
    
    # If import failed and resource can be safely deleted, delete it
    if [ "$CAN_DELETE" = "true" ]; then
        echo "  Import failed, attempting to delete (will be recreated by Terraform)..."
        case "$RESOURCE_TYPE" in
            "pubsub-topic")
                if gcloud pubsub topics describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1; then
                    gcloud pubsub topics delete "$RESOURCE_NAME" --project="$PROJECT_ID" --quiet > /dev/null 2>&1
                    if [ $? -eq 0 ]; then
                        echo "  [OK] Deleted (will be recreated)"
                        DELETED_COUNT=$((DELETED_COUNT + 1))
                        return 0
                    fi
                fi
                ;;
            "pubsub-subscription")
                if gcloud pubsub subscriptions describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1; then
                    gcloud pubsub subscriptions delete "$RESOURCE_NAME" --project="$PROJECT_ID" --quiet > /dev/null 2>&1
                    if [ $? -eq 0 ]; then
                        echo "  [OK] Deleted (will be recreated)"
                        DELETED_COUNT=$((DELETED_COUNT + 1))
                        return 0
                    fi
                fi
                ;;
            "logging-metric")
                if gcloud logging metrics describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1; then
                    gcloud logging metrics delete "$RESOURCE_NAME" --project="$PROJECT_ID" --quiet > /dev/null 2>&1
                    if [ $? -eq 0 ]; then
                        echo "  [OK] Deleted (will be recreated)"
                        DELETED_COUNT=$((DELETED_COUNT + 1))
                        return 0
                    fi
                fi
                ;;
            "cloud-run")
                if gcloud run services describe "$RESOURCE_NAME" --region="$REGION" --project="$PROJECT_ID" > /dev/null 2>&1; then
                    gcloud run services delete "$RESOURCE_NAME" --region="$REGION" --project="$PROJECT_ID" --quiet > /dev/null 2>&1
                    if [ $? -eq 0 ]; then
                        echo "  [OK] Deleted (will be recreated)"
                        DELETED_COUNT=$((DELETED_COUNT + 1))
                        return 0
                    fi
                fi
                ;;
        esac
    fi
    
    echo "  [SKIP] Resource doesn't exist or couldn't be imported/deleted"
    SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    return 0
}

echo "Processing resources..."
echo ""

# Pub/Sub Topics - try import first, delete if import fails
handle_resource "pubsub-topic" "${PROJECT_NAME}-${ENVIRONMENT}-email" \
    "google_pubsub_topic.email_topic" \
    "projects/$PROJECT_ID/topics/${PROJECT_NAME}-${ENVIRONMENT}-email" \
    true

handle_resource "pubsub-topic" "${PROJECT_NAME}-${ENVIRONMENT}-ai-generation" \
    "google_pubsub_topic.ai_generation_topic" \
    "projects/$PROJECT_ID/topics/${PROJECT_NAME}-${ENVIRONMENT}-ai-generation" \
    true

handle_resource "pubsub-topic" "${PROJECT_NAME}-${ENVIRONMENT}-export" \
    "google_pubsub_topic.export_topic" \
    "projects/$PROJECT_ID/topics/${PROJECT_NAME}-${ENVIRONMENT}-export" \
    true

# Pub/Sub Subscriptions
handle_resource "pubsub-subscription" "${PROJECT_NAME}-${ENVIRONMENT}-email-sub" \
    "google_pubsub_subscription.email_subscription" \
    "projects/$PROJECT_ID/subscriptions/${PROJECT_NAME}-${ENVIRONMENT}-email-sub" \
    true

handle_resource "pubsub-subscription" "${PROJECT_NAME}-${ENVIRONMENT}-ai-generation-sub" \
    "google_pubsub_subscription.ai_generation_subscription" \
    "projects/$PROJECT_ID/subscriptions/${PROJECT_NAME}-${ENVIRONMENT}-ai-generation-sub" \
    true

handle_resource "pubsub-subscription" "${PROJECT_NAME}-${ENVIRONMENT}-export-sub" \
    "google_pubsub_subscription.export_subscription" \
    "projects/$PROJECT_ID/subscriptions/${PROJECT_NAME}-${ENVIRONMENT}-export-sub" \
    true

# Logging Metrics
handle_resource "logging-metric" "${PROJECT_NAME}-${ENVIRONMENT}-api-errors" \
    "google_logging_metric.api_errors" \
    "projects/$PROJECT_ID/metrics/${PROJECT_NAME}-${ENVIRONMENT}-api-errors" \
    true

handle_resource "logging-metric" "${PROJECT_NAME}-${ENVIRONMENT}-api-requests" \
    "google_logging_metric.api_requests" \
    "projects/$PROJECT_ID/metrics/${PROJECT_NAME}-${ENVIRONMENT}-api-requests" \
    true

# Cloud Run Service
handle_resource "cloud-run" "${PROJECT_NAME}-${ENVIRONMENT}-api" \
    "google_cloud_run_v2_service.api_service" \
    "projects/$PROJECT_ID/locations/$REGION/services/${PROJECT_NAME}-${ENVIRONMENT}-api" \
    true

# Critical resources - only import, never delete
echo ""
echo "Processing critical resources (import only, never delete)..."
echo ""

# Cloud SQL Instance - import only, never delete
if ! in_state "google_sql_database_instance.postgresql"; then
    echo "Checking: Cloud SQL Instance"
    if gcloud sql instances describe "${PROJECT_NAME}-${ENVIRONMENT}-db" --project="$PROJECT_ID" > /dev/null 2>&1; then
        try_import "google_sql_database_instance.postgresql" "$PROJECT_ID/${PROJECT_NAME}-${ENVIRONMENT}-db"
    else
        echo "  [SKIP] Cloud SQL instance doesn't exist yet"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Cloud SQL Database
if ! in_state "google_sql_database.postgresql_db"; then
    echo "Checking: Cloud SQL Database (SqordiaDb)"
    if gcloud sql databases describe "SqordiaDb" --instance="${PROJECT_NAME}-${ENVIRONMENT}-db" --project="$PROJECT_ID" > /dev/null 2>&1; then
        try_import "google_sql_database.postgresql_db" "$PROJECT_ID/${PROJECT_NAME}-${ENVIRONMENT}-db/SqordiaDb"
    else
        echo "  [SKIP] Database doesn't exist yet"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Service Accounts - import only, never delete
echo ""
echo "Processing service accounts (import only)..."
echo ""

# Cloud Functions Service Account
if ! in_state "google_service_account.cloud_functions_sa"; then
    echo "Checking: Cloud Functions Service Account"
    SA_EMAIL="${PROJECT_NAME}-${ENVIRONMENT}-functions@${PROJECT_ID}.iam.gserviceaccount.com"
    if gcloud iam service-accounts describe "$SA_EMAIL" --project="$PROJECT_ID" > /dev/null 2>&1; then
        try_import "google_service_account.cloud_functions_sa" "projects/$PROJECT_ID/serviceAccounts/$SA_EMAIL"
    else
        echo "  [SKIP] Service account doesn't exist yet"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Cloud Run Service Account
if ! in_state "google_service_account.cloud_run_sa"; then
    echo "Checking: Cloud Run Service Account"
    SA_EMAIL="${PROJECT_NAME}-${ENVIRONMENT}-run@${PROJECT_ID}.iam.gserviceaccount.com"
    if gcloud iam service-accounts describe "$SA_EMAIL" --project="$PROJECT_ID" > /dev/null 2>&1; then
        try_import "google_service_account.cloud_run_sa" "projects/$PROJECT_ID/serviceAccounts/$SA_EMAIL"
    else
        echo "  [SKIP] Service account doesn't exist yet"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Storage Buckets - import if they have data, delete if empty
echo ""
echo "Processing storage buckets..."
echo ""

# Functions Source Bucket
if ! in_state "google_storage_bucket.functions_source"; then
    BUCKET_NAME="${PROJECT_NAME}-${ENVIRONMENT}-functions-source"
    echo "Checking: $BUCKET_NAME"
    if gsutil ls -b "gs://$BUCKET_NAME" > /dev/null 2>&1; then
        OBJECT_COUNT=$(gsutil ls "gs://$BUCKET_NAME/**" 2>/dev/null | wc -l)
        if [ "$OBJECT_COUNT" -eq 0 ]; then
            echo "  Bucket is empty, deleting (will be recreated)..."
            gsutil rm -r "gs://$BUCKET_NAME" > /dev/null 2>&1
            if [ $? -eq 0 ]; then
                echo "  [OK] Deleted empty bucket"
                DELETED_COUNT=$((DELETED_COUNT + 1))
            else
                echo "  [FAIL] Failed to delete bucket"
                ERROR_COUNT=$((ERROR_COUNT + 1))
            fi
        else
            echo "  Bucket has $OBJECT_COUNT object(s)"
            # Try to import first
            if try_import "google_storage_bucket.functions_source" "$BUCKET_NAME"; then
                echo "  [OK] Imported bucket with objects"
            else
                echo "  Import failed, deleting objects and bucket (will be recreated)..."
                # Delete all objects first
                gsutil rm "gs://$BUCKET_NAME/**" > /dev/null 2>&1
                # Then delete the bucket
                gsutil rm -r "gs://$BUCKET_NAME" > /dev/null 2>&1
                if [ $? -eq 0 ]; then
                    echo "  [OK] Deleted bucket and objects (will be recreated)"
                    DELETED_COUNT=$((DELETED_COUNT + 1))
                else
                    echo "  [FAIL] Failed to delete bucket"
                    ERROR_COUNT=$((ERROR_COUNT + 1))
                fi
            fi
        fi
    else
        echo "  [SKIP] Bucket doesn't exist"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Documents Bucket
if ! in_state "google_storage_bucket.documents"; then
    BUCKET_NAME="${PROJECT_NAME}-${ENVIRONMENT}-documents"
    echo "Checking: $BUCKET_NAME"
    if gsutil ls -b "gs://$BUCKET_NAME" > /dev/null 2>&1; then
        echo "  Bucket exists, importing..."
        try_import "google_storage_bucket.documents" "$BUCKET_NAME"
    else
        echo "  [SKIP] Bucket doesn't exist"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Artifact Registry Repository - import only, never delete
if ! in_state "google_artifact_registry_repository.container_repo"; then
    echo ""
    echo "Checking: Artifact Registry Repository"
    REPO_NAME="${PROJECT_NAME}-${ENVIRONMENT}-repo"
    if gcloud artifacts repositories describe "$REPO_NAME" --location="$REGION" --project="$PROJECT_ID" > /dev/null 2>&1; then
        try_import "google_artifact_registry_repository.container_repo" "projects/$PROJECT_ID/locations/$REGION/repositories/$REPO_NAME"
    else
        echo "  [SKIP] Repository doesn't exist yet"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

# Secret Manager Secret - import only, never delete
if ! in_state "google_secret_manager_secret.database_connection"; then
    echo ""
    echo "Checking: Secret Manager Secret"
    SECRET_NAME="${PROJECT_NAME}-${ENVIRONMENT}-db-connection"
    if gcloud secrets describe "$SECRET_NAME" --project="$PROJECT_ID" > /dev/null 2>&1; then
        try_import "google_secret_manager_secret.database_connection" "projects/$PROJECT_ID/secrets/$SECRET_NAME"
    else
        echo "  [SKIP] Secret doesn't exist yet"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
fi

echo ""
echo "=== Summary ==="
echo "  ✅ Imported into Terraform state: $IMPORTED_COUNT"
echo "  🗑️  Deleted (will be recreated): $DELETED_COUNT"
if [ $ERROR_COUNT -gt 0 ]; then
    echo "  ❌ Errors: $ERROR_COUNT"
else
    echo "  ✅ Errors: $ERROR_COUNT (none)"
fi
echo "  ⏭️  Skipped: $SKIPPED_COUNT"
echo ""

if [ $IMPORTED_COUNT -gt 0 ] || [ $DELETED_COUNT -gt 0 ]; then
    echo "✅ Preparation completed. Terraform can now proceed without 'already exists' errors."
    echo ""
fi

cd - > /dev/null

