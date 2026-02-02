# Database Migrations Guide

This document explains how database migrations are executed in the Sqordia application.

## Overview

The application uses **Entity Framework Core** with **PostgreSQL** (Npgsql provider) for database management. Migrations can be run in several ways:

1. **Automatic on Application Startup** (Default)
2. **GitHub Actions Workflow** (Azure)
3. **Manual via EF Core CLI** (Local Development)
4. **Docker Container** (Local Development)

---

## 1. Automatic Migrations on Application Startup

**Location:** `src/WebAPI/Program.cs` and `src/WebAPI/Extensions/WebApplicationExtensions.cs`

### How It Works

When the application starts, it automatically applies pending migrations:

```csharp
// Program.cs line 72
await app.ApplyDatabaseMigrationsAsync();
```

The `ApplyDatabaseMigrationsAsync()` method:
- Checks if a connection string is configured
- Creates the database if it doesn't exist
- Applies all pending migrations using `db.Database.MigrateAsync()`
- Handles PostgreSQL-specific errors gracefully
- Loads critical settings after migrations complete

### When It Runs

- ✅ **Every time the application starts** (development, staging, production)
- ✅ **Automatically** - no manual intervention needed
- ✅ **Safe** - only applies pending migrations, won't break existing data

### Configuration

The connection string is read from:
1. Environment variable: `ConnectionStrings__DefaultConnection` (highest priority)
2. `appsettings.json` or `appsettings.{Environment}.json`
3. User Secrets (for local development)

### Error Handling

The migration process is **non-blocking**:
- If migrations fail, the application logs the error but continues to start
- PostgreSQL connection errors are caught and logged
- The application will run without database connectivity if needed

---

## 2. GitHub Actions Workflow (Azure)

### 2a. Deploy Pipeline (runs migrations before every production deploy)

**Location:** `.github/workflows/deploy-azure.yml`

On every push to `main`/`master`, the deploy workflow runs a **migrate-database** job after build-and-test and **before** deploying the new Docker image. Migrations use the production connection string from Azure Key Vault (secret `database-connection-string`) or, if missing, a constructed connection string from Key Vault credentials. This ensures the production database schema stays in sync with the application (e.g. new columns like `MicrosoftId` are applied before the new API version runs).

### 2b. Standalone Migration Workflow (manual or release branch)

**Location:** `.github/workflows/migrate-database-azure.yml`

This workflow runs migrations on demand or when migration files change on the release branch.

### Triggers

1. **Manual Trigger** (Workflow Dispatch):
   - Go to GitHub Actions → "Database Migrations (Azure)"
   - Click "Run workflow"
   - Select environment (production/staging)

2. **Automatic Trigger**:
   - When code is pushed to `release` branch
   - AND migration files are changed in `src/Infrastructure/Sqordia.Persistence/Migrations/**`

### Steps

1. **Authenticate to Azure** (OIDC or Client Secret)
2. **Get Connection String** from Azure Key Vault
3. **Run Migrations** using one of three methods (in order of preference):
   - **Method 1:** Execute via Container App (`az containerapp exec`)
   - **Method 2:** Run directly in GitHub Actions runner
   - **Method 3:** Construct connection string from server details

### Command Used

```bash
dotnet ef database update \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI \
  --connection "<connection-string>"
```

### When to Use

- ✅ **Before deploying new code** that requires schema changes
- ✅ **Separate from application deployment** for better control
- ✅ **Production environments** where you want explicit migration control

### Required GitHub Secrets

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_CLIENT_SECRET` (if not using OIDC)
- `AZURE_POSTGRESQL_ADMIN_USERNAME` (fallback)
- `AZURE_POSTGRESQL_ADMIN_PASSWORD` (fallback)

---

## 3. Manual via EF Core CLI (Local Development)

### Prerequisites

Install EF Core tools (if not already installed):

```powershell
dotnet tool install --global dotnet-ef
```

### Basic Commands

#### Apply All Pending Migrations

```powershell
dotnet ef database update \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

#### Apply Migrations with Custom Connection String

```powershell
dotnet ef database update \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI \
  --connection "Host=localhost;Port=5432;Database=SqordiaDb;Username=postgres;Password=postgres;"
```

#### Create a New Migration

```powershell
dotnet ef migrations add MigrationName \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

#### List All Migrations

```powershell
dotnet ef migrations list \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

#### Remove Last Migration (if not applied)

```powershell
dotnet ef migrations remove \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

#### Rollback to Specific Migration

```powershell
dotnet ef database update PreviousMigrationName \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

### Connection String Sources

The EF Core CLI reads connection strings from:
1. `--connection` parameter (highest priority)
2. `appsettings.json` or `appsettings.Development.json`
3. User Secrets (`dotnet user-secrets`)
4. Environment variables

### Using User Secrets (Recommended for Local)

```powershell
# Set connection string in user secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=SqordiaDb;Username=postgres;Password=postgres;"

# Now migrations will use this connection string
dotnet ef database update \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

---

## 4. Docker Container (Local Development)

### Using Docker Compose

If you're running the application via `docker-compose.yml`, migrations run automatically when the container starts (see Method 1).

### Manual Execution in Running Container

```powershell
# Get container ID
docker ps

# Execute migration command in container
docker exec -it <container-id> dotnet ef database update \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

### Using Docker Compose Exec

```powershell
docker-compose exec api dotnet ef database update \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

---

## Migration Workflow Best Practices

### Development

1. **Create Migration:**
   ```powershell
   dotnet ef migrations add AddNewFeature \
     --project src/Infrastructure/Sqordia.Persistence \
     --startup-project src/WebAPI
   ```

2. **Test Locally:**
   ```powershell
   dotnet ef database update \
     --project src/Infrastructure/Sqordia.Persistence \
     --startup-project src/WebAPI
   ```

3. **Commit Migration Files:**
   - Commit the migration files in `src/Infrastructure/Sqordia.Persistence/Migrations/`
   - Push to repository

### Production

1. **Review Migration:**
   - Check the migration SQL in the generated file
   - Test on staging environment first

2. **Run Migration Workflow:**
   - Go to GitHub Actions
   - Run "Database Migrations (Azure)" workflow
   - Select production environment

3. **Deploy Application:**
   - After migrations succeed, deploy the application
   - The application startup will verify migrations are applied

---

## Migration Files Location

```
src/Infrastructure/Sqordia.Persistence/
├── Migrations/
│   ├── 20240101000000_InitialCreate.cs
│   ├── 20240102000000_AddUserTable.cs
│   └── ApplicationDbContextModelSnapshot.cs
└── ApplicationDbContext.cs
```

**Important:** Always commit migration files to version control!

---

## Troubleshooting

### Error: "No connection string configured"

**Solution:** Set the connection string in:
- `appsettings.json`
- User Secrets
- Environment variable `ConnectionStrings__DefaultConnection`

### Error: "Database does not exist"

**Solution:** The automatic migration should create it, but if it fails:
```powershell
# Create database manually
psql -U postgres -c "CREATE DATABASE SqordiaDb;"

# Then run migrations
dotnet ef database update \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

### Error: "Migration already applied"

**Solution:** This is normal if the migration was already applied. Check migration status:
```powershell
dotnet ef migrations list \
  --project src/Infrastructure/Sqordia.Persistence \
  --startup-project src/WebAPI
```

### Error: "Cannot drop database because it is currently in use"

**Solution:** Close all connections to the database:
```sql
-- In PostgreSQL
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'SqordiaDb' AND pid <> pg_backend_pid();
```

### Azure: "Connection string not found in Key Vault"

**Solution:** Ensure the connection string secret exists:
```powershell
az keyvault secret set \
  --vault-name <key-vault-name> \
  --name "database-connection-string" \
  --value "<connection-string>"
```

---

## Summary

| Method | When to Use | Manual? | Environment |
|--------|-------------|---------|-------------|
| **Automatic on Startup** | Default behavior | ❌ No | All |
| **GitHub Actions** | Production deployments | ✅ Yes | Azure Production |
| **EF Core CLI** | Local development | ✅ Yes | Local |
| **Docker Container** | Local Docker setup | ✅ Optional | Local |

**Recommendation:** 
- **Local Development:** Let automatic migrations handle it, or use EF Core CLI for testing
- **Production:** Use GitHub Actions workflow for explicit control and audit trail

---

## Related Documentation

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Azure Database Migrations](./AZURE_GITHUB_ACTIONS.md)
- [Local Development Setup](./LOCAL_DEVELOPMENT_CONFIG.md)

