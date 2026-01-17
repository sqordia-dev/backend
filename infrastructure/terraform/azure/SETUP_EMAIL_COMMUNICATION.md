# Azure Communication Services Email Setup Guide

This guide walks you through setting up Azure Communication Services for email functionality in Sqordia.

## Overview

Azure Communication Services Email allows your application to send transactional emails (password resets, notifications, etc.) at a very low cost ($0.00025 per email).

## What's Included

The Terraform configuration creates:

1. **Azure Communication Service** - Main communication resource
2. **Email Communication Service** - Email-specific service
3. **Azure Managed Domain** - Free email domain (e.g., `DoNotReply@xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net`)
4. **Email Domain Association** - Links the email service to the communication service
5. **Key Vault Secrets** - Stores connection string and email settings securely

## Cost

- **Azure Managed Domain**: FREE
- **Sending Emails**: $0.00025 per email (1,000 emails = $0.25)
- **Total Estimated Cost**: ~$0.25-$2.00/month for typical usage

## Setup Steps

### 1. Deploy Infrastructure with Terraform

```bash
cd backend/infrastructure/terraform/azure

# Initialize Terraform (if not already done)
terraform init

# Review the changes (should show new Communication Services resources)
terraform plan

# Apply the changes
terraform apply
```

When prompted, type `yes` to create the resources.

### 2. Get the Connection String

After deployment, get the connection string:

```bash
# View all outputs
terraform output

# Or get specific values
terraform output communication_service_connection_string
terraform output email_from_address
```

### 3. Configure the EmailHandler Function

#### For Local Development:

1. Copy the example settings file:
   ```bash
   cd ../../src/Functions/EmailHandler
   cp local.settings.json.example local.settings.json
   ```

2. Edit `local.settings.json` and update:
   - `AzureCommunicationServices__ConnectionString`: Use the connection string from step 2
   - `Email__FromAddress`: Use the email address from step 2
   - `AzureServiceBus__ConnectionString`: Your Service Bus connection string

3. Run the function locally:
   ```bash
   func start
   ```

#### For Azure Deployment:

The Function App will automatically get the connection string from Key Vault (already configured in Terraform).

### 4. Test Email Sending

You can test the email functionality by:

1. **Using the API**: Send a request to create a user or trigger a password reset
2. **Manual Test**: Publish a test message to the Service Bus `email-jobs` topic

Example Service Bus message:
```json
{
  "jobId": "test-123",
  "emailType": "PasswordReset",
  "toEmail": "user@example.com",
  "toName": "Test User",
  "subject": "Test Email from Sqordia",
  "body": "This is a test email",
  "htmlBody": "<h1>Test Email</h1><p>This is a test email from Sqordia</p>"
}
```

### 5. Verify Email Sending

Check the Azure Portal:

1. Go to **Azure Portal** → **Communication Services** → `sqordia-production-email-communication`
2. Click on **Email** → **Domains** → Your Azure Managed Domain
3. Click on **Insights** to see email delivery metrics

## Custom Domain (Optional)

The Azure Managed Domain is free but has a generic address. For a custom domain (e.g., `noreply@sqordia.com`):

1. Purchase and verify your domain in Azure Communication Services
2. Update the Terraform variable:
   ```hcl
   email_from_address = "noreply@sqordia.com"
   ```
3. Re-apply Terraform:
   ```bash
   terraform apply
   ```

**Cost**: Custom domains are FREE, but you need to own the domain ($10-15/year from domain registrars).

## Troubleshooting

### Email Not Sending

1. **Check Function Logs**:
   ```bash
   # Local
   Check the console output when running func start

   # Azure
   az monitor app-insights events show --app <app-insights-name> --type traces
   ```

2. **Verify Connection String**: Ensure it starts with `endpoint=https://`

3. **Check Service Bus**: Ensure messages are being published to the `email-jobs` topic

4. **Domain Not Verified**: Azure Managed Domains are auto-verified. Custom domains need DNS verification.

### Rate Limiting

Azure Communication Services has limits:
- **Free Tier**: 100 emails/hour, 1,000 emails/day
- **Paid Tier**: Higher limits (contact Azure support)

If you hit limits, you'll see errors in the function logs.

### Email Marked as Spam

Azure Managed Domains may be flagged as spam initially. To improve deliverability:

1. Use a custom domain with proper SPF/DKIM records
2. Gradually increase send volume (don't send 1,000 emails immediately)
3. Include unsubscribe links in emails
4. Monitor bounce rates in Azure Portal

## Architecture

```
┌─────────────────┐
│   Sqordia API   │
└────────┬────────┘
         │ Publishes message
         ▼
┌─────────────────┐
│  Service Bus    │
│  (email-jobs)   │
└────────┬────────┘
         │ Triggers
         ▼
┌─────────────────┐
│ EmailHandler    │
│   Function      │
└────────┬────────┘
         │ Uses
         ▼
┌─────────────────┐
│ Communication   │
│   Services      │ ──► Sends email to recipient
└─────────────────┘
```

## Environment Variables

The EmailHandler function requires these environment variables:

| Variable | Description | Source |
|----------|-------------|--------|
| `AzureCommunicationServices__ConnectionString` | Connection string | Terraform output or Key Vault |
| `Email__FromAddress` | Sender email address | Terraform output or Key Vault |
| `Email__FromName` | Sender display name | Terraform output or Key Vault |
| `AzureServiceBus__ConnectionString` | Service Bus connection | Terraform output or Key Vault |
| `AzureServiceBus__EmailTopic` | Service Bus topic name | Terraform output |
| `AzureServiceBus__EmailSubscription` | Subscription name | Terraform output |

## Additional Resources

- [Azure Communication Services Email Documentation](https://learn.microsoft.com/azure/communication-services/concepts/email/email-overview)
- [Email Pricing](https://azure.microsoft.com/pricing/details/communication-services/)
- [Custom Domain Setup](https://learn.microsoft.com/azure/communication-services/quickstarts/email/add-custom-verified-domains)
- [Email Best Practices](https://learn.microsoft.com/azure/communication-services/concepts/email/email-best-practices)

## Next Steps

After email is working:

1. [ ] Test password reset flow end-to-end
2. [ ] Test user registration emails
3. [ ] Monitor email delivery metrics in Azure Portal
4. [ ] Set up alerts for failed emails
5. [ ] (Optional) Configure custom domain for production
