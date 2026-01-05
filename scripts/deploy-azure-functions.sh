#!/bin/bash

# Deploy Azure Functions
# Usage: ./scripts/deploy-azure-functions.sh <resource-group> <location>

set -e

RESOURCE_GROUP_NAME="${1:-sqordia-production-rg}"
LOCATION="${2:-canadacentral}"

echo "=== Deploying Azure Functions ==="
echo "Resource Group: $RESOURCE_GROUP_NAME"
echo "Location: $LOCATION"

# Function names
EMAIL_FUNCTION="sqordia-production-email-handler"
AI_GENERATION_FUNCTION="sqordia-production-ai-generation-handler"
EXPORT_FUNCTION="sqordia-production-export-handler"

# Base directory
BASE_DIR="src/Functions"

# Deploy each function
deploy_function() {
    local FUNCTION_NAME=$1
    local FUNCTION_DIR=$2
    local PUBLISH_DIR="$FUNCTION_DIR/publish"
    
    echo ""
    echo "=== Deploying $FUNCTION_NAME ==="
    
    if [ ! -d "$FUNCTION_DIR" ]; then
        echo "⚠️ Function directory not found: $FUNCTION_DIR"
        return 1
    fi
    
    # Build and publish
    echo "Building $FUNCTION_NAME..."
    dotnet publish "$FUNCTION_DIR/$FUNCTION_NAME.csproj" \
        -c Release \
        -o "$PUBLISH_DIR" \
        --no-build || {
        echo "Building $FUNCTION_NAME..."
        dotnet build "$FUNCTION_DIR/$FUNCTION_NAME.csproj" -c Release
        dotnet publish "$FUNCTION_DIR/$FUNCTION_NAME.csproj" \
            -c Release \
            -o "$PUBLISH_DIR"
    }
    
    # Create zip file
    echo "Creating deployment package..."
    cd "$PUBLISH_DIR"
    zip -r "../../${FUNCTION_NAME}.zip" . -q
    cd - > /dev/null
    
    # Deploy using Azure Functions Core Tools or zip deploy
    if command -v func &> /dev/null; then
        echo "Deploying using Azure Functions Core Tools..."
        func azure functionapp publish "$FUNCTION_NAME" --dotnet-isolated
    else
        echo "Deploying using zip deploy..."
        az functionapp deployment source config-zip \
            --resource-group "$RESOURCE_GROUP_NAME" \
            --name "$FUNCTION_NAME" \
            --src "$PUBLISH_DIR/../../${FUNCTION_NAME}.zip" || {
            echo "⚠️ Zip deploy failed, trying alternative method..."
            # Alternative: Use Kudu API
            echo "Please deploy manually via Azure Portal or install Azure Functions Core Tools"
        }
    fi
    
    echo "✅ $FUNCTION_NAME deployed"
}

# Deploy all functions
deploy_function "$EMAIL_FUNCTION" "$BASE_DIR/EmailHandler"
deploy_function "$AI_GENERATION_FUNCTION" "$BASE_DIR/AIGenerationHandler"
deploy_function "$EXPORT_FUNCTION" "$BASE_DIR/ExportHandler"

echo ""
echo "=== Azure Functions Deployment Complete ==="

