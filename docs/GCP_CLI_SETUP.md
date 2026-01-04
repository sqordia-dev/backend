# GCP CLI Setup Guide

This guide will help you install and configure the Google Cloud CLI (gcloud) for local development and deployment.

## Installation

### Option 1: Install Google Cloud SDK (Recommended)

1. **Download Google Cloud SDK:**
   - Visit: https://cloud.google.com/sdk/docs/install
   - Download the Windows installer
   - Or use PowerShell:

```powershell
# Download and install Google Cloud SDK
(New-Object Net.WebClient).DownloadFile("https://dl.google.com/dl/cloudsdk/channels/rapid/GoogleCloudSDKInstaller.exe", "$env:TEMP\GoogleCloudSDKInstaller.exe")
Start-Process -FilePath "$env:TEMP\GoogleCloudSDKInstaller.exe" -Wait
```

2. **Restart your terminal** after installation

3. **Verify installation:**
```powershell
gcloud --version
```

### Option 2: Use Chocolatey (if you have it)

```powershell
choco install gcloudsdk
```

## Authentication Setup

### Step 1: Login to GCP

```powershell
gcloud auth login
```

This will open a browser window for you to authenticate with your Google account.

### Step 2: Set Application Default Credentials

For local development, set up Application Default Credentials (ADC):

```powershell
gcloud auth application-default login
```

This allows your local application to use your credentials automatically.

### Step 3: Set Your Project

```powershell
gcloud config set project project-b79ef08c-1eb8-47ea-80e
```

Or use the project ID from your terraform.tfvars file.

### Step 4: Verify Configuration

```powershell
# Check current project
gcloud config get-value project

# List authenticated accounts
gcloud auth list

# Check ADC
gcloud auth application-default print-access-token
```

## Using GCP Services Locally

Once authenticated, your .NET application can use GCP services locally by:

1. **Setting CloudProvider to GCP:**
```powershell
$env:CloudProvider="GCP"
```

2. **Setting GCP Configuration:**
```powershell
$env:GCP__ProjectId="project-b79ef08c-1eb8-47ea-80e"
$env:CloudStorage__BucketName="sqordia-production-documents"
$env:PubSub__EmailTopic="sqordia-production-email"
```

3. **Running the application:**
```powershell
dotnet run --project src/WebAPI/WebAPI.csproj
```

The application will automatically use Application Default Credentials (ADC) to authenticate with GCP services.

## Service Account (For Production/CI/CD)

For production deployments or CI/CD, use a service account:

1. **Create a service account:**
```powershell
gcloud iam service-accounts create sqordia-app \
    --display-name="Sqordia Application Service Account"
```

2. **Grant necessary permissions:**
```powershell
gcloud projects add-iam-policy-binding project-b79ef08c-1eb8-47ea-80e \
    --member="serviceAccount:sqordia-app@project-b79ef08c-1eb8-47ea-80e.iam.gserviceaccount.com" \
    --role="roles/storage.objectAdmin"

gcloud projects add-iam-policy-binding project-b79ef08c-1eb8-47ea-80e \
    --member="serviceAccount:sqordia-app@project-b79ef08c-1eb8-47ea-80e.iam.gserviceaccount.com" \
    --role="roles/pubsub.publisher"
```

3. **Create and download key:**
```powershell
gcloud iam service-accounts keys create key.json \
    --iam-account=sqordia-app@project-b79ef08c-1eb8-47ea-80e.iam.gserviceaccount.com
```

4. **Set environment variable:**
```powershell
$env:GOOGLE_APPLICATION_CREDENTIALS="path\to\key.json"
```

## Quick Setup Script

Run this script to set up everything:

```powershell
# Set project
gcloud config set project project-b79ef08c-1eb8-47ea-80e

# Authenticate
gcloud auth login
gcloud auth application-default login

# Enable required APIs
gcloud services enable storage.googleapis.com
gcloud services enable pubsub.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable run.googleapis.com
gcloud services enable sqladmin.googleapis.com
gcloud services enable cloudfunctions.googleapis.com
gcloud services enable artifactregistry.googleapis.com
```

## Troubleshooting

### Issue: "gcloud: command not found"
- **Solution:** Restart your terminal after installation
- Or add to PATH manually: `C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin`

### Issue: "Application Default Credentials not found"
- **Solution:** Run `gcloud auth application-default login`

### Issue: "Permission denied"
- **Solution:** Check that your account has the necessary IAM roles
- Verify project is set correctly: `gcloud config get-value project`

### Issue: "Project not found"
- **Solution:** Verify project ID is correct
- List your projects: `gcloud projects list`

## Next Steps

After setting up the CLI:

1. ✅ Install gcloud CLI
2. ✅ Authenticate with `gcloud auth login`
3. ✅ Set up ADC with `gcloud auth application-default login`
4. ✅ Set project with `gcloud config set project`
5. ✅ Test locally with GCP services
6. ✅ Deploy to GCP using Terraform

## References

- [Google Cloud SDK Documentation](https://cloud.google.com/sdk/docs)
- [Application Default Credentials](https://cloud.google.com/docs/authentication/application-default-credentials)
- [gcloud CLI Reference](https://cloud.google.com/sdk/gcloud/reference)

