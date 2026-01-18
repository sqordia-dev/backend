# Environment Variables Setup Guide

This guide explains how to set the necessary environment variables for the Container App deployment.

## Required Environment Variables

### OpenAI Configuration (Required for AI Features)

The following environment variables are **required** for OpenAI API functionality:

- `OPENAI_API_KEY` - Your OpenAI API key (starts with `sk-` or `sk-proj-`)
- `OPENAI_MODEL` - The OpenAI model to use (default: `gpt-4`)

**Alternative variable names** (all supported):
- `OpenAI__ApiKey` / `AI__OpenAI__ApiKey` (for configuration binding)
- `OpenAI__Model` / `AI__OpenAI__Model` (for configuration binding)

### Frontend Configuration

- `FRONTEND_BASE_URL` - Your frontend URL (default: `https://sqordia.app`)
- `Frontend__BaseUrl` - Alternative format for configuration binding

### Azure Communication Services Email

- `AzureCommunicationServices__ConnectionString` - Connection string for Azure Communication Services
- `AzureCommunicationServices__FromEmail` - Email sender address (auto-generated if empty)
- `AzureCommunicationServices__FromName` - Email sender display name (default: "Sqordia")

## Setting Environment Variables

### Option 1: Using Terraform (Recommended)

Add these variables to your `terraform.tfvars` file:

```hcl
# OpenAI Configuration
openai_api_key = "sk-your-actual-api-key-here"
openai_model   = "gpt-4"  # or "gpt-4o", "gpt-3.5-turbo", etc.

# Frontend Configuration
frontend_base_url = "https://sqordia.app"
```

Then run:
```bash
terraform plan
terraform apply
```

The Terraform configuration will automatically set all environment variables in the Container App.

### Option 2: Using Azure Portal

1. Go to Azure Portal → Container Apps → Your Container App
2. Navigate to **Settings** → **Environment variables**
3. Add the following variables:

| Name | Value |
|------|-------|
| `OPENAI_API_KEY` | `sk-your-actual-api-key-here` |
| `OPENAI_MODEL` | `gpt-4` |
| `FRONTEND_BASE_URL` | `https://sqordia.app` |

4. Click **Save**

### Option 3: Using Azure CLI

```bash
az containerapp update \
  --name sqordia-production-api \
  --resource-group sqordia-production-rg \
  --set-env-vars \
    OPENAI_API_KEY="sk-your-actual-api-key-here" \
    OPENAI_MODEL="gpt-4" \
    FRONTEND_BASE_URL="https://sqordia.app"
```

### Option 4: Using Azure Key Vault (Recommended for Secrets)

For sensitive values like API keys, store them in Azure Key Vault:

1. Store the secret in Key Vault:
```bash
az keyvault secret set \
  --vault-name sqordia-production-kv \
  --name OpenAI--ApiKey \
  --value "sk-your-actual-api-key-here"
```

2. Reference it in Container App environment variables:
   - In Azure Portal: Use Key Vault reference format: `@Microsoft.KeyVault(SecretUri=https://sqordia-production-kv.vault.azure.net/secrets/OpenAI--ApiKey/)`
   - In Terraform: Use `azurerm_key_vault_secret` resource and reference it

## Verification

After setting the environment variables, verify they are working:

1. Check Container App logs for OpenAI initialization:
   ```
   OpenAI service initialized successfully with model: gpt-4
   ```

2. If you see warnings like:
   ```
   OpenAI API key not configured. AI features will be unavailable.
   ```
   This means the environment variable is not set or contains a placeholder value.

## Troubleshooting

### Error: "invalid_api_key: Incorrect API key provided: ${OPENAI_API_KEY}"

**Cause**: The placeholder value `${OPENAI_API_KEY}` is being used instead of the actual API key.

**Solution**: 
1. Ensure the `OPENAI_API_KEY` environment variable is set in your Container App
2. Verify it's not a placeholder value (should start with `sk-`)
3. The code now automatically skips placeholder values, but you still need to set the actual value

### Error: "OpenAI service is not configured"

**Cause**: The API key environment variable is missing or empty.

**Solution**:
1. Set the `OPENAI_API_KEY` environment variable in your Container App
2. Restart the Container App after setting the variable
3. Check the logs to confirm the service initialized

## Security Best Practices

1. **Never commit API keys to version control**
   - Use `terraform.tfvars` (which should be in `.gitignore`)
   - Or use Azure Key Vault for secrets

2. **Use Key Vault for production**
   - Store sensitive values in Azure Key Vault
   - Reference them in Container App environment variables

3. **Rotate keys regularly**
   - Update API keys periodically
   - Use separate keys for different environments

## Current Configuration Status

The Terraform configuration (`container_apps.tf`) now includes:
- ✅ OpenAI API key and model environment variables
- ✅ Frontend base URL
- ✅ Azure Communication Services connection string and email settings
- ✅ All other required environment variables

All environment variables are automatically set when you run `terraform apply` with the proper `terraform.tfvars` file.
