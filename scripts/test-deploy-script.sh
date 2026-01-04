#!/bin/bash
# Test script to validate deploy-gcp-infrastructure.sh logic
# This script validates the deployment script without actually running gcloud commands

echo "=== Testing Deployment Script Logic ==="
echo ""

# Test 1: Check if script exists and is readable
if [ ! -f "scripts/deploy-gcp-infrastructure.sh" ]; then
    echo "❌ Script not found: scripts/deploy-gcp-infrastructure.sh"
    exit 1
fi
echo "✅ Script file exists"

# Test 2: Check if script is executable
if [ ! -x "scripts/deploy-gcp-infrastructure.sh" ]; then
    echo "⚠️  Script is not executable, making it executable..."
    chmod +x scripts/deploy-gcp-infrastructure.sh
fi
echo "✅ Script is executable"

# Test 3: Validate script syntax (basic checks)
echo ""
echo "Validating script structure..."

# Check for required functions
if grep -q "resource_exists" scripts/deploy-gcp-infrastructure.sh; then
    echo "✅ resource_exists function found"
else
    echo "❌ resource_exists function not found"
fi

# Check for required resource types
REQUIRED_RESOURCES=(
    "service-account"
    "sql-instance"
    "storage-bucket"
    "pubsub-topic"
    "cloud-run"
)

for RESOURCE in "${REQUIRED_RESOURCES[@]}"; do
    if grep -q "resource_exists.*$RESOURCE" scripts/deploy-gcp-infrastructure.sh; then
        echo "✅ $RESOURCE handling found"
    else
        echo "⚠️  $RESOURCE handling not found"
    fi
done

# Test 4: Check for required gcloud commands
echo ""
echo "Validating gcloud commands..."

REQUIRED_COMMANDS=(
    "gcloud services enable"
    "gcloud iam service-accounts create"
    "gcloud projects add-iam-policy-binding"
    "gcloud sql instances create"
    "gsutil mb"
    "gcloud pubsub topics create"
    "gcloud run deploy"
    "gcloud functions deploy"
)

for CMD in "${REQUIRED_COMMANDS[@]}"; do
    if grep -q "$CMD" scripts/deploy-gcp-infrastructure.sh; then
        echo "✅ $CMD found"
    else
        echo "⚠️  $CMD not found"
    fi
done

# Test 5: Check parameter handling
echo ""
echo "Validating parameter handling..."

if grep -q 'PROJECT_ID="\${1:-}"' scripts/deploy-gcp-infrastructure.sh; then
    echo "✅ Project ID parameter handling found"
else
    echo "⚠️  Project ID parameter handling may be missing"
fi

if grep -q 'CLOUD_SQL_PASSWORD="\${5:-}"' scripts/deploy-gcp-infrastructure.sh; then
    echo "✅ Cloud SQL password parameter handling found"
else
    echo "⚠️  Cloud SQL password parameter handling may be missing"
fi

# Test 6: Check error handling
echo ""
echo "Validating error handling..."

if grep -q "set +e" scripts/deploy-gcp-infrastructure.sh; then
    echo "✅ Error handling configured (set +e)"
else
    echo "⚠️  Error handling may be strict (set -e)"
fi

if grep -q "resource_exists\|already exists\|already exist" scripts/deploy-gcp-infrastructure.sh; then
    echo "✅ Idempotency checks found"
else
    echo "⚠️  Idempotency checks may be missing"
fi

echo ""
echo "=== Test Summary ==="
echo "Script structure validation completed."
echo ""
echo "To actually test the script, run:"
echo "  scripts/deploy-gcp-infrastructure.sh PROJECT_ID REGION PROJECT_NAME ENVIRONMENT PASSWORD"
echo ""
echo "Example:"
echo "  scripts/deploy-gcp-infrastructure.sh project-b79ef08c-1eb8-47ea-80e northamerica-northeast2 sqordia production 'your-password'"
echo ""

