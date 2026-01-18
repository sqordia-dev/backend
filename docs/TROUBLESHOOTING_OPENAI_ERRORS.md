# Troubleshooting OpenAI API Errors

## Error: "Failed to generate content after 3 attempts"

This error occurs when the OpenAI API calls fail repeatedly. Here's how to diagnose and fix it.

## Common Causes

### 1. API Key Not Configured in Deployment

**Symptoms:**
- Error: "Failed to generate content after 3 attempts"
- Logs show: "OpenAI service not configured" or "OpenAI API key not configured"

**Solution:**
1. Verify environment variables are set in Azure Container App:
   ```bash
   # Check if variables are set
   az containerapp show \
     --name sqordia-production-api \
     --resource-group sqordia-production-rg \
     --query "properties.template.containers[0].env"
   ```

2. Apply Terraform changes:
   ```bash
   cd backend/infrastructure/terraform/azure
   terraform plan
   terraform apply
   ```

3. Restart the Container App to pick up new environment variables:
   ```bash
   az containerapp revision restart \
     --name sqordia-production-api \
     --resource-group sqordia-production-rg
   ```

### 2. Invalid or Expired API Key

**Symptoms:**
- Error: "401 Unauthorized" or "invalid_api_key"
- Logs show authentication failures

**Solution:**
1. Verify the API key is valid:
   - Go to https://platform.openai.com/account/api-keys
   - Check if the key is active and not revoked
   - Ensure the key has access to the model you're using (gpt-4o)

2. Update the API key in `terraform.tfvars`:
   ```hcl
   openai_api_key = "sk-proj-..." # Your new API key
   ```

3. Apply and restart (see above)

### 3. Rate Limiting

**Symptoms:**
- Error: "429 Too Many Requests" or "rate limit"
- Intermittent failures

**Solution:**
1. Check your OpenAI usage limits:
   - Go to https://platform.openai.com/account/usage
   - Check if you've exceeded rate limits

2. Options:
   - Upgrade your OpenAI plan
   - Reduce request frequency
   - Implement better retry logic with longer delays

### 4. Quota Exceeded

**Symptoms:**
- Error: "quota exceeded" or "billing"
- All requests fail

**Solution:**
1. Check billing and usage:
   - Go to https://platform.openai.com/account/billing
   - Ensure you have available credits/quota

2. Add payment method if needed

### 5. Model Not Available

**Symptoms:**
- Error: "model not found" or "model unavailable"
- Specific to model name

**Solution:**
1. Verify the model name in `terraform.tfvars`:
   ```hcl
   openai_model = "gpt-4o"  # Ensure this model exists
   ```

2. Check available models:
   - Go to https://platform.openai.com/docs/models
   - Verify the model is available for your account

## Debugging Steps

### 1. Check Container App Logs

```bash
# View recent logs
az containerapp logs show \
  --name sqordia-production-api \
  --resource-group sqordia-production-rg \
  --tail 100 \
  --follow
```

Look for:
- "OpenAI Settings - ApiKey configured: true/false"
- "OpenAI service initialized successfully"
- "Error generating content with OpenAI"
- Specific error messages (401, 429, quota, etc.)

### 2. Verify Environment Variables

```bash
# List all environment variables
az containerapp show \
  --name sqordia-production-api \
  --resource-group sqordia-production-rg \
  --query "properties.template.containers[0].env[*].{name:name,value:value}" \
  --output table
```

Ensure these are set:
- `OPENAI_API_KEY`
- `OpenAI__ApiKey`
- `AI__OpenAI__ApiKey`
- `OPENAI_MODEL`
- `OpenAI__Model`
- `AI__OpenAI__Model`

### 3. Test API Key Locally

```bash
# Test the API key directly
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer YOUR_API_KEY"
```

### 4. Check Application Startup Logs

Look for these log messages on startup:
```
OpenAI Settings - ApiKey configured: True, Model: gpt-4o, ApiKey length: 51
Initializing OpenAI client with model: gpt-4o
OpenAI service initialized successfully with model: gpt-4o
```

If you see warnings instead, the API key is not being read correctly.

## Environment Variable Priority

The application checks environment variables in this order:
1. `OPENAI_API_KEY` (highest priority)
2. `OpenAI__ApiKey`
3. `AI__OpenAI__ApiKey`
4. Configuration from `appsettings.json`

## Updated Error Messages

After the recent improvements, error messages now include:
- Exception type
- Detailed error message
- Inner exception details (if available)
- Specific guidance for common errors (401, 429, quota)

Check the application logs for these detailed messages to identify the exact issue.

## Quick Fix Checklist

- [ ] Environment variables set in Terraform `terraform.tfvars`
- [ ] Terraform changes applied (`terraform apply`)
- [ ] Container App restarted after Terraform apply
- [ ] API key is valid and active
- [ ] API key has access to the model (gpt-4o)
- [ ] No rate limiting or quota issues
- [ ] Check application logs for detailed error messages

## Need More Help?

1. Check Azure Container App logs for detailed error messages
2. Verify environment variables are correctly set
3. Test the API key directly with OpenAI's API
4. Check OpenAI account status and billing
