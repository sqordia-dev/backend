#!/bin/bash
# Build and Package Cloud Functions for GCP
# This script builds all Cloud Functions and packages them for deployment

set -e

PROJECT_ID="${1:-}"
REGION="${2:-northamerica-northeast2}"

echo ""
echo "=== Build Cloud Functions for GCP ==="
echo "Region: $REGION"
echo ""

# Check prerequisites
echo "[Checking Prerequisites]"

if ! command -v dotnet &> /dev/null; then
    echo "❌ dotnet CLI not found. Please install .NET SDK first."
    exit 1
fi

if ! command -v gcloud &> /dev/null; then
    echo "❌ gcloud CLI not found. Please install Google Cloud SDK first."
    exit 1
fi

echo "✅ Prerequisites met"

# Get project ID from terraform.tfvars if not provided
if [ -z "$PROJECT_ID" ]; then
    TFVARS_PATH="infrastructure/terraform/gcp/terraform.tfvars"
    if [ -f "$TFVARS_PATH" ]; then
        PROJECT_ID=$(grep -E '^\s*gcp_project_id\s*=' "$TFVARS_PATH" | sed -E 's/.*=\s*"([^"]+)".*/\1/' | head -1)
        if [ -n "$PROJECT_ID" ]; then
            echo "Using project ID from terraform.tfvars: $PROJECT_ID"
        fi
    fi
    
    if [ -z "$PROJECT_ID" ]; then
        echo "❌ Project ID not found. Please provide PROJECT_ID as first argument or set it in terraform.tfvars."
        exit 1
    fi
fi

# Set GCP project
echo ""
echo "[Setting GCP Project]"
gcloud config set project "$PROJECT_ID" > /dev/null
echo "✅ Project set to $PROJECT_ID"

# Functions to build
declare -a FUNCTIONS=(
    "EmailHandler:src/Functions/EmailHandler"
    "AIGenerationHandler:src/Functions/AIGenerationHandler"
    "ExportHandler:src/Functions/ExportHandler"
)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_DIR="$REPO_ROOT/infrastructure/terraform/gcp"
mkdir -p "$OUTPUT_DIR"

for FUNCTION_SPEC in "${FUNCTIONS[@]}"; do
    FUNCTION_NAME="${FUNCTION_SPEC%%:*}"
    FUNCTION_PATH="${FUNCTION_SPEC#*:}"
    FUNCTION_ABS_PATH="$REPO_ROOT/$FUNCTION_PATH"
    
    echo ""
    echo "[Building $FUNCTION_NAME]"
    
    if [ ! -d "$FUNCTION_ABS_PATH" ]; then
        echo "❌ Function path not found: $FUNCTION_ABS_PATH"
        continue
    fi
    
    cd "$FUNCTION_ABS_PATH"
    
    # Clean previous builds
    if [ -d "./publish" ]; then
        rm -rf "./publish"
    fi
    
    # Build and publish
    echo "Publishing $FUNCTION_NAME..."
    dotnet publish -c Release -o ./publish -p:PublishReadyToRun=false
    
    if [ $? -ne 0 ]; then
        echo "❌ Failed to publish $FUNCTION_NAME"
        cd "$REPO_ROOT"
        continue
    fi
    
    # Create zip file
    ZIP_NAME=$(echo "$FUNCTION_NAME" | tr '[:upper:]' '[:lower:]')
    ZIP_PATH="$OUTPUT_DIR/${ZIP_NAME}-handler.zip"
    
    if [ -f "$ZIP_PATH" ]; then
        rm -f "$ZIP_PATH"
    fi
    
    echo "Creating zip archive..."
    cd publish
    zip -r "$ZIP_PATH" . > /dev/null
    cd "$REPO_ROOT"
    
    echo "✅ $FUNCTION_NAME built and packaged: $ZIP_PATH"
    
    # Upload to Cloud Storage
    BUCKET_NAME="sqordia-production-functions-source"
    echo "Uploading to Cloud Storage bucket: $BUCKET_NAME"
    
    gsutil cp "$ZIP_PATH" "gs://$BUCKET_NAME/${ZIP_NAME}-handler.zip"
    
    if [ $? -eq 0 ]; then
        echo "✅ $FUNCTION_NAME uploaded to Cloud Storage"
    else
        echo "❌ Failed to upload $FUNCTION_NAME to Cloud Storage"
    fi
done

echo ""
echo "=== Build Complete ==="
echo ""
echo "Next steps:"
echo "1. Update Terraform to reference the uploaded zip files"
echo "2. Run terraform apply to deploy the functions"
echo ""

