# Azure Terraform Infrastructure

This directory contains Terraform configuration for deploying Sqordia to Microsoft Azure.

## Architecture

- **Container Apps**: API container service (replaces GCP Cloud Run)
- **Azure Database for PostgreSQL**: Managed PostgreSQL (replaces GCP Cloud SQL)
- **Azure Blob Storage**: Object storage for documents (replaces GCP Cloud Storage)
- **Azure Service Bus**: Message queuing (replaces GCP Pub/Sub)
- **Azure Functions**: Serverless functions for background jobs (replaces GCP Cloud Functions)
- **Azure Key Vault**: Secrets management (replaces GCP Secret Manager)
- **Azure Container Registry**: Container registry (replaces GCP Artifact Registry)
- **Azure Monitor Logs**: Logging and monitoring (replaces GCP Cloud Logging)

## Cost Optimization

This configuration is optimized for cost:
- **PostgreSQL**: Uses Basic tier (B_Gen5_1) - free tier eligible for first 12 months
- **Container Apps**: Minimum instances = 0 (pay-per-use)
- **Service Bus**: Basic tier (free tier eligible)
- **Functions**: Consumption plan (pay-per-execution)
- **Blob Storage**: LRS replication (cheapest option)
- **Log Analytics**: 7-day retention

**Estimated Monthly Cost:**
- First 12 months: ~$23-27 CAD/month
- After 12 months: ~$43-51 CAD/month

## Prerequisites

1. **Azure Account**: Create an Azure account and subscription
2. **Azure CLI**: Install and authenticate
   ```bash
   az login
   az account set --subscription "YOUR_SUBSCRIPTION_ID"
   ```
3. **Terraform**: Install Terraform >= 1.0
4. **Service Principal** (optional, for CI/CD):
   ```bash
   az ad sp create-for-rbac --role="Contributor" --scopes="/subscriptions/YOUR_SUBSCRIPTION_ID"
   ```

## Setup

1. **Copy example variables file:**
   ```bash
   cd infrastructure/terraform/azure
   cp terraform.tfvars.example terraform.tfvars
   ```

2. **Edit terraform.tfvars:**
   - Set `azure_subscription_id` (get from `az account show`)
   - Set `azure_location` (recommended: `canadacentral` for CAD pricing)
   - Set `postgresql_admin_password` (strong password)
   - Adjust other variables as needed

3. **Initialize Terraform:**
   ```bash
   terraform init
   ```

4. **Plan deployment:**
   ```bash
   terraform plan
   ```

5. **Apply deployment:**
   ```bash
   terraform apply
   ```

## Configuration

### Required Variables

- `azure_subscription_id`: Your Azure subscription ID
- `postgresql_admin_password`: Strong password for PostgreSQL admin user

### Optional Variables

All other variables have sensible defaults. See `variables.tf` for details.

### Important Notes

1. **Storage Account Names**: Must be globally unique, lowercase, alphanumeric (no hyphens)
   - Blob Storage: `${project_name}${environment}storage`
   - Functions Storage: `${project_name}${environment}func`
   - Container Registry: `${project_name}${environment}acr`

2. **Key Vault Name**: Must be globally unique, lowercase, alphanumeric (hyphens allowed)

3. **Container Registry**: Must be globally unique, lowercase, alphanumeric (no hyphens)

4. **PostgreSQL Firewall**: Currently allows all IPs (0.0.0.0/0) for development. **Restrict this in production!**

## Deployment Steps

1. **Create Resource Group** (if not exists):
   ```bash
   az group create --name sqordia-production-rg --location canadacentral
   ```

2. **Run Terraform:**
   ```bash
   terraform init
   terraform plan
   terraform apply
   ```

3. **Get outputs:**
   ```bash
   terraform output
   ```

## Post-Deployment

1. **Build and push Docker image:**
   ```bash
   # Login to ACR
   az acr login --name sqordiaproductionacr

   # Build and push
   docker build -t sqordiaproductionacr.azurecr.io/api:latest .
   docker push sqordiaproductionacr.azurecr.io/api:latest
   ```

2. **Deploy Functions:**
   - Build your function projects
   - Deploy using Azure Functions Core Tools or VS Code

3. **Configure Application:**
   - Update connection strings in Container Apps
   - Configure Service Bus connections
   - Set up Key Vault access policies

## Outputs

After deployment, Terraform will output:
- Container App URL
- PostgreSQL connection details
- Storage account information
- Service Bus namespace
- Key Vault URI
- And more...

## Cost Monitoring

Monitor costs using Azure Cost Management:
```bash
az consumption usage list --start-date 2024-01-01 --end-date 2024-01-31
```

Or via Azure Portal: Cost Management + Billing

## Troubleshooting

### Common Issues

1. **Storage Account Name Already Exists**
   - Storage account names must be globally unique
   - Change the name in `terraform.tfvars` or use a different project name

2. **Key Vault Name Already Exists**
   - Key Vault names must be globally unique
   - Change the name in variables

3. **Container Registry Name Already Exists**
   - Container Registry names must be globally unique
   - Change the name in variables

4. **PostgreSQL Connection Issues**
   - Check firewall rules
   - Verify SSL is enabled in connection string
   - Check network security groups

## Security Best Practices

1. **Restrict PostgreSQL Firewall**: Remove the "AllowAll" rule and add specific IP ranges
2. **Use Managed Identity**: Instead of access keys for storage
3. **Enable Key Vault Soft Delete**: Already configured
4. **Rotate Secrets**: Regularly rotate database passwords
5. **Enable Logging**: Monitor access to Key Vault and other resources

## Cleanup

To destroy all resources:
```bash
terraform destroy
```

**Warning**: This will delete all resources including databases. Make sure you have backups!

## Related Documentation

- [Azure Services Summary](../../../docs/AZURE_SERVICES_SUMMARY.md)
- [Azure Cost Estimate](../../../docs/AZURE_COST_ESTIMATE.md)
- [GCP Terraform README](../gcp/README.md) - For comparison

