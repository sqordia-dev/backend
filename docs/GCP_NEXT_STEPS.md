# GCP Migration - Next Steps Guide

## ✅ Current Status

- **GCP Project**: `project-b79ef08c-1eb8-47ea-80e` (sqordia-production) ✅
- **Terraform Configuration**: Created ✅
- **terraform.tfvars**: Created with your project ID ✅

## 🚀 Immediate Next Steps

### Step 1: Set Database Password ⚠️ REQUIRED

**Edit `infrastructure/terraform/gcp/terraform.tfvars`** and replace `CHANGE_ME` with a strong password:

```hcl
cloud_sql_password = "YourStrongPasswordHere123!"
```

**Important:** 
- Use a strong password (at least 16 characters, mix of letters, numbers, symbols)
- This file is already in `.gitignore` so it won't be committed

### Step 2: Enable GCP APIs ⚠️ REQUIRED

Run the quick start script to enable all required APIs:

```powershell
.\scripts\phase1-quick-start.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e" -Region "northamerica-northeast2"
```

This will:
- Enable Cloud Run API
- Enable Cloud SQL Admin API
- Enable Cloud Storage API
- Enable Pub/Sub API
- Enable Cloud Functions API
- Enable Secret Manager API
- Enable Artifact Registry API
- Enable Cloud Logging API
- Create GitHub Actions service account (for CI/CD)

**Alternative (manual):**
```powershell
.\scripts\setup-gcp-apis.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e" -Region "northamerica-northeast2"
```

### Step 3: Verify Setup

Check that everything is configured correctly:

```powershell
.\scripts\verify-gcp-setup.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e"
```

### Step 4: Review Terraform Plan

Before deploying, review what will be created:

```powershell
.\scripts\deploy-gcp.ps1 -Action plan
```

This will show you:
- Cloud SQL PostgreSQL instance (db-f1-micro, 10GB)
- Cloud Run service (API container)
- Cloud Storage bucket (for documents)
- Pub/Sub topics (email, AI generation, export)
- Cloud Functions (email handler, AI handler, export handler)
- Secret Manager (for database connection string)
- Artifact Registry (for container images)
- IAM roles and service accounts

**Review the output carefully** to ensure everything looks correct.

### Step 5: Deploy Infrastructure

Once you're satisfied with the plan, deploy:

```powershell
.\scripts\deploy-gcp.ps1 -Action apply
```

This will:
- Create all GCP resources
- Take approximately 5-10 minutes
- Output connection details at the end

**Note:** You'll be prompted to confirm before applying.

### Step 6: Build and Push Container Image

After infrastructure is deployed, build and push your API Docker image:

```powershell
.\scripts\build-and-push-gcp.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e" -Region "us-central1"
```

This will:
- Authenticate Docker with Artifact Registry
- Build the Docker image from your Dockerfile
- Push the image to Artifact Registry
- Tag it as `latest`

### Step 7: Update Cloud Run Service

After pushing the image, update Cloud Run to use it:

```powershell
cd infrastructure\terraform\gcp
terraform apply
```

Or manually update the Cloud Run service to use the new image.

### Step 8: Build and Deploy Cloud Functions

Build the Lambda function packages:

```powershell
# Email handler
cd src\Lambda\EmailHandler
dotnet publish -c Release -o .\publish
cd publish
Compress-Archive -Path * -DestinationPath ..\..\..\..\infrastructure\terraform\gcp\email-handler.zip

# AI Generation handler
cd ..\..\AIGenerationHandler
dotnet publish -c Release -o .\publish
cd publish
Compress-Archive -Path * -DestinationPath ..\..\..\..\infrastructure\terraform\gcp\ai-generation-handler.zip

# Export handler
cd ..\..\ExportHandler
dotnet publish -c Release -o .\publish
cd publish
Compress-Archive -Path * -DestinationPath ..\..\..\..\infrastructure\terraform\gcp\export-handler.zip
```

Then update Terraform to deploy the functions (they should reference the zip files).

## 📊 Expected Costs

- **First 12 months**: ~$12-15/month ✅ (within $20/month budget)
- **After 12 months**: ~$19-25/month ⚠️ (slightly over budget but optimized)

## 🔍 View Deployment Outputs

After deployment, view all outputs:

```powershell
cd infrastructure\terraform\gcp
terraform output
```

This will show:
- Cloud Run service URL
- Cloud SQL connection details
- Cloud Storage bucket name
- Pub/Sub topic names
- Cloud Function names

## 🧪 Test the Deployment

1. **Test Cloud Run API:**
   ```powershell
   $url = terraform output -raw cloud_run_service_url
   curl $url/health
   ```

2. **Test Cloud SQL Connection:**
   - Use Cloud SQL Proxy for local access
   - Or connect via Cloud Console

3. **Test Cloud Functions:**
   - Publish a message to Pub/Sub topics
   - Check function logs in Cloud Console

## 📝 Important Notes

1. **terraform.tfvars** is already in `.gitignore` - it won't be committed
2. **Database password** must be set before deploying
3. **APIs must be enabled** before Terraform can create resources
4. **Container image** must be built and pushed before Cloud Run can start
5. **Cloud Functions** need to be built and packaged before deployment

## 🆘 Troubleshooting

### Terraform fails with "API not enabled"
- Run `.\scripts\setup-gcp-apis.ps1` to enable all APIs

### Cloud Run fails to start
- Check that container image is pushed to Artifact Registry
- Verify service account has required permissions
- Check Cloud Run logs in GCP Console

### Cloud SQL connection issues
- Use Cloud SQL Proxy for local access
- Verify firewall rules allow connections
- Check IAM permissions for service account

## 🎯 Quick Command Reference

```powershell
# Enable APIs
.\scripts\phase1-quick-start.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e" -Region "us-central1"

# Verify setup
.\scripts\verify-gcp-setup.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e"

# Plan deployment
.\scripts\deploy-gcp.ps1 -Action plan

# Deploy infrastructure
.\scripts\deploy-gcp.ps1 -Action apply

# Build and push image
.\scripts\build-and-push-gcp.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e" -Region "northamerica-northeast2"

# View outputs
cd infrastructure\terraform\gcp
terraform output
```

## 📚 Related Documentation

- [GCP Migration Plan](./GCP_MIGRATION_PLAN.md) - Full migration plan
- [GCP Migration Status](./GCP_MIGRATION_STATUS.md) - Current progress
- [GCP Cost Estimate](./GCP_COST_ESTIMATE.md) - Cost breakdown
- [GCP Terraform README](../infrastructure/terraform/gcp/README.md) - Terraform docs

