# Quick Start: Email Communication Setup

## What Was Configured

The setup for `sqordia-email-communication` includes:

1. **Azure Communication Service** - Main service for email
2. **Email Communication Service** - Email-specific functionality
3. **Azure Managed Domain** - Free fallback domain (auto-configured)
4. **Custom Domain** - `sqordia.app` for professional emails from `donotreply@sqordia.app`
5. **Key Vault Integration** - Connection strings stored securely
6. **EmailHandler Function** - Azure Function to process email jobs

## Next Steps

### 1. Deploy the Infrastructure

```bash
cd backend/infrastructure/terraform/azure

# Initialize and plan
terraform init
terraform plan

# Deploy (creates Communication Services + Custom Domain)
terraform apply
```

### 2. Verify Custom Domain in Azure

After deployment, you need to verify domain ownership:

```bash
# Get DNS verification records
terraform output email_domain_verification_records
```

This will show DNS TXT records you need to add to your domain provider (e.g., Cloudflare, GoDaddy).

**Example Output:**
```
{
  "domain_verification" = {
    "Domain" = {
      "type" = "TXT"
      "name" = "@"
      "value" = "ms-domain-verification=abc123..."
    }
  }
  "note" = "Add these DNS TXT records to your domain provider to verify ownership"
}
```

### 3. Add DNS Records

Go to your DNS provider (where you manage `sqordia.app`) and add the TXT records shown above.

**Example for Cloudflare:**
1. Log in to Cloudflare
2. Select `sqordia.app` domain
3. Go to DNS → Records
4. Click "Add record"
5. Type: TXT
6. Name: @ (or leave blank)
7. Content: (paste the verification value)
8. Click "Save"

### 4. Verify in Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **Communication Services** → `sqordia-production-email`
3. Click **Provision domains** → **sqordia.app**
4. Click **Verify** (may take up to 24 hours for DNS propagation)

### 5. Configure Email Handler Function

#### Local Development:

```bash
cd backend/src/Functions/EmailHandler

# Copy the example settings
cp local.settings.json.example local.settings.json

# Edit local.settings.json with your values
# Get connection string from: terraform output -raw communication_service_connection_string
```

Update these values in `local.settings.json`:
```json
{
  "AzureCommunicationServices__ConnectionString": "endpoint=https://...",
  "Email__FromAddress": "donotreply@sqordia.app",
  "Email__FromName": "Sqordia"
}
```

#### Azure Production:

Already configured! The function app gets values from Key Vault automatically.

### 6. Test Email Sending

**Option A: Through API**
```bash
# Trigger a password reset or user registration
curl -X POST https://your-api.azurecontainerapps.io/api/v1/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com"}'
```

**Option B: Direct Test (Service Bus)**

Publish a test message to the `email-jobs` Service Bus topic:

```json
{
  "jobId": "test-123",
  "emailType": "Test",
  "toEmail": "your-email@example.com",
  "toName": "Test User",
  "subject": "Test Email from Sqordia",
  "body": "This is a test email",
  "htmlBody": "<h1>Test Email</h1><p>This is a test email from Sqordia</p>"
}
```

## Configuration Files Updated

- ✅ `communication_services.tf` - New Terraform resource
- ✅ `variables.tf` - Added email configuration variables
- ✅ `outputs.tf` - Added Communication Services outputs
- ✅ `terraform.tfvars` - Set `donotreply@sqordia.app`
- ✅ `terraform.tfvars.example` - Updated example
- ✅ `local.settings.json.example` - EmailHandler configuration template

## Important Notes

### Custom Domain vs Azure Managed Domain

| Feature | Azure Managed Domain | Custom Domain (`sqordia.app`) |
|---------|---------------------|-------------------------------|
| **Cost** | FREE | FREE (just need to own domain) |
| **Email Address** | `DoNotReply@[random-guid].azurecomm.net` | `donotreply@sqordia.app` |
| **Setup** | Automatic | Requires DNS verification |
| **Professional** | ❌ No | ✅ Yes |
| **Spam Score** | Medium | Better (with proper SPF/DKIM) |

### DNS Propagation

After adding DNS records:
- **Minimum**: 5-10 minutes
- **Maximum**: 24-48 hours
- **Typical**: 1-2 hours

### Email Sending Limits

- **Free Tier**: 100 emails/hour, 1,000 emails/day
- **Cost**: $0.00025 per email (1,000 emails = $0.25)

### SPF and DKIM Records

Azure Communication Services automatically manages SPF and DKIM for sending:
- No additional configuration needed
- Records are added when you verify the domain

## Troubleshooting

### Domain Verification Fails

1. **Wait longer** - DNS can take up to 48 hours
2. **Check DNS propagation**: Use [whatsmydns.net](https://www.whatsmydns.net/)
3. **Verify TXT record**: Ensure exact value matches (no extra spaces)

### Emails Not Sending

1. **Check domain verification status** in Azure Portal
2. **Verify connection string** is correct
3. **Check Service Bus** - messages arriving in topic?
4. **Function logs** - any errors?

### Emails Go to Spam

1. **Start slow** - Don't send 1,000 emails immediately
2. **Gradual ramp-up** - Increase volume over days/weeks
3. **Good content** - Include unsubscribe links, avoid spam words
4. **Monitor** - Check bounce rates in Azure Portal

## Get Connection String

```bash
# Show connection string (sensitive - don't share!)
terraform output -raw communication_service_connection_string

# Show email configuration
terraform output email_from_address
terraform output email_from_name
```

## Useful Commands

```bash
# View all outputs
terraform output

# Refresh state
terraform refresh

# View DNS records to add
terraform output email_domain_verification_records

# Check Service Bus messages
az servicebus topic subscription show \
  --resource-group sqordia-production-rg \
  --namespace-name sqordia-production-sb \
  --topic-name email-jobs \
  --name email-handler
```

## Cost Estimate

- **Azure Communication Services**: $0 (no base cost)
- **Email sending**: $0.00025 per email
- **Monthly estimate**: $0.25 - $2.00 (1,000-8,000 emails)
- **Domain ownership**: $10-15/year (external cost)

## Resources

- [Setup Guide](./SETUP_EMAIL_COMMUNICATION.md) - Detailed setup instructions
- [Azure Portal](https://portal.azure.com) - Manage resources
- [Communication Services Docs](https://learn.microsoft.com/azure/communication-services/concepts/email/email-overview)
