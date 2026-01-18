# Environment Variables - Local Configuration Values

This document contains the environment variable values found in your local configuration files. Use these values when setting up your Terraform `terraform.tfvars` file or Azure Container App environment variables.

## Values Found in Local Configuration

### OpenAI Configuration

**From `appsettings.json` and `docker-compose.dev.yml`:**

- **OPENAI_API_KEY**: `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`
- **OPENAI_MODEL**: `gpt-4o`

### Frontend Configuration

**From `appsettings.json`:**

- **FRONTEND_BASE_URL** (Development): `http://localhost:5173`
- **FRONTEND_BASE_URL** (Production): `https://sqordia.app` (from appsettings.Production.json)

### Azure Communication Services Email

**From `appsettings.json`:**

- **AzureCommunicationServices__ConnectionString**: `endpoint=https://sqordia-production-email-communication.unitedstates.communication.azure.com/;accesskey=3krcF1HehIY9GJXALp1nXZdizvZcvrBhFTIARftGslV3R9gOiCTnJQQJ99CAACULyCpYfSk6AAAAAZCS9tdd`
- **AzureCommunicationServices__FromEmail**: `DoNotReply@b1affd52-d472-447c-98a4-933170151cde.azurecomm.net`
- **AzureCommunicationServices__FromName**: `Sqordia`

### Google OAuth Configuration

**From `appsettings.json`:**

- **GoogleOAuth__ClientId**: `787375969256-r1qbhjb39t2r77ro1evr849ph75s8i4n.apps.googleusercontent.com`
- **GoogleOAuth__ClientSecret**: `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`
- **GoogleOAuth__RedirectUri** (Production): `https://sqordia.app/api/v1/auth/google/callback`

## Recommended terraform.tfvars Configuration

Based on the local values found, here's what your `terraform.tfvars` should contain:

```hcl
# OpenAI Configuration
openai_api_key = "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"
openai_model   = "gpt-4o"

# Frontend Configuration
frontend_base_url = "https://sqordia.app"

# Email Configuration (Azure Communication Services)
# Note: These are already configured in Terraform from Azure resources
# The connection string will be automatically set from azurerm_communication_service
# The from email will be auto-generated if email_from_address is empty
email_from_address = ""  # Leave empty to use Azure managed domain
email_from_name    = "Sqordia"
```

## Important Notes

1. **Security Warning**: The values shown above are from your local development configuration. For production:
   - Consider using Azure Key Vault for sensitive values
   - Rotate API keys regularly
   - Never commit `terraform.tfvars` with actual secrets to version control

2. **Azure Communication Services**: The connection string and email address are already managed by Terraform resources. The values in `appsettings.json` are for reference - Terraform will automatically set them from the Azure resources.

3. **OpenAI API Key**: This is a real API key found in your local configuration. Make sure it's valid and has appropriate usage limits set.

4. **Model Selection**: The local config uses `gpt-4o`. You can change this to `gpt-4`, `gpt-3.5-turbo`, or any other supported model in your `terraform.tfvars`.

## Quick Setup

To quickly set up your Terraform variables, copy the values above into your `terraform.tfvars` file:

```bash
cd backend/infrastructure/terraform/azure
cp terraform.tfvars.example terraform.tfvars
# Then edit terraform.tfvars with the values above
```

Then run:
```bash
terraform plan
terraform apply
```

This will automatically configure all environment variables in your Azure Container App.
