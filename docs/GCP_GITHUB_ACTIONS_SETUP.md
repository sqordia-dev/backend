# GitHub Actions Setup for GCP Deployment

This guide explains how to set up GitHub Actions for automated GCP deployment.

## Prerequisites

1. GCP Project with billing enabled
2. GitHub repository with Actions enabled
3. GCP Service Account for GitHub Actions
4. GitHub repository secrets configured

## Step 1: Create GCP Service Account

Run the provided script to create a service account with necessary permissions:

```powershell
.\scripts\create-github-actions-sa.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e"
```

This script will:
- Create a service account named `github-actions`
- Grant necessary IAM roles:
  - `roles/run.admin` - Deploy Cloud Run services
  - `roles/artifactregistry.writer` - Push Docker images
  - `roles/cloudfunctions.admin` - Deploy Cloud Functions
  - `roles/secretmanager.secretAccessor` - Access secrets
  - `roles/cloudsql.client` - Connect to Cloud SQL
- Generate a service account key file

## Step 2: Configure GitHub Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secrets:

### Required Secrets

| Secret Name | Description | Example Value |
|------------|-------------|--------------|
| `GCP_PROJECT_ID` | Your GCP Project ID | `project-b79ef08c-1eb8-47ea-80e` |
| `GCP_REGION` | GCP Region (optional, defaults to `northamerica-northeast2`) | `northamerica-northeast2` |
| `GCP_SA_KEY` | Service Account Key JSON (entire contents of the key file) | `{"type":"service_account",...}` |

### Getting the Service Account Key

After running the setup script, get the key content:

```powershell
Get-Content github-actions-key.json | Out-String
```

Copy the entire JSON output and paste it as the `GCP_SA_KEY` secret value.

**⚠️ Important**: Never commit the key file to Git. It's already in `.gitignore`.

## Step 3: Verify Workflow File

The workflow file is located at:
```
.github/workflows/deploy-gcp.yml
```

It includes:
- Automatic deployment on push to `main` or `master` branch
- Manual deployment via workflow dispatch
- Docker image build and push
- Cloud Functions build
- Terraform deployment
- Health checks
- Deployment summary

## Step 4: Test the Workflow

### Option A: Manual Trigger

1. Go to **Actions** tab in GitHub
2. Select **Deploy to GCP** workflow
3. Click **Run workflow**
4. Select branch and optionally skip tests
5. Click **Run workflow**

### Option B: Push to Main Branch

Simply push to the `main` or `master` branch:

```bash
git add .
git commit -m "Trigger deployment"
git push origin main
```

## Step 5: Monitor Deployment

1. Go to **Actions** tab in GitHub
2. Click on the running workflow
3. Monitor each step:
   - ✅ Checkout code
   - ✅ Set up .NET
   - ✅ Run tests (if not skipped)
   - ✅ Authenticate to Google Cloud
   - ✅ Build Docker image
   - ✅ Push Docker image
   - ✅ Build Cloud Functions
   - ✅ Terraform Init/Plan/Apply
   - ✅ Health Check
   - ✅ Deployment Summary

## Workflow Features

### Automatic Deployment
- Triggers on push to `main` or `master`
- Builds and deploys latest code
- Tags images with commit SHA

### Manual Deployment
- Can be triggered manually via GitHub UI
- Option to skip tests for faster deployment
- Useful for hotfixes

### Health Checks
- Waits 30 seconds for Cloud Run to be ready
- Tests `/health` endpoint
- Reports deployment status

### Deployment Summary
- Shows Cloud Run URL
- Displays image tag (commit SHA)
- Includes commit message

## Troubleshooting

### Authentication Errors

**Error**: `google-github-actions/auth@v2` authentication failed

**Solution**:
1. Verify `GCP_SA_KEY` secret is correctly set
2. Ensure service account has necessary permissions
3. Check that the key JSON is complete (no truncation)

### Docker Build Failures

**Error**: Docker build fails

**Solution**:
1. Check Dockerfile syntax
2. Verify all dependencies are available
3. Check build logs for specific errors

### Terraform Errors

**Error**: Terraform apply fails

**Solution**:
1. Check Terraform state is initialized
2. Verify all required variables are set in `terraform.tfvars`
3. Check GCP API quotas and limits
4. Review Terraform plan output

### Cloud Run Deployment Fails

**Error**: Cloud Run service not updating

**Solution**:
1. Verify Artifact Registry image exists
2. Check Cloud Run service account permissions
3. Verify environment variables are set correctly
4. Check Cloud Run logs for errors

## Security Best Practices

1. **Service Account Key Rotation**: Rotate keys periodically
2. **Least Privilege**: Only grant necessary IAM roles
3. **Secret Management**: Use GitHub Secrets, never hardcode
4. **Branch Protection**: Protect `main` branch from direct pushes
5. **Review Workflows**: Require PR reviews for workflow changes

## Advanced Configuration

### Custom Regions

If using a different region, update the workflow:

```yaml
env:
  GCP_REGION: ${{ secrets.GCP_REGION || 'your-region' }}
```

### Custom Image Tags

Modify the image tagging strategy:

```yaml
docker tag ...:${{ github.sha }} ...:${{ github.ref_name }}-${{ github.sha }}
```

### Conditional Deployment

Add conditions to skip deployment:

```yaml
- name: Terraform Apply
  if: github.event_name != 'pull_request'
  run: terraform apply -auto-approve
```

## Related Documentation

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GCP GitHub Actions](https://github.com/google-github-actions)
- [Terraform GitHub Actions](https://github.com/hashicorp/setup-terraform)
- [GCP Completion Plan](./GCP_COMPLETION_PLAN.md)

## Quick Reference

### Required GitHub Secrets
```
GCP_PROJECT_ID=project-b79ef08c-1eb8-47ea-80e
GCP_REGION=northamerica-northeast2
GCP_SA_KEY={"type":"service_account",...}
```

### Workflow File Location
```
.github/workflows/deploy-gcp.yml
```

### Service Account Setup Script
```powershell
.\scripts\create-github-actions-sa.ps1 -ProjectId "YOUR_PROJECT_ID"
```

### Manual Workflow Trigger
GitHub UI → Actions → Deploy to GCP → Run workflow

