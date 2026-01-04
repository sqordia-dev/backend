# Phase 1: GCP Setup Checklist

## Quick Start (Automated)

For automated setup, run:
```powershell
.\scripts\phase1-quick-start.ps1 -ProjectId "sqordia-production" -Region "us-central1"
```

This will:
- Enable all required APIs
- Create GitHub Actions service account
- Grant necessary permissions
- Create service account key

## Manual Setup (Step-by-Step)

## 1.1 GCP Account Setup

### Step 1: Create GCP Project
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click "Select a project" → "New Project"
3. Project name: `sqordia-production`
4. Organization: (select your organization if applicable)
5. Location: (select your location)
6. Click "Create"

### Step 2: Enable Billing
1. Go to [Billing](https://console.cloud.google.com/billing)
2. Link billing account to `sqordia-production` project
3. Verify billing is enabled

### Step 3: Enable Required APIs
Run these commands or enable via Console:

```bash
# Set your project
gcloud config set project sqordia-production

# Enable required APIs
gcloud services enable run.googleapis.com
gcloud services enable sqladmin.googleapis.com
gcloud services enable storage.googleapis.com
gcloud services enable pubsub.googleapis.com
gcloud services enable cloudfunctions.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable artifactregistry.googleapis.com
gcloud services enable logging.googleapis.com
```

**Or enable via Console:**
1. Go to [APIs & Services](https://console.cloud.google.com/apis/library)
2. Search and enable each API:
   - ✅ Cloud Run API
   - ✅ Cloud SQL Admin API
   - ✅ Cloud Storage API
   - ✅ Cloud Pub/Sub API
   - ✅ Cloud Functions API
   - ✅ Secret Manager API
   - ✅ Artifact Registry API
   - ✅ Cloud Logging API

### Step 4: Create Service Account for GitHub Actions
```bash
# Create service account
gcloud iam service-accounts create github-actions \
  --display-name="GitHub Actions Service Account" \
  --project=sqordia-production

# Grant required permissions
gcloud projects add-iam-policy-binding sqordia-production \
  --member="serviceAccount:github-actions@sqordia-production.iam.gserviceaccount.com" \
  --role="roles/run.admin"

gcloud projects add-iam-policy-binding sqordia-production \
  --member="serviceAccount:github-actions@sqordia-production.iam.gserviceaccount.com" \
  --role="roles/artifactregistry.writer"

gcloud projects add-iam-policy-binding sqordia-production \
  --member="serviceAccount:github-actions@sqordia-production.iam.gserviceaccount.com" \
  --role="roles/cloudfunctions.admin"

gcloud projects add-iam-policy-binding sqordia-production \
  --member="serviceAccount:github-actions@sqordia-production.iam.gserviceaccount.com" \
  --role="roles/secretmanager.secretAccessor"

gcloud projects add-iam-policy-binding sqordia-production \
  --member="serviceAccount:github-actions@sqordia-production.iam.gserviceaccount.com" \
  --role="roles/cloudsql.client"

# Create and download service account key
gcloud iam service-accounts keys create github-actions-key.json \
  --iam-account=github-actions@sqordia-production.iam.gserviceaccount.com \
  --project=sqordia-production
```

### Step 5: Add GitHub Secrets
1. Go to your GitHub repository
2. Navigate to: Settings → Secrets and variables → Actions
3. Add the following secrets:
   - `GCP_PROJECT_ID`: `sqordia-production`
   - `GCP_SA_KEY`: (contents of `github-actions-key.json` file)
   - `GCP_REGION`: `us-central1` (or your preferred region)

## 1.2 Install GCP Tools

### Install Google Cloud SDK (gcloud)
**Windows:**
```powershell
# Using winget
winget install Google.CloudSDK

# Or download from:
# https://cloud.google.com/sdk/docs/install
```

**Verify installation:**
```bash
gcloud --version
gcloud init
gcloud auth login
gcloud auth application-default login
```

### Install Terraform
**Windows:**
```powershell
# Using winget
winget install HashiCorp.Terraform

# Or download from:
# https://www.terraform.io/downloads
```

**Verify installation:**
```bash
terraform version
```

## 1.3 Document Current Configuration

**Note:** No AWS data backup needed - starting fresh on GCP.

### Document for Reference:
- Current AWS configuration values (for reference only)
- Environment-specific settings
- Custom configurations that need to be replicated in GCP
- Any important notes about the current setup

