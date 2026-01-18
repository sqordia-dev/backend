# Environment Variables Configuration Summary

This document summarizes all environment variables configured in the Terraform deployment for the Azure Container App.

## ✅ Core Application Variables

### ASP.NET Core
- `ASPNETCORE_ENVIRONMENT` = `Production`

### Database
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string (auto-generated from Terraform resources)

### Azure Storage
- `AzureStorage__AccountName` - Storage account name (from Terraform)
- `AzureStorage__ConnectionString` - Storage connection string (from Terraform)
- `AzureStorage__ContainerName` - Blob container name (from Terraform)

### Azure Service Bus
- `AzureServiceBus__ConnectionString` - Service Bus connection string (from Terraform)
- `AzureServiceBus__EmailTopic` - Email topic name (from Terraform)
- `AzureServiceBus__AiGenerationTopic` - AI generation topic name (from Terraform)
- `AzureServiceBus__ExportTopic` - Export topic name (from Terraform)

### Azure Key Vault
- `AzureKeyVault__VaultUrl` - Key Vault URL (from Terraform)

### Azure Communication Services
- `AzureCommunicationServices__ConnectionString` - Email service connection string (from Terraform)
- `AzureCommunicationServices__FromEmail` - Sender email address (from Terraform or variable)
- `AzureCommunicationServices__FromName` - Sender display name (from variable)

## ✅ Authentication & Security

### JWT Settings
- `JWT_SECRET` - JWT secret key (compatibility variable)
- `JwtSettings__Secret` - JWT secret key
- `JwtSettings__Issuer` - JWT issuer
- `JwtSettings__Audience` - JWT audience
- `JwtSettings__ExpirationInMinutes` - JWT expiration time

### Google OAuth
- `GOOGLE_OAUTH_CLIENT_ID` - Google OAuth client ID (compatibility variable)
- `GOOGLE_OAUTH_CLIENT_SECRET` - Google OAuth client secret (compatibility variable)
- `GoogleOAuth__ClientId` - Google OAuth client ID
- `GoogleOAuth__ClientSecret` - Google OAuth client secret
- `GoogleOAuth__RedirectUri` - OAuth redirect URI (auto-generated if not provided)

## ✅ AI Configuration

### OpenAI
- `OPENAI_API_KEY` - OpenAI API key (compatibility variable)
- `OpenAI__ApiKey` - OpenAI API key
- `AI__OpenAI__ApiKey` - OpenAI API key (alternative format)
- `OPENAI_MODEL` - OpenAI model name (compatibility variable)
- `OpenAI__Model` - OpenAI model name
- `AI__OpenAI__Model` - OpenAI model name (alternative format)

## ✅ Frontend Configuration

- `FRONTEND_BASE_URL` - Frontend base URL (for CORS, email links, redirects)
- `Frontend__BaseUrl` - Frontend base URL (alternative format)

## ✅ Payment Processing (Stripe - Optional)

### Stripe Keys
- `Stripe__SecretKey` - Stripe secret key (optional)
- `Stripe__PublishableKey` - Stripe publishable key (optional)
- `Stripe__WebhookSecret` - Stripe webhook secret (optional)

### Stripe Price IDs
- `Stripe__PriceIds__Free__Monthly` - Free plan monthly price ID (optional)
- `Stripe__PriceIds__Free__Yearly` - Free plan yearly price ID (optional)
- `Stripe__PriceIds__Pro__Monthly` - Pro plan monthly price ID (optional)
- `Stripe__PriceIds__Pro__Yearly` - Pro plan yearly price ID (optional)
- `Stripe__PriceIds__Enterprise__Monthly` - Enterprise plan monthly price ID (optional)
- `Stripe__PriceIds__Enterprise__Yearly` - Enterprise plan yearly price ID (optional)

**Note:** Stripe variables are optional. If not configured, subscription features will be disabled.

## ✅ Security Settings (Optional - Has Defaults)

- `Security__MaxFailedLoginAttempts` - Maximum failed login attempts before lockout (default: 5)
- `Security__LockoutDurationMinutes` - Account lockout duration in minutes (default: 15)

**Note:** These variables are optional. If set to 0 or empty, defaults from `appsettings.json` will be used.

## 📋 Variable Priority

The application checks environment variables in the following order:
1. Environment variables (highest priority)
2. Configuration from `appsettings.json` (fallback)

## 🔧 Configuration Files

### Terraform Files
- `container_apps.tf` - Defines all environment variables
- `variables.tf` - Defines variable declarations
- `terraform.tfvars` - Contains actual values (DO NOT commit to version control)
- `terraform.tfvars.example` - Example configuration file

### Application Files
- `appsettings.json` - Local development configuration
- `appsettings.Production.json` - Production configuration template

## 🚀 Deployment

To update environment variables:

1. Edit `terraform.tfvars` with your values
2. Run `terraform plan` to preview changes
3. Run `terraform apply` to apply changes

The Container App will automatically restart with the new environment variables.

## 🔒 Security Notes

1. **Sensitive Variables**: Variables marked as `sensitive = true` in `variables.tf` will not be displayed in Terraform output
2. **Never Commit**: Never commit `terraform.tfvars` to version control - it contains secrets
3. **Key Rotation**: Regularly rotate API keys and secrets
4. **Azure Key Vault**: Consider using Azure Key Vault for sensitive values instead of direct environment variables

## 📝 Notes

- All Azure resource connection strings are automatically generated from Terraform resources
- Optional variables (Stripe, Security) can be left empty if not needed
- The application gracefully handles missing optional configuration
- Multiple variable name formats are supported for compatibility (e.g., `OPENAI_API_KEY`, `OpenAI__ApiKey`, `AI__OpenAI__ApiKey`)
