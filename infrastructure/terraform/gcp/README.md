# GCP Terraform Infrastructure

This directory contains Terraform configuration for deploying Sqordia to Google Cloud Platform.

## Architecture

- **Cloud Run**: API container service (replaces ECS Fargate)
- **Cloud SQL**: PostgreSQL database (replaces RDS)
- **Cloud Storage**: Object storage for documents (replaces S3)
- **Cloud Pub/Sub**: Message queuing (replaces SQS)
- **Cloud Functions**: Serverless functions for background jobs (replaces Lambda)
- **Secret Manager**: Secrets management (replaces AWS Secrets Manager)
- **Artifact Registry**: Container registry (replaces ECR)

## Cost Optimization

This configuration is optimized for cost:
- **Skip Cloud Load Balancing**: Use Cloud Run default URL (saves $18/month)
- **Skip Cloud DNS**: Use Cloud Run default URL (saves $0.20/month)
- **Cloud SQL**: Uses db-f1-micro (free tier) or db-shared-core (cost optimization)
- **Cloud SQL Storage**: 10GB default (can be increased if needed)
- **Cloud Logging**: 3-day retention (reduces cost)
- **Cloud Run**: Minimum instances = 0 (pay-per-use)

**Estimated Monthly Cost:**
- First 12 months: ~$12-15/month
- After 12 months: ~$19-25/month (with optimizations)

## Prerequisites

1. **GCP Project**: Create a GCP project and enable billing
2. **gcloud CLI**: Install and authenticate
   ```bash
   gcloud auth login
   gcloud auth application-default login
   ```
3. **Terraform**: Install Terraform >= 1.0
4. **APIs Enabled**: Run `scripts/setup-gcp-apis.ps1` to enable required APIs
5. **Service Account**: Run `scripts/create-github-actions-sa.ps1` for CI/CD

## Setup

1. **Copy example variables file:**
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

2. **Edit terraform.tfvars:**
   - Set `gcp_project_id`
   - Set `gcp_region`
   - Set `cloud_sql_password` (strong password)
   - Adjust other variables as needed

3. **Initialize Terraform:**
   ```bash
   terraform init
   ```

4. **Plan deployment:**
   ```bash
   terraform plan
   ```

5. **Apply configuration:**
   ```bash
   terraform apply
   ```

## Deployment

### Build and Push Container Image

1. **Authenticate with Artifact Registry:**
   ```bash
   gcloud auth configure-docker us-central1-docker.pkg.dev
   ```

2. **Build Docker image:**
   ```bash
   docker build -t us-central1-docker.pkg.dev/PROJECT_ID/REPO_NAME/api:latest .
   ```

3. **Push to Artifact Registry:**
   ```bash
   docker push us-central1-docker.pkg.dev/PROJECT_ID/REPO_NAME/api:latest
   ```

### Deploy Cloud Functions

1. **Build function packages:**
   ```bash
   # Build email handler
   dotnet publish src/Lambda/EmailHandler/EmailHandler.csproj -c Release -o ./publish
   cd publish
   zip -r email-handler.zip .
   
   # Repeat for ai-generation-handler and export-handler
   ```

2. **Upload to Cloud Storage:**
   ```bash
   gsutil cp email-handler.zip gs://sqordia-production-functions-source/
   ```

3. **Terraform will deploy the functions automatically**

## Configuration

### Cloud SQL

- **Tier**: `db-f1-micro` (free tier) or `db-shared-core` (cost optimization)
- **Storage**: 10GB default (configurable)
- **Backups**: Enabled with 7-day retention
- **Access**: Private IP recommended (use Cloud SQL Proxy for local access)

### Cloud Run

- **CPU**: 0.5 vCPU (configurable)
- **Memory**: 1GB (configurable)
- **Min Instances**: 0 (pay-per-use) or 1 (avoid cold starts)
- **Max Instances**: 10 (configurable)
- **Timeout**: 300 seconds (5 minutes)

### Cloud Functions

- **Memory**: 512MB (configurable)
- **Timeout**: 300 seconds (5 minutes)
- **Trigger**: Pub/Sub topics

## Outputs

After deployment, Terraform will output:
- Cloud Run service URL
- Cloud SQL connection details
- Cloud Storage bucket name
- Pub/Sub topic names
- Cloud Function names

## Accessing Resources

### Cloud SQL

Use Cloud SQL Proxy for local access:
```bash
cloud-sql-proxy PROJECT_ID:REGION:INSTANCE_NAME
```

### Cloud Run

Access via the service URL:
```bash
curl https://sqordia-production-api-xxxxx.run.app
```

## Cost Monitoring

1. **Set up billing alerts** in GCP Console
2. **Monitor usage** via Cloud Billing dashboard
3. **Review costs** monthly and adjust resources as needed

## Troubleshooting

### Cloud Run not starting
- Check container image is pushed to Artifact Registry
- Verify service account has required permissions
- Check Cloud Run logs

### Cloud Functions not triggering
- Verify Pub/Sub subscription exists
- Check function logs
- Verify function has required permissions

### Cloud SQL connection issues
- Use Cloud SQL Proxy for local access
- Verify VPC connector is configured (if using private IP)
- Check firewall rules

## Cleanup

To destroy all resources:
```bash
terraform destroy
```

**Warning**: This will delete all resources including the database!

## Next Steps

1. Set up CI/CD pipeline (GitHub Actions)
2. Configure monitoring and alerting
3. Set up backup strategy
4. Configure custom domain (optional)
5. Set up VPC connector for private Cloud SQL access (recommended)

