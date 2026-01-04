# Fix Terraform Permission Errors

If you're seeing permission errors like:
- `Permission 'iam.serviceAccounts.create' denied`
- `Permission 'storage.buckets.create' denied`
- `Permission 'logging.logMetrics.create' denied`
- `Permission 'artifactregistry.repositories.create' denied`
- `Permission 'secretmanager.secrets.create' denied`

This means the GitHub Actions service account doesn't have sufficient permissions to create GCP resources via Terraform.

## Quick Fix

Run the permission grant script:

```powershell
.\scripts\grant-terraform-permissions.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e"
```

Replace `project-b79ef08c-1eb8-47ea-80e` with your actual GCP Project ID.

## What the Script Does

The script grants the `roles/owner` role to the GitHub Actions service account, which provides full access to create and manage all GCP resources. This is the recommended approach for Terraform deployments.

### Alternative: Least Privilege Approach

If you prefer to grant only specific permissions, you can modify the script to grant individual roles:

- `roles/iam.serviceAccountAdmin` - Create service accounts
- `roles/storage.admin` - Create storage buckets
- `roles/logging.admin` - Create logging metrics
- `roles/artifactregistry.admin` - Create Artifact Registry repositories
- `roles/cloudsql.admin` - Create Cloud SQL instances
- `roles/pubsub.admin` - Create Pub/Sub topics
- `roles/secretmanager.admin` - Create secrets
- `roles/run.admin` - Deploy Cloud Run services
- `roles/cloudfunctions.admin` - Deploy Cloud Functions
- `roles/resourcemanager.projectIamAdmin` - Manage IAM bindings

## Manual Grant (if script doesn't work)

If you need to grant permissions manually:

```powershell
$projectId = "project-b79ef08c-1eb8-47ea-80e"
$saEmail = "github-actions@$projectId.iam.gserviceaccount.com"

# Grant Owner role (full access)
gcloud projects add-iam-policy-binding $projectId `
    --member="serviceAccount:$saEmail" `
    --role="roles/owner"
```

## Verify Permissions

After granting permissions, verify the service account has the roles:

```powershell
gcloud projects get-iam-policy $projectId `
    --flatten="bindings[].members" `
    --filter="bindings.members:serviceAccount:$saEmail" `
    --format="table(bindings.role)"
```

## After Fixing Permissions

1. Re-run the failed GitHub Actions workflow
2. The Terraform deployment should now succeed
3. All resources should be created successfully

## Security Note

Granting `roles/owner` provides full access to the GCP project. This is standard for CI/CD service accounts that need to deploy infrastructure. The service account is:
- Only accessible via Workload Identity Federation
- Scoped to specific GitHub repositories
- Audited in Cloud Logging
- Can be restricted further if needed after initial deployment

