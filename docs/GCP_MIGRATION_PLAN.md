# GCP Migration Plan - Implementation Status

## Overview

This document outlines the GCP migration plan for Sqordia, including what has been implemented and next steps.

## ✅ Completed: Phase 1 - Infrastructure as Code

### Terraform Configuration Created

All GCP Terraform configuration files have been created in `infrastructure/terraform/gcp/`:

1. **main.tf** - Provider configuration
2. **variables.tf** - All configuration variables
3. **outputs.tf** - Resource outputs
4. **cloud_sql.tf** - PostgreSQL database configuration
5. **cloud_run.tf** - API container service configuration
6. **cloud_storage.tf** - Object storage for documents
7. **pubsub.tf** - Message queuing topics and subscriptions
8. **cloud_functions.tf** - Serverless functions (email, AI, export handlers)
9. **secret_manager.tf** - Secrets management
10. **cloud_logging.tf** - Logging configuration
11. **iam.tf** - IAM roles and service accounts
12. **terraform.tfvars.example** - Example configuration file
13. **README.md** - Comprehensive documentation

### Deployment Scripts Created

1. **scripts/deploy-gcp.ps1** - Terraform deployment automation
2. **scripts/build-and-push-gcp.ps1** - Docker image build and push

### Existing Scripts (Already Created)

1. **scripts/phase1-quick-start.ps1** - Quick setup automation
2. **scripts/setup-gcp-apis.ps1** - API enablement
3. **scripts/create-github-actions-sa.ps1** - Service account creation
4. **scripts/verify-gcp-setup.ps1** - Setup verification

## 📋 Next Steps: Phase 2 - Initial Setup

### Step 1: GCP Project Setup

1. **Create GCP Project:**
   ```bash
   gcloud projects create sqordia-production
   gcloud config set project sqordia-production
   ```

2. **Enable Billing:**
   - Go to [GCP Console → Billing](https://console.cloud.google.com/billing)
   - Link billing account to project

3. **Run Quick Start Script:**
   ```powershell
   .\scripts\phase1-quick-start.ps1 -ProjectId "sqordia-production" -Region "us-central1"
   ```

### Step 2: Configure Terraform

1. **Copy example variables:**
   ```bash
   cd infrastructure/terraform/gcp
   cp terraform.tfvars.example terraform.tfvars
   ```

2. **Edit terraform.tfvars:**
   - Set `gcp_project_id = "sqordia-production"`
   - Set `gcp_region = "us-central1"`
   - Set `cloud_sql_password` (strong password)
   - Review and adjust other variables

3. **Initialize Terraform:**
   ```bash
   terraform init
   ```

### Step 3: Deploy Infrastructure

1. **Plan deployment:**
   ```powershell
   .\scripts\deploy-gcp.ps1 -Action plan
   ```

2. **Review the plan** and verify resources

3. **Apply configuration:**
   ```powershell
   .\scripts\deploy-gcp.ps1 -Action apply
   ```

### Step 4: Build and Push Container Image

1. **Build and push API image:**
   ```powershell
   .\scripts\build-and-push-gcp.ps1 -ProjectId "sqordia-production" -Region "us-central1"
   ```

### Step 5: Deploy Cloud Functions

1. **Build function packages:**
   ```bash
   # Email handler
   cd src/Lambda/EmailHandler
   dotnet publish -c Release -o ./publish
   cd publish
   Compress-Archive -Path * -DestinationPath ../../../infrastructure/terraform/gcp/email-handler.zip
   
   # Repeat for ai-generation-handler and export-handler
   ```

2. **Upload to Cloud Storage:**
   ```bash
   gsutil cp email-handler.zip gs://sqordia-production-functions-source/
   gsutil cp ai-generation-handler.zip gs://sqordia-production-functions-source/
   gsutil cp export-handler.zip gs://sqordia-production-functions-source/
   ```

3. **Update Terraform** to reference the uploaded files (or use gsutil in Terraform)

## 📋 Phase 3: Application Code Updates

### Required Code Changes

1. **Update Connection Strings:**
   - Replace AWS-specific code with GCP equivalents
   - Update SQS → Pub/Sub integration
   - Update S3 → Cloud Storage integration
   - Update Secrets Manager → Secret Manager integration

2. **Update Lambda Handlers:**
   - Adapt for Cloud Functions v2
   - Update Pub/Sub event handling
   - Test function deployments

3. **Update API Configuration:**
   - Ensure Cloud Run environment variables are correct
   - Update database connection to use Cloud SQL Proxy or private IP
   - Test API deployment

### Code Files to Update

1. **S3 Integration** → Cloud Storage:
   - `src/Infrastructure/Sqordia.Infrastructure/` - Storage service
   - Update to use Google.Cloud.Storage library

2. **SQS Integration** → Pub/Sub:
   - `src/Infrastructure/Sqordia.Infrastructure/` - Queue service
   - Update to use Google.Cloud.PubSub.V1 library

3. **Secrets Manager** → Secret Manager:
   - Update to use Google.Cloud.SecretManager.V1 library

4. **Lambda Handlers** → Cloud Functions:
   - `src/Lambda/EmailHandler/`
   - `src/Lambda/AIGenerationHandler/`
   - `src/Lambda/ExportHandler/`
   - Update to Cloud Functions v2 event format

## 📋 Phase 4: Database Migration

### Option 1: Fresh Start (Recommended for Development)
- Create new database in Cloud SQL
- Run migrations
- Seed initial data

### Option 2: Data Migration (For Production)
1. **Export from AWS RDS:**
   ```bash
   pg_dump -h RDS_ENDPOINT -U postgres -d SqordiaDb > backup.sql
   ```

2. **Import to Cloud SQL:**
   ```bash
   psql -h CLOUD_SQL_IP -U postgres -d SqordiaDb < backup.sql
   ```

## 📋 Phase 5: Testing and Validation

1. **Test API endpoints:**
   - Verify Cloud Run service is accessible
   - Test authentication
   - Test database connections

2. **Test Cloud Functions:**
   - Trigger email handler
   - Trigger AI generation handler
   - Trigger export handler

3. **Test integrations:**
   - Pub/Sub message publishing
   - Cloud Storage file uploads
   - Secret Manager access

## 📋 Phase 6: CI/CD Setup

### ✅ Completed: Secrets and Service Accounts Configuration

- **Service Account Created**: `github-actions@[PROJECT_ID].iam.gserviceaccount.com`
- **Workload Identity Federation**: Configured via `scripts/setup-workload-identity.ps1`
- **GitHub Secrets Configured**: All required secrets added to repository
  - `GCP_PROJECT_ID`
  - `GCP_REGION`
  - `WORKLOAD_IDENTITY_POOL`
  - `WORKLOAD_IDENTITY_PROVIDER`
  - `GCP_SERVICE_ACCOUNT`
- **Documentation**: See [GCP GitHub Actions Workload Identity](./GCP_GITHUB_ACTIONS_WORKLOAD_IDENTITY.md)

### GitHub Actions Workflow

Create `.github/workflows/deploy-gcp.yml`:

```yaml
name: Deploy to GCP

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - uses: google-github-actions/setup-gcloud@v1
        with:
          service_account_key: ${{ secrets.GCP_SA_KEY }}
          project_id: ${{ secrets.GCP_PROJECT_ID }}
      
      - name: Authenticate Docker
        run: gcloud auth configure-docker us-central1-docker.pkg.dev
      
      - name: Build and Push
        run: |
          docker build -t us-central1-docker.pkg.dev/${{ secrets.GCP_PROJECT_ID }}/sqordia-production-repo/api:${{ github.sha }} .
          docker push us-central1-docker.pkg.dev/${{ secrets.GCP_PROJECT_ID }}/sqordia-production-repo/api:${{ github.sha }}
      
      - name: Deploy to Cloud Run
        run: |
          gcloud run deploy sqordia-production-api \
            --image us-central1-docker.pkg.dev/${{ secrets.GCP_PROJECT_ID }}/sqordia-production-repo/api:${{ github.sha }} \
            --region us-central1 \
            --platform managed
```

## 📊 Cost Monitoring

1. **Set up billing alerts** in GCP Console
2. **Monitor usage** via Cloud Billing dashboard
3. **Review costs** monthly
4. **Optimize resources** as needed

## 🔒 Security Considerations

1. **Private Cloud SQL Access:**
   - Use VPC connector for private IP access
   - Use Cloud SQL Proxy for local development

2. **IAM Permissions:**
   - Use least privilege principle
   - Review service account permissions
   - Restrict Cloud Run public access if needed

3. **Secrets Management:**
   - Use Secret Manager for sensitive data
   - Rotate secrets regularly
   - Never commit secrets to version control

## 📝 Notes

- **Cloud Load Balancing:** Skipped for cost optimization (saves $18/month)
- **Cloud DNS:** Skipped initially (can be added later if needed)
- **Custom Domain:** Can be added later using Cloud Run domain mapping
- **VPC Connector:** Recommended for production (private Cloud SQL access)

## 🎯 Success Criteria

- [ ] All infrastructure deployed via Terraform
- [ ] API running on Cloud Run
- [ ] Database accessible and migrated
- [ ] Cloud Functions working
- [x] CI/CD secrets and service accounts configured
- [ ] CI/CD pipeline workflow created and tested
- [ ] Monitoring and alerts set up
- [ ] Cost within budget (~$12-15/month first year)

## 📚 Resources

- [GCP Cost Estimate](./GCP_COST_ESTIMATE.md)
- [GCP Cost Optimization](./GCP_COST_OPTIMIZATION.md)
- [GCP Service Alternatives](./GCP_SERVICE_ALTERNATIVES.md)
- [Phase 1 Setup Checklist](../scripts/phase1-gcp-setup-checklist.md)
- [GCP Terraform README](../infrastructure/terraform/gcp/README.md)

