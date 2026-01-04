# GitHub Secrets Setup for sqordia-dev/backend

## ✅ Workload Identity Federation Setup Complete

The Workload Identity Federation has been configured for your repository: **sqordia-dev/backend**

## 📋 Required GitHub Secrets

Add these secrets to your GitHub repository:

**Repository**: https://github.com/sqordia-dev/backend  
**Path**: Settings → Secrets and variables → Actions → New repository secret

### Secret 1: GCP_PROJECT_ID
- **Name**: `GCP_PROJECT_ID`
- **Value**: `project-b79ef08c-1eb8-47ea-80e`

### Secret 2: GCP_REGION
- **Name**: `GCP_REGION`
- **Value**: `northamerica-northeast2`

### Secret 3: WORKLOAD_IDENTITY_POOL
- **Name**: `WORKLOAD_IDENTITY_POOL`
- **Value**: `projects/458949557590/locations/global/workloadIdentityPools/github-actions-pool`

### Secret 4: WORKLOAD_IDENTITY_PROVIDER
- **Name**: `WORKLOAD_IDENTITY_PROVIDER`
- **Value**: `projects/458949557590/locations/global/workloadIdentityPools/github-actions-pool/providers/github-provider`

### Secret 5: GCP_SERVICE_ACCOUNT
- **Name**: `GCP_SERVICE_ACCOUNT`
- **Value**: `github-actions@project-b79ef08c-1eb8-47ea-80e.iam.gserviceaccount.com`

## 🔧 How to Add Secrets

1. Go to: https://github.com/sqordia-dev/backend/settings/secrets/actions
2. Click **"New repository secret"**
3. Enter the secret name and value from the list above
4. Click **"Add secret"**
5. Repeat for all 5 secrets

## ✅ Verification

After adding all secrets:

1. Go to the **Actions** tab in your repository
2. You should see the **"Deploy to GCP"** workflow
3. The workflow is configured to:
   - Trigger on push to `main` or `master` branch
   - Can be manually triggered via **"Run workflow"**

## 🚀 Testing the Workflow

### Option 1: Manual Trigger
1. Go to **Actions** → **Deploy to GCP**
2. Click **"Run workflow"**
3. Select branch and click **"Run workflow"**

### Option 2: Push to Main
```bash
git add .
git commit -m "Test GitHub Actions deployment"
git push origin main
```

## 📝 Workflow Configuration

The workflow file (`.github/workflows/deploy-gcp.yml`) has been updated to use Workload Identity Federation:

- ✅ Uses `workload_identity_provider` instead of service account key
- ✅ Requires `id-token: write` permission (already configured)
- ✅ Authenticates using short-lived tokens (more secure)

## 🔒 Security Notes

- ✅ No long-lived service account keys stored
- ✅ Tokens are automatically rotated
- ✅ Repository-scoped access (only works for sqordia-dev/backend)
- ✅ All authentications are logged in GCP

## 🐛 Troubleshooting

### Error: "Workload Identity Provider not found"
- Verify the `WORKLOAD_IDENTITY_PROVIDER` secret is set correctly
- Check that the pool and provider were created successfully

### Error: "Permission denied"
- Verify the service account has the necessary IAM roles
- Check that the repository name matches exactly: `sqordia-dev/backend`

### Error: "Invalid issuer"
- The issuer URI should be: `https://token.actions.githubusercontent.com`
- This is already configured correctly

## 📚 Related Documentation

- [Workload Identity Setup Guide](./GCP_GITHUB_ACTIONS_WORKLOAD_IDENTITY.md)
- [GitHub Actions Setup](./GCP_GITHUB_ACTIONS_SETUP.md)
- [Workflow File](../.github/workflows/deploy-gcp.yml)

## ✅ Setup Checklist

- [x] Workload Identity Pool created
- [x] OIDC Provider created
- [x] Service account granted access
- [x] Workflow file updated
- [ ] GitHub secrets added (5 secrets)
- [ ] Workflow tested

**Next Step**: Add the 5 GitHub secrets listed above, then test the workflow!

