# PgAdmin Setup Guide for GCP Cloud SQL

Complete guide for connecting to GCP Cloud SQL PostgreSQL database using PgAdmin.

## Prerequisites

1. **Your IP address must be authorized** in Cloud SQL authorized networks (see [Authorizing Your IP](#authorizing-your-ip))
2. **PgAdmin installed** on your local machine ([Download pgAdmin](https://www.pgadmin.org/download/))
3. **Cloud SQL password** from GitHub Secrets or deployment configuration

## Quick Setup

### Step 1: Get Connection Information

Run the helper script to get all connection details:

```powershell
.\scripts\get-cloud-sql-connection-info.ps1
```

This will display:
- Host/Address (Public IP)
- Port
- Database name
- Username
- Connection status
- Your current IP and authorization status

### Step 2: Install PgAdmin

1. Download pgAdmin from https://www.pgadmin.org/download/
2. Install pgAdmin on your local machine
3. Launch pgAdmin

### Step 3: Create Server Connection

1. In pgAdmin, right-click **"Servers"** in the left panel
2. Select **"Create"** → **"Server"**

3. **General Tab:**
   - **Name**: `Sqordia GCP Production` (or any name you prefer)

4. **Connection Tab:**
   - **Host name/address**: [Get from script - Cloud SQL Public IP]
   - **Port**: `5432`
   - **Maintenance database**: `SqordiaDb`
   - **Username**: `sqordia_admin` (or `postgres` if sqordia_admin doesn't exist)
   - **Password**: [Your Cloud SQL password from GitHub Secrets: `CLOUD_SQL_PASSWORD`]
   - ✅ **Save password** (optional, for convenience)

5. **SSL Tab:**
   - **SSL mode**: Select **"Require"** from dropdown
   - This is **required** for Cloud SQL connections

6. **Advanced Tab** (optional):
   - **DB restriction**: Leave empty to see all databases
   - Or enter `SqordiaDb` to restrict to this database only

7. Click **"Save"**

### Step 4: Test Connection

1. pgAdmin will attempt to connect automatically
2. If successful, you'll see the server in the left panel
3. Expand the server to see databases, schemas, and tables

## Connection Details Reference

### Default Values

- **Host**: [Cloud SQL Public IP - get from script]
- **Port**: `5432`
- **Database**: `SqordiaDb`
- **Username**: `sqordia_admin` (preferred) or `postgres` (default)
- **Password**: From GitHub Secrets: `CLOUD_SQL_PASSWORD`
- **SSL Mode**: `Require`

### Connection String Format

```
Host=YOUR_CLOUD_SQL_IP;Port=5432;Database=SqordiaDb;Username=sqordia_admin;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

**Note**: If `sqordia_admin` user doesn't exist, use `postgres` as the username.

## Creating sqordia_admin User

If the `sqordia_admin` user doesn't exist, you can create it:

### Option 1: Using Helper Script (Recommended)

```powershell
.\scripts\create-sqordia-admin-user.ps1 -Password "YOUR_PASSWORD"
```

This script will:
- Check if the user exists
- Create the user if it doesn't exist
- Update the password if the user already exists

### Option 2: Using gcloud CLI

```powershell
gcloud sql users create sqordia_admin `
    --instance=sqordia-production-db `
    --project=project-b79ef08c-1eb8-47ea-80e `
    --password=YOUR_PASSWORD
```

### Verify Users

To see all database users:

```powershell
gcloud sql users list --instance=sqordia-production-db --project=project-b79ef08c-1eb8-47ea-80e
```

## Authorizing Your IP

Cloud SQL requires your IP address to be in the authorized networks list. To authorize your IP:

### Option 1: Using gcloud CLI (Recommended)

1. Get your current IP:
   ```powershell
   $myIp = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content
   Write-Host "Your IP: $myIp"
   ```

2. Authorize your IP:
   ```powershell
   gcloud sql instances patch sqordia-production-db `
       --project="project-b79ef08c-1eb8-47ea-80e" `
       --authorized-networks=$myIp/32 `
       --quiet
   ```

### Option 2: Using GCP Console

1. Go to **GCP Console** → **SQL** → **Instances**
2. Click on your instance: `sqordia-production-db`
3. Click **"EDIT"**
4. Scroll to **"Connections"** section
5. Under **"Authorized networks"**, click **"ADD NETWORK"**
6. Enter:
   - **Name**: `My IP` (or any name)
   - **Network**: `YOUR_IP/32` (replace YOUR_IP with your public IP)
7. Click **"DONE"** → **"SAVE"**
8. Wait for the update to complete (1-2 minutes)

## Troubleshooting

### Connection Timeout

**Problem**: Cannot connect to Cloud SQL from PgAdmin

**Solutions**:
1. Verify your IP is authorized:
   ```powershell
   .\scripts\get-cloud-sql-connection-info.ps1
   ```
   Check if your IP appears in authorized networks

2. Check Cloud SQL instance status:
   ```powershell
   gcloud sql instances describe sqordia-production-db --project=project-b79ef08c-1eb8-47ea-80e --format="value(state)"
   ```
   Should return `RUNNABLE`

3. Test network connectivity:
   ```powershell
   # Get the IP first
   $ip = gcloud sql instances describe sqordia-production-db --project=project-b79ef08c-1eb8-47ea-80e --format="value(ipAddresses[0].ipAddress)"
   Test-NetConnection -ComputerName $ip -Port 5432
   ```

4. Verify public IP is enabled:
   ```powershell
   gcloud sql instances describe sqordia-production-db --project=project-b79ef08c-1eb8-47ea-80e --format="value(settings.ipConfiguration.ipv4Enabled)"
   ```
   Should return `True`

### Authentication Failed

**Problem**: "FATAL: password authentication failed"

**Solutions**:
1. Verify username: Should be `sqordia_admin` (preferred) or `postgres` (default)
2. Check if user exists:
   ```powershell
   gcloud sql users list --instance=sqordia-production-db --project=project-b79ef08c-1eb8-47ea-80e
   ```
3. Create `sqordia_admin` user if it doesn't exist:
   ```powershell
   gcloud sql users create sqordia_admin `
       --instance=sqordia-production-db `
       --project=project-b79ef08c-1eb8-47ea-80e `
       --password=YOUR_PASSWORD
   ```
4. Verify password: Check GitHub Secrets `CLOUD_SQL_PASSWORD` or deployment configuration
5. Reset password if needed:
   ```powershell
   # For sqordia_admin
   gcloud sql users set-password sqordia_admin `
       --instance=sqordia-production-db `
       --project=project-b79ef08c-1eb8-47ea-80e `
       --password=NEW_PASSWORD
   
   # Or for postgres
   gcloud sql users set-password postgres `
       --instance=sqordia-production-db `
       --project=project-b79ef08c-1eb8-47ea-80e `
       --password=NEW_PASSWORD
   ```

### SSL Connection Required

**Problem**: "SSL connection is required"

**Solutions**:
1. In PgAdmin, go to **SSL tab** in server settings
2. Set **SSL mode** to **"Require"**
3. Save and reconnect

### Database Does Not Exist

**Problem**: Cannot find database `SqordiaDb`

**Solutions**:
1. Run database migrations:
   ```powershell
   .\scripts\run-db-migrations.ps1
   ```
2. Check if database exists:
   ```powershell
   .\scripts\check-migration-status.ps1
   ```

## Security Best Practices

1. **Limit IP Access**: Only authorize specific IP addresses
   - Use `/32` CIDR notation for single IP
   - Example: `1.2.3.4/32`
   - Remove IPs when no longer needed

2. **Use Strong Passwords**: Ensure Cloud SQL password is strong and stored securely in GitHub Secrets

3. **Use SSL**: Always require SSL connections (already configured)

4. **Rotate Passwords**: Regularly rotate Cloud SQL passwords

5. **Monitor Access**: Review Cloud SQL logs for connection attempts

6. **Use Private IP When Possible**: For applications within GCP, use private IP instead of public IP

## Verifying Connection

After connecting, verify the database structure:

1. Expand **Servers** → **Sqordia GCP Production** → **Databases** → **SqordiaDb** → **Schemas** → **public** → **Tables**
2. You should see application tables if migrations have run
3. Check `__EFMigrationsHistory` table to see applied migrations

## Alternative: Using psql Command Line

If you prefer command line:

```powershell
# Get the Cloud SQL IP first
$ip = gcloud sql instances describe sqordia-production-db --project=project-b79ef08c-1eb8-47ea-80e --format="value(ipAddresses[0].ipAddress)"

# Set password as environment variable
$env:PGPASSWORD = "YOUR_PASSWORD"

# Connect (using sqordia_admin)
psql -h $ip `
     -p 5432 `
     -U sqordia_admin `
     -d SqordiaDb `
     --set=sslmode=require

# Or using postgres if sqordia_admin doesn't exist
psql -h $ip `
     -p 5432 `
     -U postgres `
     -d SqordiaDb `
     --set=sslmode=require
```

## Quick Reference

| Setting | Value |
|---------|-------|
| Host | [Get from script] |
| Port | `5432` |
| Database | `SqordiaDb` |
| Username | `sqordia_admin` (or `postgres`) |
| Password | From GitHub Secrets |
| SSL Mode | `Require` |

## Next Steps

1. ✅ Run `.\scripts\get-cloud-sql-connection-info.ps1` to get connection details
2. ✅ Authorize your IP address if needed
3. ✅ Connect to Cloud SQL using PgAdmin
4. ✅ Verify database tables exist
5. ✅ Run migrations if needed via GitHub Actions workflow

## Related Documentation

- [GCP Database Connection Guide](GCP_DATABASE_CONNECTION.md) - Detailed GCP connection information
- [Migration Guide](MIGRATION_GUIDE.md) - Running database migrations
- [GCP Deployment Guide](GCP_COMPLETION_SUMMARY.md) - GCP deployment overview

