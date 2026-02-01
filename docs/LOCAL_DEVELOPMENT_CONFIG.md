# Local Development Configuration Guide

This document explains the `appsettings.json` configuration for local development.

## Configuration Overview

The `appsettings.json` file is used for **local development only**. This file is in `.gitignore`, so it's safe to store your local development secrets here.

### Security Model

- **Local Development**: Store secrets directly in `appsettings.json` (this file is ignored by git)
- **Production**: Use environment variables or Azure Key Vault (configured via Terraform)

**Important**: Never commit `appsettings.json` with real secrets. The file is already in `.gitignore`, but always verify before committing.

## Configuration Priority

Configuration is read in this order (highest to lowest priority):

1. **Environment Variables** (highest priority)
2. `appsettings.Production.json` (when `ASPNETCORE_ENVIRONMENT=Production`)
3. `appsettings.json` (base configuration - used for local development)

## Required Settings for Local Development

### 1. Database Connection

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=SqordiaDb;Username=postgres;Password=postgres;SSL Mode=Prefer;Trust Server Certificate=true"
}
```

**Setup:**
- Start PostgreSQL using Docker: `docker-compose up sqordia-db -d`
- Or use a local PostgreSQL installation
- Default credentials: `postgres/postgres`

### 2. JWT Settings

```json
"JwtSettings": {
  "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity123!",
  "Issuer": "Sqordia",
  "Audience": "SqordiaUsers",
  "ExpirationInMinutes": 60
}
```

**Note:** Change the secret for production! For local dev, this default is fine.

### 3. Google OAuth (Optional)

```json
"GoogleOAuth": {
  "ClientId": "295510443982-46f8941gu47m03j7ka80husou7dndlug.apps.googleusercontent.com",
  "ClientSecret": "",
  "RedirectUri": "http://localhost:5241/api/v1/auth/google/callback"
}
```

**Setup:**
- Set `ClientSecret` via environment variable: `GOOGLE_OAUTH_CLIENT_SECRET`
- Or use User Secrets: `dotnet user-secrets set "GoogleOAuth:ClientSecret" "your-secret"`

## Optional Settings

### AI Providers

For local development, you can set AI API keys directly in `appsettings.json`:

```json
"AI": {
  "OpenAI": {
    "ApiKey": "sk-proj-your-key-here",
    "Model": "gpt-4o"
  },
  "Claude": {
    "ApiKey": "your-claude-key"
  },
  "Gemini": {
    "ApiKey": "your-gemini-key"
  }
}
```

**Alternative methods** (environment variables take precedence):

1. **Environment Variables**:
```bash
export OPENAI_API_KEY=your-key
export CLAUDE_API_KEY=your-key
export GEMINI_API_KEY=your-key
```

2. **User Secrets** (for .NET):
```bash
dotnet user-secrets set "AI:OpenAI:ApiKey" "your-key"
```

### Azure Services (Local Development)

For local development, Azure services are **automatically disabled** when connection strings are empty:

```json
"AzureStorage": {
  "AccountName": "",
  "ConnectionString": "",
  "ContainerName": "documents"
},
"AzureServiceBus": {
  "ConnectionString": "",
  "EmailTopic": "sqordia-production-email",
  "AiGenerationTopic": "sqordia-production-ai-generation",
  "ExportTopic": "sqordia-production-export"
}
```

**Behavior:**
- **Empty ConnectionString** → Uses in-memory storage service
- **Empty ServiceBus ConnectionString** → Uses mock email service
- **Empty KeyVault URL** → Not used (secrets read from config/env vars)

To force use of mock services even if Azure config is present:
```json
"SkipAzureServices": true
```

### GCP Services (Backward Compatibility)

GCP settings are kept for backward compatibility but are not used when Azure is configured:

```json
"GCP": {
  "ProjectId": "project-b79ef08c-1eb8-47ea-80e"
},
"PubSub": {
  "EmailTopic": "email-topic"
},
"CloudStorage": {
  "BucketName": "sqordia-documents"
}
```

## Environment Variables for Local Development

You can override any setting using environment variables:

```bash
# Database
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=SqordiaDb;Username=postgres;Password=mypassword;"

# JWT
export JWT_SECRET="your-secret-key-here"

# Google OAuth
export GOOGLE_OAUTH_CLIENT_SECRET="your-google-secret"

# AI Providers
export OPENAI_API_KEY="sk-..."
export CLAUDE_API_KEY="sk-ant-..."
export GEMINI_API_KEY="..."

# Force skip Azure services
export SKIP_AZURE_SERVICES="true"
```

## Using .NET User Secrets (Recommended for Sensitive Data)

For sensitive configuration like API keys, use User Secrets:

```bash
# Initialize (one time)
dotnet user-secrets init --project src/WebAPI

# Set secrets
dotnet user-secrets set "GoogleOAuth:ClientSecret" "your-secret" --project src/WebAPI
dotnet user-secrets set "AI:OpenAI:ApiKey" "sk-..." --project src/WebAPI
dotnet user-secrets set "AI:Claude:ApiKey" "sk-ant-..." --project src/WebAPI
dotnet user-secrets set "AI:Gemini:ApiKey" "..." --project src/WebAPI
```

**Note:** User Secrets are stored in `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json` on Windows and are **never committed** to version control.

## Quick Start Checklist

1. ✅ **PostgreSQL Running**
   ```bash
   docker-compose up sqordia-db -d
   ```

2. ✅ **Database Migrations**
   ```bash
   dotnet ef database update --project src/Infrastructure/Sqordia.Persistence
   ```

3. ✅ **Optional: Set User Secrets**
   ```bash
   dotnet user-secrets init --project src/WebAPI
   dotnet user-secrets set "GoogleOAuth:ClientSecret" "your-secret" --project src/WebAPI
   ```

4. ✅ **Run Application**
   ```bash
   dotnet run --project src/WebAPI
   ```

5. ✅ **Access Application**
   - API: http://localhost:5241
   - Swagger: http://localhost:5241/swagger
   - Health: http://localhost:5241/health

## Service Behavior in Local Development

| Service | Configuration | Behavior |
|---------|--------------|----------|
| **Database** | Connection string set | ✅ Uses PostgreSQL |
| **Storage** | Azure connection string empty | ✅ Uses in-memory storage |
| **Email** | Service Bus connection string empty | ✅ Uses mock email service |
| **AI** | API keys empty | ⚠️ AI features disabled (no errors) |
| **Google OAuth** | ClientSecret empty | ⚠️ Google login disabled |

## Troubleshooting

### Application won't start
- Check PostgreSQL is running: `docker ps`
- Verify connection string matches your database
- Check logs: `logs/sqordia-*.log`

### Can't connect to database
- Verify PostgreSQL is accessible: `psql -h localhost -U postgres -d SqordiaDb`
- Check port 5432 is not in use
- Verify password matches connection string

### AI features not working
- Set API keys via environment variables or User Secrets
- Check logs for API key errors

### Email not sending
- Expected behavior in local dev (uses mock service)
- Check logs to see email content
- To test real emails, configure Azure Service Bus connection string

## Next Steps

- See [DEVELOPER_SETUP.md](./DEVELOPER_SETUP.md) for complete setup instructions
- See [AZURE_APPSETTINGS_UPDATE.md](./AZURE_APPSETTINGS_UPDATE.md) for Azure production configuration

