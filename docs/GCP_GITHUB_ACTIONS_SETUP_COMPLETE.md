# GitHub Actions Setup - Current Status

## ✅ Completed

1. **Service Account Created**
   - Name: `github-actions@project-b79ef08c-1eb8-47ea-80e.iam.gserviceaccount.com`
   - Display Name: "GitHub Actions Service Account"

2. **IAM Roles Granted**
   - ✅ `roles/artifactregistry.writer` - Push Docker images
   - ✅ `roles/cloudfunctions.admin` - Deploy Cloud Functions
   - ✅ `roles/secretmanager.secretAccessor` - Access secrets
   - ✅ `roles/cloudsql.client` - Connect to Cloud SQL
   - ⚠️ `roles/run.admin` - Needs to be granted (may have been skipped)

3. **Workflow File Created**
   - Location: `.github/workflows/deploy-gcp.yml`
   - Includes all deployment steps

## ⚠️ Service Account Key Creation Blocked

Your organization has a policy that prevents service account key creation:
```
ERROR: Key creation is not allowed on this service account.
- constraints/iam.disableServiceAccountKeyCreation
```

## 🔐 Solution: Use Workload Identity Federation

Since service account keys are blocked, use **Workload Identity Federation** (recommended and more secure).

### Quick Setup

1. **Enable Required APIs:**
```powershell
gcloud services enable iamcredentials.googleapis.com --project="project-b79ef08c-1eb8-47ea-80e"
gcloud services enable sts.googleapis.com --project="project-b79ef08c-1eb8-47ea-80e"
```

2. **Create Workload Identity Pool and Provider:**
```powershell
$projectId = "project-b79ef08c-1eb8-47ea-80e"
$repository = "YOUR_GITHUB_USERNAME/Sqordia-backend"  # Update this

# Create pool
gcloud iam workload-identity-pools create github-actions-pool `
    --project=$projectId `
    --location="global" `
    --display-name="GitHub Actions Pool"

# Create provider
gcloud iam workload-identity-pools providers create-oidc github-provider `
    --project=$projectId `
    --location="global" `
    --workload-identity-pool=github-actions-pool `
    --display-name="GitHub Provider" `
    --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository" `
    --issuer-uri="https://token.actions.githubusercontent.com" `
    --attribute-condition="assertion.repository=='$repository'"
```

3. **Grant Service Account Access:**
```powershell
$saEmail = "github-actions@${projectId}.iam.gserviceaccount.com"
$projectNumber = (gcloud projects describe $projectId --format="value(projectNumber)")
$poolResource = "projects/$projectNumber/locations/global/workloadIdentityPools/github-actions-pool"

gcloud iam service-accounts add-iam-policy-binding $saEmail `
    --project=$projectId `
    --role="roles/iam.workloadIdentityUser" `
    --member="principalSet://iam.googleapis.com/$poolResource/attribute.repository/$repository"
```

4. **Get Resource Names for GitHub Secrets:**
```powershell
$projectNumber = (gcloud projects describe $projectId --format="value(projectNumber)")
Write-Host "WORKLOAD_IDENTITY_POOL: projects/$projectNumber/locations/global/workloadIdentityPools/github-actions-pool"
Write-Host "WORKLOAD_IDENTITY_PROVIDER: projects/$projectNumber/locations/global/workloadIdentityPools/github-actions-pool/providers/github-provider"
Write-Host "GCP_SERVICE_ACCOUNT: github-actions@${projectId}.iam.gserviceaccount.com"
```

5. **Update GitHub Actions Workflow:**
   - Update `.github/workflows/deploy-gcp.yml` to use Workload Identity Federation
   - See `docs/GCP_GITHUB_ACTIONS_WORKLOAD_IDENTITY.md` for details

6. **Add GitHub Secrets:**
   - `GCP_PROJECT_ID`: `project-b79ef08c-1eb8-47ea-80e`
   - `GCP_REGION`: `northamerica-northeast2`
   - `WORKLOAD_IDENTITY_POOL`: (from step 4)
   - `WORKLOAD_IDENTITY_PROVIDER`: (from step 4)
   - `GCP_SERVICE_ACCOUNT`: `github-actions@project-b79ef08c-1eb8-47ea-80e.iam.gserviceaccount.com`

## 📚 Documentation

- **Workload Identity Setup**: `docs/GCP_GITHUB_ACTIONS_WORKLOAD_IDENTITY.md`
- **Original Setup Guide**: `docs/GCP_GITHUB_ACTIONS_SETUP.md`
- **Workflow File**: `.github/workflows/deploy-gcp.yml`

## 🎯 Next Steps

1. ✅ Service account created
2. ✅ IAM roles granted
3. ⏳ Set up Workload Identity Federation (see guide above)
4. ⏳ Update workflow file for Workload Identity
5. ⏳ Add GitHub secrets
6. ⏳ Test the workflow

## 💡 Benefits of Workload Identity Federation

- ✅ More secure (no long-lived keys)
- ✅ Automatic token rotation
- ✅ Repository-scoped access
- ✅ Meets organization security policies
- ✅ Better audit trail

