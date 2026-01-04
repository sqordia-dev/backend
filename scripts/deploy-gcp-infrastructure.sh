#!/bin/bash
# Deploy GCP Infrastructure without Terraform
# This script creates all GCP resources using gcloud commands directly

set +e  # Don't exit on error - continue with other resources

PROJECT_ID="${1:-}"
REGION="${2:-northamerica-northeast2}"
PROJECT_NAME="${3:-sqordia}"
ENVIRONMENT="${4:-production}"
CLOUD_SQL_PASSWORD="${5:-}"

# Default values
CLOUD_SQL_TIER="${6:-db-f1-micro}"
CLOUD_SQL_DISK_SIZE="${7:-10}"
CLOUD_SQL_DB_NAME="${8:-SqordiaDb}"
CLOUD_SQL_USER="${9:-postgres}"
CLOUD_RUN_CPU="${10:-0.5}"
CLOUD_RUN_MEMORY="${11:-1Gi}"
CLOUD_RUN_MIN_INSTANCES="${12:-0}"
CLOUD_RUN_MAX_INSTANCES="${13:-10}"
CLOUD_STORAGE_LOCATION="${14:-US}"

echo ""
echo "=== Deploying GCP Infrastructure ==="
echo "Project ID: $PROJECT_ID"
echo "Region: $REGION"
echo "Project Name: $PROJECT_NAME"
echo "Environment: $ENVIRONMENT"
echo ""

if [ -z "$PROJECT_ID" ]; then
    echo "❌ Project ID is required"
    exit 1
fi

if [ -z "$CLOUD_SQL_PASSWORD" ]; then
    echo "⚠️  Warning: Cloud SQL password not provided. Database creation will be skipped."
fi

# Set project
gcloud config set project "$PROJECT_ID" > /dev/null 2>&1

# Enable required APIs
echo "Enabling required GCP APIs..."
APIS=(
    "cloudresourcemanager.googleapis.com"
    "sqladmin.googleapis.com"
    "run.googleapis.com"
    "cloudfunctions.googleapis.com"
    "storage.googleapis.com"
    "pubsub.googleapis.com"
    "secretmanager.googleapis.com"
    "artifactregistry.googleapis.com"
    "logging.googleapis.com"
    "iam.googleapis.com"
)

for API in "${APIS[@]}"; do
    gcloud services enable "$API" --project="$PROJECT_ID" > /dev/null 2>&1 || echo "  API $API may already be enabled"
done
echo "✅ APIs enabled"
echo ""

# Helper function to check if resource exists
resource_exists() {
    local RESOURCE_TYPE=$1
    local RESOURCE_NAME=$2
    local EXTRA_ARGS="${3:-}"
    
    case "$RESOURCE_TYPE" in
        "service-account")
            gcloud iam service-accounts describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "sql-instance")
            gcloud sql instances describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "sql-database")
            gcloud sql databases describe "$RESOURCE_NAME" --instance="$EXTRA_ARGS" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "storage-bucket")
            gsutil ls -b "gs://$RESOURCE_NAME" > /dev/null 2>&1
            ;;
        "pubsub-topic")
            gcloud pubsub topics describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "pubsub-subscription")
            gcloud pubsub subscriptions describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "secret")
            gcloud secrets describe "$RESOURCE_NAME" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "artifact-repo")
            gcloud artifacts repositories describe "$RESOURCE_NAME" --location="$REGION" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "cloud-run")
            gcloud run services describe "$RESOURCE_NAME" --region="$REGION" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        "cloud-function")
            gcloud functions describe "$RESOURCE_NAME" --region="$REGION" --project="$PROJECT_ID" > /dev/null 2>&1
            ;;
        *)
            return 1
            ;;
    esac
}

# Create Service Accounts
echo "Creating service accounts..."
echo ""

# Cloud Run Service Account
SA_RUN="${PROJECT_NAME}-${ENVIRONMENT}-run"
SA_RUN_EMAIL="${SA_RUN}@${PROJECT_ID}.iam.gserviceaccount.com"
if ! resource_exists "service-account" "$SA_RUN_EMAIL"; then
    echo "Creating: $SA_RUN"
    gcloud iam service-accounts create "$SA_RUN" \
        --display-name="Cloud Run Service Account for ${PROJECT_NAME} ${ENVIRONMENT}" \
        --description="Service account for Cloud Run service" \
        --project="$PROJECT_ID" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "  ✅ Created"
    else
        echo "  ❌ Failed"
    fi
else
    echo "  ⏭️  $SA_RUN already exists"
fi

# Cloud Functions Service Account
SA_FUNCTIONS="${PROJECT_NAME}-${ENVIRONMENT}-functions"
SA_FUNCTIONS_EMAIL="${SA_FUNCTIONS}@${PROJECT_ID}.iam.gserviceaccount.com"
if ! resource_exists "service-account" "$SA_FUNCTIONS_EMAIL"; then
    echo "Creating: $SA_FUNCTIONS"
    gcloud iam service-accounts create "$SA_FUNCTIONS" \
        --display-name="Cloud Functions Service Account for ${PROJECT_NAME} ${ENVIRONMENT}" \
        --description="Service account for Cloud Functions" \
        --project="$PROJECT_ID" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "  ✅ Created"
    else
        echo "  ❌ Failed"
    fi
else
    echo "  ⏭️  $SA_FUNCTIONS already exists"
fi

# Grant IAM roles to service accounts
echo ""
echo "Granting IAM roles..."

# Cloud Run SA roles
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_RUN_EMAIL}" \
    --role="roles/cloudsql.client" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_RUN_EMAIL}" \
    --role="roles/secretmanager.secretAccessor" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_RUN_EMAIL}" \
    --role="roles/storage.objectUser" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_RUN_EMAIL}" \
    --role="roles/pubsub.publisher" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

# Cloud Functions SA roles
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_FUNCTIONS_EMAIL}" \
    --role="roles/cloudsql.client" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_FUNCTIONS_EMAIL}" \
    --role="roles/secretmanager.secretAccessor" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_FUNCTIONS_EMAIL}" \
    --role="roles/storage.objectAdmin" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_FUNCTIONS_EMAIL}" \
    --role="roles/pubsub.subscriber" \
    --condition=None \
    --quiet > /dev/null 2>&1 || echo "  IAM binding may already exist"

echo "✅ IAM roles granted"
echo ""

# Create Cloud SQL Instance
if [ -n "$CLOUD_SQL_PASSWORD" ]; then
    echo "Creating Cloud SQL instance..."
    SQL_INSTANCE="${PROJECT_NAME}-${ENVIRONMENT}-db"
    if ! resource_exists "sql-instance" "$SQL_INSTANCE"; then
        echo "Creating: $SQL_INSTANCE"
        gcloud sql instances create "$SQL_INSTANCE" \
            --database-version=POSTGRES_15 \
            --tier="$CLOUD_SQL_TIER" \
            --region="$REGION" \
            --root-password="$CLOUD_SQL_PASSWORD" \
            --storage-type=SSD \
            --storage-size="${CLOUD_SQL_DISK_SIZE}GB" \
            --storage-auto-increase \
            --backup-start-time=03:00 \
            --enable-bin-log \
            --database-flags=max_connections=100 \
            --project="$PROJECT_ID" \
            --quiet > /dev/null 2>&1
        
        if [ $? -eq 0 ]; then
            echo "  ✅ Created"
            
            # Create database
            echo "Creating database: $CLOUD_SQL_DB_NAME"
            gcloud sql databases create "$CLOUD_SQL_DB_NAME" \
                --instance="$SQL_INSTANCE" \
                --project="$PROJECT_ID" \
                --quiet > /dev/null 2>&1 || echo "  Database may already exist"
            
            # Create user (if not exists)
            echo "Creating user: $CLOUD_SQL_USER"
            gcloud sql users create "$CLOUD_SQL_USER" \
                --instance="$SQL_INSTANCE" \
                --password="$CLOUD_SQL_PASSWORD" \
                --project="$PROJECT_ID" \
                --quiet > /dev/null 2>&1 || echo "  User may already exist"
        else
            echo "  ❌ Failed"
        fi
    else
        echo "  ⏭️  $SQL_INSTANCE already exists"
    fi
    echo ""
fi

# Get Cloud SQL connection info
SQL_INSTANCE="${PROJECT_NAME}-${ENVIRONMENT}-db"
SQL_PUBLIC_IP=$(gcloud sql instances describe "$SQL_INSTANCE" --project="$PROJECT_ID" --format="value(ipAddresses[0].ipAddress)" 2>/dev/null || echo "")
SQL_PRIVATE_IP=$(gcloud sql instances describe "$SQL_INSTANCE" --project="$PROJECT_ID" --format="value(ipAddresses[?type=='PRIVATE'].ipAddress)" 2>/dev/null || echo "")

# Create Storage Buckets
echo "Creating storage buckets..."
echo ""

# Documents bucket
BUCKET_DOCS="${PROJECT_NAME}-${ENVIRONMENT}-documents"
if ! resource_exists "storage-bucket" "$BUCKET_DOCS"; then
    echo "Creating: $BUCKET_DOCS"
    gsutil mb -p "$PROJECT_ID" -c STANDARD -l "$CLOUD_STORAGE_LOCATION" "gs://$BUCKET_DOCS" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "  ✅ Created"
        # Enable versioning
        gsutil versioning set on "gs://$BUCKET_DOCS" > /dev/null 2>&1
        # Set uniform bucket-level access
        gsutil uniformbucketlevelaccess set on "gs://$BUCKET_DOCS" > /dev/null 2>&1
    else
        echo "  ❌ Failed"
    fi
else
    echo "  ⏭️  $BUCKET_DOCS already exists"
fi

# Functions source bucket
BUCKET_FUNCTIONS="${PROJECT_NAME}-${ENVIRONMENT}-functions-source"
if ! resource_exists "storage-bucket" "$BUCKET_FUNCTIONS"; then
    echo "Creating: $BUCKET_FUNCTIONS"
    gsutil mb -p "$PROJECT_ID" -c STANDARD -l "$CLOUD_STORAGE_LOCATION" "gs://$BUCKET_FUNCTIONS" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "  ✅ Created"
        gsutil uniformbucketlevelaccess set on "gs://$BUCKET_FUNCTIONS" > /dev/null 2>&1
    else
        echo "  ❌ Failed"
    fi
else
    echo "  ⏭️  $BUCKET_FUNCTIONS already exists"
fi
echo ""

# Create Pub/Sub Topics
echo "Creating Pub/Sub topics..."
TOPICS=(
    "${PROJECT_NAME}-${ENVIRONMENT}-email"
    "${PROJECT_NAME}-${ENVIRONMENT}-ai-generation"
    "${PROJECT_NAME}-${ENVIRONMENT}-export"
)

for TOPIC in "${TOPICS[@]}"; do
    if ! resource_exists "pubsub-topic" "$TOPIC"; then
        echo "Creating: $TOPIC"
        gcloud pubsub topics create "$TOPIC" --project="$PROJECT_ID" > /dev/null 2>&1
        if [ $? -eq 0 ]; then
            echo "  ✅ Created"
        else
            echo "  ❌ Failed"
        fi
    else
        echo "  ⏭️  $TOPIC already exists"
    fi
done
echo ""

# Create Pub/Sub Subscriptions
echo "Creating Pub/Sub subscriptions..."
SUBSCRIPTIONS=(
    "${PROJECT_NAME}-${ENVIRONMENT}-email-sub:${PROJECT_NAME}-${ENVIRONMENT}-email"
    "${PROJECT_NAME}-${ENVIRONMENT}-ai-generation-sub:${PROJECT_NAME}-${ENVIRONMENT}-ai-generation"
    "${PROJECT_NAME}-${ENVIRONMENT}-export-sub:${PROJECT_NAME}-${ENVIRONMENT}-export"
)

for SUB_SPEC in "${SUBSCRIPTIONS[@]}"; do
    SUB_NAME="${SUB_SPEC%%:*}"
    TOPIC_NAME="${SUB_SPEC#*:}"
    if ! resource_exists "pubsub-subscription" "$SUB_NAME"; then
        echo "Creating: $SUB_NAME"
        gcloud pubsub subscriptions create "$SUB_NAME" \
            --topic="$TOPIC_NAME" \
            --ack-deadline=60 \
            --project="$PROJECT_ID" > /dev/null 2>&1
        if [ $? -eq 0 ]; then
            echo "  ✅ Created"
        else
            echo "  ❌ Failed"
        fi
    else
        echo "  ⏭️  $SUB_NAME already exists"
    fi
done
echo ""

# Create Artifact Registry Repository
echo "Creating Artifact Registry repository..."
REPO_NAME="${PROJECT_NAME}-${ENVIRONMENT}-repo"
if ! resource_exists "artifact-repo" "$REPO_NAME"; then
    echo "Creating: $REPO_NAME"
    gcloud artifacts repositories create "$REPO_NAME" \
        --repository-format=docker \
        --location="$REGION" \
        --description="Container repository for ${PROJECT_NAME} ${ENVIRONMENT}" \
        --project="$PROJECT_ID" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "  ✅ Created"
    else
        echo "  ❌ Failed"
    fi
else
    echo "  ⏭️  $REPO_NAME already exists"
fi
echo ""

# Create Secret Manager Secret
echo "Creating Secret Manager secret..."
SECRET_NAME="${PROJECT_NAME}-${ENVIRONMENT}-db-connection"
if ! resource_exists "secret" "$SECRET_NAME"; then
    echo "Creating: $SECRET_NAME"
    gcloud secrets create "$SECRET_NAME" \
        --replication-policy="automatic" \
        --project="$PROJECT_ID" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "  ✅ Created"
        # Add secret version if we have connection info
        if [ -n "$SQL_PUBLIC_IP" ] && [ -n "$CLOUD_SQL_PASSWORD" ]; then
            CONNECTION_STRING="Host=${SQL_PUBLIC_IP};Port=5432;Database=${CLOUD_SQL_DB_NAME};Username=${CLOUD_SQL_USER};Password=${CLOUD_SQL_PASSWORD}"
            echo -n "$CONNECTION_STRING" | gcloud secrets versions add "$SECRET_NAME" \
                --data-file=- \
                --project="$PROJECT_ID" > /dev/null 2>&1 || echo "  Failed to add secret version"
        fi
    else
        echo "  ❌ Failed"
    fi
else
    echo "  ⏭️  $SECRET_NAME already exists"
fi
echo ""

# Create Cloud Run Service
echo "Creating Cloud Run service..."
SERVICE_NAME="${PROJECT_NAME}-${ENVIRONMENT}-api"
IMAGE_URL="${REGION}-docker.pkg.dev/${PROJECT_ID}/${REPO_NAME}/api:latest"

if ! resource_exists "cloud-run" "$SERVICE_NAME"; then
    echo "Creating: $SERVICE_NAME"
    
    # Build environment variables
    ENV_VARS=(
        "ASPNETCORE_ENVIRONMENT=Production"
        "CloudProvider=GCP"
        "GCP__ProjectId=${PROJECT_ID}"
        "ConnectionStrings__SqordiaDb=Host=${SQL_PUBLIC_IP};Port=5432;Database=${CLOUD_SQL_DB_NAME};Username=${CLOUD_SQL_USER};Password=${CLOUD_SQL_PASSWORD};SSL Mode=Require;Trust Server Certificate=true"
        "CloudStorage__BucketName=${BUCKET_DOCS}"
        "PubSub__EmailTopic=${PROJECT_NAME}-${ENVIRONMENT}-email"
        "PubSub__AIGenerationTopic=${PROJECT_NAME}-${ENVIRONMENT}-ai-generation"
        "PubSub__ExportTopic=${PROJECT_NAME}-${ENVIRONMENT}-export"
    )
    
    # Build environment variables string
    ENV_VAR_STRING="ASPNETCORE_ENVIRONMENT=Production,CloudProvider=GCP,GCP__ProjectId=${PROJECT_ID},ConnectionStrings__SqordiaDb=Host=${SQL_PUBLIC_IP};Port=5432;Database=${CLOUD_SQL_DB_NAME};Username=${CLOUD_SQL_USER};Password=${CLOUD_SQL_PASSWORD};SSL Mode=Require;Trust Server Certificate=true,CloudStorage__BucketName=${BUCKET_DOCS},PubSub__EmailTopic=${PROJECT_NAME}-${ENVIRONMENT}-email,PubSub__AIGenerationTopic=${PROJECT_NAME}-${ENVIRONMENT}-ai-generation,PubSub__ExportTopic=${PROJECT_NAME}-${ENVIRONMENT}-export"
    
    gcloud run deploy "$SERVICE_NAME" \
        --image="$IMAGE_URL" \
        --region="$REGION" \
        --platform=managed \
        --service-account="${SA_RUN_EMAIL}" \
        --cpu="$CLOUD_RUN_CPU" \
        --memory="$CLOUD_RUN_MEMORY" \
        --min-instances="$CLOUD_RUN_MIN_INSTANCES" \
        --max-instances="$CLOUD_RUN_MAX_INSTANCES" \
        --timeout=300 \
        --allow-unauthenticated \
        --set-env-vars="$ENV_VAR_STRING" \
        --project="$PROJECT_ID" \
        --quiet > /dev/null 2>&1
    
    if [ $? -eq 0 ]; then
        echo "  ✅ Created"
    else
        echo "  ❌ Failed"
    fi
else
    echo "  ⏭️  $SERVICE_NAME already exists, updating..."
    # Update existing service
    ENV_VAR_STRING="ASPNETCORE_ENVIRONMENT=Production,CloudProvider=GCP,GCP__ProjectId=${PROJECT_ID},ConnectionStrings__SqordiaDb=Host=${SQL_PUBLIC_IP};Port=5432;Database=${CLOUD_SQL_DB_NAME};Username=${CLOUD_SQL_USER};Password=${CLOUD_SQL_PASSWORD};SSL Mode=Require;Trust Server Certificate=true,CloudStorage__BucketName=${BUCKET_DOCS},PubSub__EmailTopic=${PROJECT_NAME}-${ENVIRONMENT}-email,PubSub__AIGenerationTopic=${PROJECT_NAME}-${ENVIRONMENT}-ai-generation,PubSub__ExportTopic=${PROJECT_NAME}-${ENVIRONMENT}-export"
    
    gcloud run services update "$SERVICE_NAME" \
        --image="$IMAGE_URL" \
        --region="$REGION" \
        --update-env-vars="$ENV_VAR_STRING" \
        --project="$PROJECT_ID" \
        --quiet > /dev/null 2>&1
    
    if [ $? -eq 0 ]; then
        echo "  ✅ Updated"
    else
        echo "  ❌ Failed to update"
    fi
fi

# Deploy Cloud Functions (if zip files exist)
echo "Deploying Cloud Functions..."
echo ""

# Email Handler
FUNC_NAME="${PROJECT_NAME}-${ENVIRONMENT}-email-handler"
# Try both naming conventions
FUNC_ZIP="emailhandler-handler.zip"
if ! gsutil ls "gs://${BUCKET_FUNCTIONS}/${FUNC_ZIP}" > /dev/null 2>&1; then
    FUNC_ZIP="email-handler.zip"
fi
ZIP_PATH="gs://${BUCKET_FUNCTIONS}/${FUNC_ZIP}"
TOPIC="${PROJECT_NAME}-${ENVIRONMENT}-email"

if gsutil ls "$ZIP_PATH" > /dev/null 2>&1; then
    echo "Deploying: $FUNC_NAME"
    CONN_STRING="Host=${SQL_PUBLIC_IP};Port=5432;Database=${CLOUD_SQL_DB_NAME};Username=${CLOUD_SQL_USER};Password=${CLOUD_SQL_PASSWORD};SSL Mode=Require;Trust Server Certificate=true"
    ENV_VARS="ASPNETCORE_ENVIRONMENT=Production,ConnectionStrings__SqordiaDb=${CONN_STRING}"
    
    gcloud functions deploy "$FUNC_NAME" \
        --gen2 \
        --runtime=dotnet8 \
        --region="$REGION" \
        --source="$ZIP_PATH" \
        --entry-point="Sqordia.Functions.EmailHandler.Function" \
        --trigger-topic="$TOPIC" \
        --service-account="${SA_FUNCTIONS_EMAIL}" \
        --memory=512MB \
        --timeout=300s \
        --max-instances=10 \
        --min-instances=0 \
        --set-env-vars="$ENV_VARS" \
        --project="$PROJECT_ID" \
        --quiet 2>&1 | grep -v "already exists" && echo "  ✅ Deployed" || echo "  ⏭️  Function may already exist"
else
    echo "  ⏭️  $FUNC_NAME skipped (zip file not found)"
fi

# AI Generation Handler
FUNC_NAME="${PROJECT_NAME}-${ENVIRONMENT}-ai-generation-handler"
# Try both naming conventions
FUNC_ZIP="aigenerationhandler-handler.zip"
if ! gsutil ls "gs://${BUCKET_FUNCTIONS}/${FUNC_ZIP}" > /dev/null 2>&1; then
    FUNC_ZIP="ai-generation-handler.zip"
fi
ZIP_PATH="gs://${BUCKET_FUNCTIONS}/${FUNC_ZIP}"
TOPIC="${PROJECT_NAME}-${ENVIRONMENT}-ai-generation"

if gsutil ls "$ZIP_PATH" > /dev/null 2>&1; then
    echo "Deploying: $FUNC_NAME"
    CONN_STRING="Host=${SQL_PUBLIC_IP};Port=5432;Database=${CLOUD_SQL_DB_NAME};Username=${CLOUD_SQL_USER};Password=${CLOUD_SQL_PASSWORD};SSL Mode=Require;Trust Server Certificate=true"
    ENV_VARS="ASPNETCORE_ENVIRONMENT=Production,ConnectionStrings__SqordiaDb=${CONN_STRING}"
    
    gcloud functions deploy "$FUNC_NAME" \
        --gen2 \
        --runtime=dotnet8 \
        --region="$REGION" \
        --source="$ZIP_PATH" \
        --entry-point="Sqordia.Functions.AIGenerationHandler.Function" \
        --trigger-topic="$TOPIC" \
        --service-account="${SA_FUNCTIONS_EMAIL}" \
        --memory=512MB \
        --timeout=300s \
        --max-instances=10 \
        --min-instances=0 \
        --set-env-vars="$ENV_VARS" \
        --project="$PROJECT_ID" \
        --quiet 2>&1 | grep -v "already exists" && echo "  ✅ Deployed" || echo "  ⏭️  Function may already exist"
else
    echo "  ⏭️  $FUNC_NAME skipped (zip file not found)"
fi

# Export Handler
FUNC_NAME="${PROJECT_NAME}-${ENVIRONMENT}-export-handler"
# Try both naming conventions
FUNC_ZIP="exporthandler-handler.zip"
if ! gsutil ls "gs://${BUCKET_FUNCTIONS}/${FUNC_ZIP}" > /dev/null 2>&1; then
    FUNC_ZIP="export-handler.zip"
fi
ZIP_PATH="gs://${BUCKET_FUNCTIONS}/${FUNC_ZIP}"
TOPIC="${PROJECT_NAME}-${ENVIRONMENT}-export"

if gsutil ls "$ZIP_PATH" > /dev/null 2>&1; then
    echo "Deploying: $FUNC_NAME"
    CONN_STRING="Host=${SQL_PRIVATE_IP};Port=5432;Database=${CLOUD_SQL_DB_NAME};Username=${CLOUD_SQL_USER};Password=${CLOUD_SQL_PASSWORD}"
    ENV_VARS="ASPNETCORE_ENVIRONMENT=Production,ConnectionStrings__SqordiaDb=${CONN_STRING},CloudStorage__BucketName=${BUCKET_DOCS}"
    
    gcloud functions deploy "$FUNC_NAME" \
        --gen2 \
        --runtime=dotnet8 \
        --region="$REGION" \
        --source="$ZIP_PATH" \
        --entry-point="Sqordia.Functions.ExportHandler.Function" \
        --trigger-topic="$TOPIC" \
        --service-account="${SA_FUNCTIONS_EMAIL}" \
        --memory=512MB \
        --timeout=300s \
        --max-instances=10 \
        --min-instances=0 \
        --set-env-vars="$ENV_VARS" \
        --project="$PROJECT_ID" \
        --quiet 2>&1 | grep -v "already exists" && echo "  ✅ Deployed" || echo "  ⏭️  Function may already exist"
else
    echo "  ⏭️  $FUNC_NAME skipped (zip file not found)"
fi
echo ""

# Get Cloud Run URL
SERVICE_URL=$(gcloud run services describe "$SERVICE_NAME" --region="$REGION" --project="$PROJECT_ID" --format="value(status.url)" 2>/dev/null || echo "")
echo ""
echo "=== Deployment Summary ==="
echo "Cloud Run URL: $SERVICE_URL"
echo ""
echo "✅ Infrastructure deployment completed!"
echo ""

