#!/bin/bash
# Cleanup Existing GCP Resources Before Terraform Apply
# This script removes resources that can be safely recreated to avoid "already exists" errors
# WARNING: This will delete resources! Use with caution.

set +e  # Don't exit on error - continue cleaning up other resources

PROJECT_ID="${1:-}"
REGION="${2:-northamerica-northeast2}"
PROJECT_NAME="${3:-sqordia}"
ENVIRONMENT="${4:-production}"

echo ""
echo "=== Cleaning Up Existing GCP Resources ==="
echo "Project ID: $PROJECT_ID"
echo "Region: $REGION"
echo "Project Name: $PROJECT_NAME"
echo "Environment: $ENVIRONMENT"
echo ""
echo "⚠️  WARNING: This will delete resources that can be safely recreated"
echo ""

if [ -z "$PROJECT_ID" ]; then
    echo "❌ Project ID is required"
    exit 1
fi

SUCCESS_COUNT=0
ERROR_COUNT=0
SKIPPED_COUNT=0

# Function to delete a resource
delete_resource() {
    local RESOURCE_TYPE=$1
    local RESOURCE_NAME=$2
    local DESCRIPTION=$3
    
    echo "Deleting: $RESOURCE_TYPE - $RESOURCE_NAME"
    echo "  Description: $DESCRIPTION"
    
    case "$RESOURCE_TYPE" in
        "pubsub-topic")
            gcloud pubsub topics delete "$RESOURCE_NAME" --project="$PROJECT_ID" 2>&1
            ;;
        "pubsub-subscription")
            gcloud pubsub subscriptions delete "$RESOURCE_NAME" --project="$PROJECT_ID" 2>&1
            ;;
        "logging-metric")
            gcloud logging metrics delete "$RESOURCE_NAME" --project="$PROJECT_ID" 2>&1
            ;;
        "storage-bucket")
            # Check if bucket exists and is empty
            if gsutil ls -b "gs://$RESOURCE_NAME" > /dev/null 2>&1; then
                # Try to delete with force (will fail if not empty and force_destroy not set)
                gsutil rm -r "gs://$RESOURCE_NAME" 2>&1 || echo "  [SKIP] Bucket not empty or has force_destroy=false"
            else
                echo "  [SKIP] Bucket doesn't exist"
                SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
                return 0
            fi
            ;;
        "cloud-run")
            gcloud run services delete "$RESOURCE_NAME" --region="$REGION" --project="$PROJECT_ID" --quiet 2>&1
            ;;
        "artifact-registry")
            gcloud artifacts repositories delete "$RESOURCE_NAME" --location="$REGION" --project="$PROJECT_ID" --quiet 2>&1
            ;;
        *)
            echo "  [SKIP] Unknown resource type: $RESOURCE_TYPE"
            SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
            return 0
            ;;
    esac
    
    if [ $? -eq 0 ]; then
        echo "  [OK] Successfully deleted"
        SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
        return 0
    else
        echo "  [FAIL] Failed to delete (may not exist or may be in use)"
        ERROR_COUNT=$((ERROR_COUNT + 1))
        return 1
    fi
}

# Resources that can be safely deleted and recreated
echo "Deleting resources that can be safely recreated..."
echo ""

# Pub/Sub Topics (subscriptions will be recreated)
delete_resource "pubsub-topic" "${PROJECT_NAME}-${ENVIRONMENT}-email" "Email topic" || true
delete_resource "pubsub-topic" "${PROJECT_NAME}-${ENVIRONMENT}-ai-generation" "AI generation topic" || true
delete_resource "pubsub-topic" "${PROJECT_NAME}-${ENVIRONMENT}-export" "Export topic" || true

# Pub/Sub Subscriptions
delete_resource "pubsub-subscription" "${PROJECT_NAME}-${ENVIRONMENT}-email-sub" "Email subscription" || true
delete_resource "pubsub-subscription" "${PROJECT_NAME}-${ENVIRONMENT}-ai-generation-sub" "AI generation subscription" || true
delete_resource "pubsub-subscription" "${PROJECT_NAME}-${ENVIRONMENT}-export-sub" "Export subscription" || true

# Logging Metrics
delete_resource "logging-metric" "${PROJECT_NAME}-${ENVIRONMENT}-api-errors" "API errors metric" || true
delete_resource "logging-metric" "${PROJECT_NAME}-${ENVIRONMENT}-api-requests" "API requests metric" || true

# Cloud Run Service (will be recreated with new image)
delete_resource "cloud-run" "${PROJECT_NAME}-${ENVIRONMENT}-api" "Cloud Run API service" || true

# Artifact Registry (if empty, can be recreated)
# Note: We'll skip this as it may contain images
echo "Skipping Artifact Registry deletion (may contain images)"
SKIPPED_COUNT=$((SKIPPED_COUNT + 1))

# Storage Buckets - be careful with these
echo ""
echo "⚠️  Storage buckets:"
echo "  - Documents bucket: Will be skipped (may contain data)"
echo "  - Functions source bucket: Will attempt to delete if empty"
SKIPPED_COUNT=$((SKIPPED_COUNT + 1))

# Try to delete functions-source bucket if it exists and is empty
FUNCTIONS_BUCKET="${PROJECT_NAME}-${ENVIRONMENT}-functions-source"
if gsutil ls -b "gs://$FUNCTIONS_BUCKET" > /dev/null 2>&1; then
    # Check if bucket is empty
    OBJECT_COUNT=$(gsutil ls "gs://$FUNCTIONS_BUCKET/**" 2>/dev/null | wc -l)
    if [ "$OBJECT_COUNT" -eq 0 ]; then
        delete_resource "storage-bucket" "$FUNCTIONS_BUCKET" "Functions source bucket (empty)" || true
    else
        echo "  [SKIP] Functions source bucket contains objects, skipping deletion"
        SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
    fi
else
    echo "  [SKIP] Functions source bucket doesn't exist"
    SKIPPED_COUNT=$((SKIPPED_COUNT + 1))
fi

# Note: We're NOT deleting:
# - Cloud SQL instances (contain data)
# - Cloud SQL databases (contain data)
# - Service accounts (may have IAM bindings)
# - Secret Manager secrets (contain sensitive data)
# - IAM bindings (will be recreated by Terraform if missing)

echo ""
echo "=== Cleanup Summary ==="
echo "  ✅ Successfully deleted: $SUCCESS_COUNT"
if [ $ERROR_COUNT -gt 0 ]; then
    echo "  ❌ Failed: $ERROR_COUNT"
else
    echo "  ✅ Failed: $ERROR_COUNT (none)"
fi
echo "  ⏭️  Skipped: $SKIPPED_COUNT"
echo ""

if [ $SUCCESS_COUNT -gt 0 ]; then
    echo "✅ Cleanup completed. Terraform can now create these resources."
    echo ""
    echo "Note: Some resources were skipped to preserve data:"
    echo "  - Cloud SQL instances and databases"
    echo "  - Storage buckets with data"
    echo "  - Service accounts"
    echo "  - Secret Manager secrets"
    echo ""
    echo "These will need to be imported into Terraform state if they already exist."
    echo ""
fi

