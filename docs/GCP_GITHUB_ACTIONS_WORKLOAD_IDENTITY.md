# GitHub Actions with Workload Identity Federation

Since service account key creation is disabled (organization policy), we'll use **Workload Identity Federation**, which is the recommended and more secure approach for GitHub Actions.

## ✅ Completion Checklist

- [x] **Step 1: Enable Required APIs** - Script created: `scripts/setup-workload-identity.ps1`
- [x] **Step 2: Create Workload Identity Pool** - Script handles pool creation
- [x] **Step 3: Configure GitHub Repository Access** - Script configures repository access
- [x] **Step 4: Grant Service Account Access** - Script grants service account permissions
- [x] **Step 5: Update GitHub Actions Workflow** - Workflow configured: `.github/workflows/deploy-gcp.yml`
  - [x] Uses `google-github-actions/auth@v2` with Workload Identity
  - [x] Has `id-token: write` permission
  - [x] Uses `WORKLOAD_IDENTITY_PROVIDER` and `GCP_SERVICE_ACCOUNT` secrets
- [x] **Step 6: Configure GitHub Secrets** - **COMPLETED**: All secrets added to GitHub repository
  - [x] `GCP_PROJECT_ID`
  - [x] `GCP_REGION`
  - [x] `WORKLOAD_IDENTITY_POOL`
  - [x] `WORKLOAD_IDENTITY_PROVIDER`
  - [x] `GCP_SERVICE_ACCOUNT`
- [x] **Quick Setup Script** - Created: `scripts/setup-workload-identity.ps1`

## What is Workload Identity Federation?

Workload Identity Federation allows GitHub Actions to authenticate to GCP without storing long-lived service account keys. It uses short-lived tokens instead, which is more secure.

## Prerequisites

1. GCP Project with billing enabled
2. GitHub repository
3. `gcloud` CLI installed and authenticated
4. Organization policy that allows Workload Identity Federation (or admin access to enable it)

## Step 1: Enable Required APIs

**✅ Automated via script**: `scripts/setup-workload-identity.ps1`

Or manually:
```powershell
gcloud services enable iamcredentials.googleapis.com --project="project-b79ef08c-1eb8-47ea-80e"
gcloud services enable sts.googleapis.com --project="project-b79ef08c-1eb8-47ea-80e"
```

## Step 2: Create Workload Identity Pool

**✅ Automated via script**: `scripts/setup-workload-identity.ps1`

Or manually:
```powershell
$projectId = "project-b79ef08c-1eb8-47ea-80e"
$poolId = "github-actions-pool"
$providerId = "github-provider"

# Create the pool
gcloud iam workload-identity-pools create $poolId `
    --project=$projectId `
    --location="global" `
    --display-name="GitHub Actions Pool"

# Create the provider
gcloud iam workload-identity-pools providers create-oidc $providerId `
    --project=$projectId `
    --location="global" `
    --workload-identity-pool=$poolId `
    --display-name="GitHub Provider" `
    --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository" `
    --issuer-uri="https://token.actions.githubusercontent.com"
```

## Step 3: Configure GitHub Repository Access

**✅ Automated via script**: `scripts/setup-workload-identity.ps1` (defaults to `sqordia-dev/backend`)

Get your GitHub repository in the format: `OWNER/REPO`

Or manually:
```powershell
$repository = "sqordia-dev/backend"  # Format: owner/repo (no https://github.com/)

# Allow GitHub Actions from your repository
gcloud iam workload-identity-pools providers update-oidc $providerId `
    --project=$projectId `
    --location="global" `
    --workload-identity-pool=$poolId `
    --attribute-condition="assertion.repository=='$repository'"
```

## Step 4: Grant Service Account Access

**✅ Automated via script**: `scripts/setup-workload-identity.ps1`

**Note**: Service account must be created first using `scripts/create-github-actions-sa.ps1`

Or manually:
```powershell
$saEmail = "github-actions@${projectId}.iam.gserviceaccount.com"
$poolResource = "projects/$projectNumber/locations/global/workloadIdentityPools/$poolId"

# Grant the service account permission to be impersonated
gcloud iam service-accounts add-iam-policy-binding $saEmail `
    --project=$projectId `
    --role="roles/iam.workloadIdentityUser" `
    --member="principalSet://iam.googleapis.com/$poolResource/attribute.repository/$repository"
```

## Step 5: Update GitHub Actions Workflow

**✅ COMPLETED**: `.github/workflows/deploy-gcp.yml` is already configured for Workload Identity Federation

The workflow includes:
- ✅ `id-token: write` permission (required for Workload Identity)
- ✅ Uses `google-github-actions/auth@v2` with `workload_identity_provider`
- ✅ References `WORKLOAD_IDENTITY_PROVIDER` and `GCP_SERVICE_ACCOUNT` secrets

Current configuration:

```yaml
name: Deploy to GCP

on:
  push:
    branches:
      - main
      - master
  workflow_dispatch:

env:
  GCP_PROJECT_ID: ${{ secrets.GCP_PROJECT_ID }}
  GCP_REGION: ${{ secrets.GCP_REGION || 'northamerica-northeast2' }}
  WORKLOAD_IDENTITY_POOL: ${{ secrets.WORKLOAD_IDENTITY_POOL }}
  WORKLOAD_IDENTITY_PROVIDER: ${{ secrets.WORKLOAD_IDENTITY_PROVIDER }}
  SERVICE_ACCOUNT: ${{ secrets.GCP_SERVICE_ACCOUNT }}

jobs:
  build-and-deploy:
    name: Build and Deploy to GCP
    runs-on: ubuntu-latest
    permissions:
      contents: read
      id-token: write  # Required for Workload Identity Federation

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Authenticate to Google Cloud
        uses: google-github-actions/auth@v2
        with:
          workload_identity_provider: ${{ env.WORKLOAD_IDENTITY_PROVIDER }}
          service_account: ${{ env.SERVICE_ACCOUNT }}

      # ... rest of the workflow remains the same
```

## Step 6: Configure GitHub Secrets

**✅ COMPLETED**: All secrets have been added to the GitHub repository

To verify or update secrets:
1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Verify the following secrets are configured:

| Secret Name | Value | Example |
|------------|-------|---------|
| `GCP_PROJECT_ID` | Your GCP Project ID | `project-b79ef08c-1eb8-47ea-80e` |
| `GCP_REGION` | GCP Region | `northamerica-northeast2` |
| `WORKLOAD_IDENTITY_POOL` | Full pool resource name | `projects/458949557590/locations/global/workloadIdentityPools/github-actions-pool` |
| `WORKLOAD_IDENTITY_PROVIDER` | Full provider resource name | `projects/458949557590/locations/global/workloadIdentityPools/github-actions-pool/providers/github-provider` |
| `GCP_SERVICE_ACCOUNT` | Service account email | `github-actions@project-b79ef08c-1eb8-47ea-80e.iam.gserviceaccount.com` |

### Getting the Resource Names

```powershell
# Get project number
$projectNumber = (gcloud projects describe $projectId --format="value(projectNumber)")

# Workload Identity Pool
$poolResource = "projects/$projectNumber/locations/global/workloadIdentityPools/github-actions-pool"

# Workload Identity Provider
$providerResource = "$poolResource/providers/github-provider"

Write-Host "WORKLOAD_IDENTITY_POOL: $poolResource"
Write-Host "WORKLOAD_IDENTITY_PROVIDER: $providerResource"
```

## Alternative: Use Service Account Key (If Policy Allows)

If your organization policy allows service account keys, you can use the traditional approach:

1. Create service account key:
```powershell
gcloud iam service-accounts keys create github-actions-key.json \
    --iam-account=github-actions@project-b79ef08c-1eb8-47ea-80e
```

2. Add `GCP_SA_KEY` secret to GitHub with the JSON content

3. Update workflow to use:
```yaml
- name: Authenticate to Google Cloud
  uses: google-github-actions/auth@v2
  with:
    credentials_json: ${{ secrets.GCP_SA_KEY }}
```

## Troubleshooting

### Error: "Key creation is not allowed"

**Solution**: Use Workload Identity Federation (this guide)

### Error: "Workload Identity Pool not found"

**Solution**: Ensure you've created the pool and provider correctly

### Error: "Permission denied" or "IAM_PERMISSION_DENIED"

**Solution**: 
1. **Grant Terraform permissions to the service account:**
   ```powershell
   .\scripts\grant-terraform-permissions.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e"
   ```
   
   This script grants all necessary IAM roles for Terraform to create GCP resources:
   - `roles/owner` - Full access to create all resources (recommended for Terraform)
   - Or specific roles: `iam.serviceAccountAdmin`, `storage.admin`, `logging.admin`, `artifactregistry.admin`, `cloudsql.admin`, `pubsub.admin`, `secretmanager.admin`, `run.admin`, `cloudfunctions.admin`

2. Verify service account has necessary IAM roles
3. Check Workload Identity binding is correct
4. Verify repository name matches exactly

### Error: "Invalid issuer"

**Solution**: Ensure issuer URI is exactly: `https://token.actions.githubusercontent.com`

## Security Benefits

✅ **No long-lived keys**: Uses short-lived tokens
✅ **Automatic rotation**: Tokens are automatically rotated
✅ **Repository-scoped**: Only works for specified repositories
✅ **Audit trail**: All authentications are logged
✅ **Policy compliance**: Meets organization security requirements

## Quick Setup Script

**✅ COMPLETED**: Script created at `scripts/setup-workload-identity.ps1`

**Usage:**
```powershell
.\scripts\setup-workload-identity.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e" -GitHubRepository "sqordia-dev/backend"
```

The script will:
1. Enable required APIs
2. Create Workload Identity Pool
3. Create OIDC Provider
4. Grant service account access
5. Display all required GitHub secrets

**Original script reference:**

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectId,
    
    [Parameter(Mandatory=$true)]
    [string]$GitHubRepository  # Format: owner/repo
)

$poolId = "github-actions-pool"
$providerId = "github-provider"
$saEmail = "github-actions@${ProjectId}.iam.gserviceaccount.com"

# Enable APIs
gcloud services enable iamcredentials.googleapis.com --project=$ProjectId
gcloud services enable sts.googleapis.com --project=$ProjectId

# Create pool
gcloud iam workload-identity-pools create $poolId `
    --project=$ProjectId `
    --location="global" `
    --display-name="GitHub Actions Pool"

# Create provider
gcloud iam workload-identity-pools providers create-oidc $providerId `
    --project=$ProjectId `
    --location="global" `
    --workload-identity-pool=$poolId `
    --display-name="GitHub Provider" `
    --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository" `
    --issuer-uri="https://token.actions.githubusercontent.com" `
    --attribute-condition="assertion.repository=='$GitHubRepository'"

# Get project number
$projectNumber = (gcloud projects describe $ProjectId --format="value(projectNumber)")
$poolResource = "projects/$projectNumber/locations/global/workloadIdentityPools/$poolId"

# Grant service account access
gcloud iam service-accounts add-iam-policy-binding $saEmail `
    --project=$ProjectId `
    --role="roles/iam.workloadIdentityUser" `
    --member="principalSet://iam.googleapis.com/$poolResource/attribute.repository/$GitHubRepository"

# Output secrets
Write-Host "`n=== GitHub Secrets ===" -ForegroundColor Cyan
Write-Host "GCP_PROJECT_ID: $ProjectId"
Write-Host "GCP_REGION: northamerica-northeast2"
Write-Host "WORKLOAD_IDENTITY_POOL: $poolResource"
Write-Host "WORKLOAD_IDENTITY_PROVIDER: $poolResource/providers/$providerId"
Write-Host "GCP_SERVICE_ACCOUNT: $saEmail"
```

## Summary

### ✅ Completed Items

1. **Setup Script Created**: `scripts/setup-workload-identity.ps1` - Automated setup script for all Workload Identity Federation steps
2. **Service Account Script**: `scripts/create-github-actions-sa.ps1` - Creates service account with required permissions
3. **Workflow Configured**: `.github/workflows/deploy-gcp.yml` - Uses Workload Identity Federation
   - Has `id-token: write` permission
   - Uses `google-github-actions/auth@v2` with Workload Identity
   - References correct secrets

### ✅ All Steps Completed

**GitHub Secrets Configuration**: ✅ All secrets have been configured in the GitHub repository

**Setup Status:**
1. ✅ Service account created via `scripts/create-github-actions-sa.ps1`
2. ✅ Workload Identity Federation configured via `scripts/setup-workload-identity.ps1`
3. ✅ GitHub secrets added to repository
4. ✅ Workflow configured for Workload Identity Federation

**Next Steps:**
- Test the workflow by pushing to `main` branch or manually triggering it
- Monitor the workflow execution in GitHub Actions
- Verify deployment to GCP Cloud Run

## Related Documentation

- [GCP Workload Identity Federation](https://cloud.google.com/iam/docs/workload-identity-federation)
- [GitHub Actions with GCP](https://github.com/google-github-actions/auth)
- [GCP GitHub Actions Setup](./GCP_GITHUB_ACTIONS_SETUP.md)

