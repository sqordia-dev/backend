# Connecting to GCP Cloud SQL with PgAdmin or DBeaver

Complete guide for connecting to GCP Cloud SQL PostgreSQL database using PgAdmin or DBeaver.

## Connection Details

**Host/Server:** `34.130.197.191`  
**Port:** `5432`  
**Database:** `SqordiaDb`  
**Username:** `sqordia_admin`  
**Password:** (From `infrastructure/terraform/gcp/terraform.tfvars` - `cloud_sql_password`)  
**SSL Mode:** `Require`

## Prerequisites

1. **Your IP address must be authorized** in Cloud SQL firewall rules
2. **PgAdmin or DBeaver installed** on your local machine
3. **Password from terraform.tfvars**

### Check Your IP Authorization

Your IP (`24.200.142.208`) is already authorized. To add additional IPs:

```powershell
# Get your current IP
$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content
Write-Host "Your IP: $myIp"

# Add to Cloud SQL authorized networks
gcloud sql instances patch sqordia-production-db `
    --project="project-b79ef08c-1eb8-47ea-80e" `
    --authorized-networks=$myIp/32 `
    --quiet
```

## Option 1: Using PgAdmin

### Step 1: Install PgAdmin

1. Download pgAdmin from https://www.pgadmin.org/download/
2. Install pgAdmin on your local machine
3. Launch pgAdmin

### Step 2: Create Server Connection

1. In pgAdmin, right-click **"Servers"** in the left panel
2. Select **"Create"** → **"Server"**

3. **General Tab:**
   - **Name**: `Sqordia GCP Production` (or any name you prefer)

4. **Connection Tab:**
   - **Host name/address**: `34.130.197.191`
   - **Port**: `5432`
   - **Maintenance database**: `SqordiaDb`
   - **Username**: `sqordia_admin`
   - **Password**: [Your Cloud SQL password from terraform.tfvars]
   - **Save password**: ✅ (optional, for convenience)

5. **SSL Tab:**
   - **SSL mode**: Select **"Require"** from dropdown
   - This is **required** for Cloud SQL connections

6. **Advanced Tab** (optional):
   - **DB restriction**: Leave empty to see all databases
   - Or enter `SqordiaDb` to restrict to this database only

7. Click **"Save"**

### Step 3: Test Connection

1. pgAdmin will attempt to connect automatically
2. If successful, you'll see the server in the left panel
3. Expand the server to see databases, schemas, and tables

## Option 2: Using DBeaver

### Step 1: Install DBeaver

1. Download DBeaver from https://dbeaver.io/download/
2. Install DBeaver on your local machine
3. Launch DBeaver

### Step 2: Create Database Connection

1. Click **"New Database Connection"** (plug icon) or press `Ctrl+Shift+N`
2. Select **PostgreSQL** from the database list
3. Click **"Next"**

4. **Main Tab:**
   - **Host**: `34.130.197.191`
   - **Port**: `5432`
   - **Database**: `SqordiaDb`
   - **Username**: `sqordia_admin`
   - **Password**: [Your Cloud SQL password from terraform.tfvars]
   - ✅ **Save password** (optional)

5. **SSL Tab:**
   - ✅ **Use SSL**
   - **SSL Mode**: Select **"require"** from dropdown
   - ✅ **Allow invalid hostname** (Cloud SQL uses IP, not hostname)

6. Click **"Test Connection"** to verify
7. If successful, click **"Finish"**

### Step 3: Connect

1. The connection will appear in the Database Navigator
2. Double-click to connect
3. Expand to see schemas, tables, and data

## Getting Your Password

If you need to retrieve the password:

```powershell
# Read from terraform.tfvars
cd infrastructure\terraform\gcp
$tfvars = Get-Content terraform.tfvars -Raw
if ($tfvars -match 'cloud_sql_password\s*=\s*"([^"]+)"') {
    Write-Host "Password: $($matches[1])" -ForegroundColor Yellow
} else {
    Write-Host "Password not found in terraform.tfvars" -ForegroundColor Red
}
```

## Connection String Format

For reference, here's the connection string format:

```
Host=34.130.197.191;Port=5432;Database=SqordiaDb;Username=sqordia_admin;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

## Troubleshooting

### Connection Timeout

**Problem**: Cannot connect to Cloud SQL

**Solutions**:
1. Verify your IP is authorized:
   ```powershell
   gcloud sql instances describe sqordia-production-db `
       --project="project-b79ef08c-1eb8-47ea-80e" `
       --format="value(settings.ipConfiguration.authorizedNetworks)"
   ```

2. Add your IP if not listed:
   ```powershell
   $myIp = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content
   gcloud sql instances patch sqordia-production-db `
       --project="project-b79ef08c-1eb8-47ea-80e" `
       --authorized-networks=$myIp/32 `
       --quiet
   ```

3. Test network connectivity:
   ```powershell
   Test-NetConnection -ComputerName 34.130.197.191 -Port 5432
   ```

### Authentication Failed

**Problem**: "FATAL: password authentication failed"

**Solutions**:
1. Verify username: Should be `sqordia_admin`
2. Verify password: Check `terraform.tfvars` file
3. Reset password if needed (via Terraform or GCP Console)

### SSL Connection Required

**Problem**: "SSL connection is required" or "SSL error"

**Solutions**:
1. **PgAdmin**: Go to **SSL tab** → Set **SSL mode** to **"Require"**
2. **DBeaver**: Go to **SSL tab** → ✅ **Use SSL** → Set **SSL Mode** to **"require"**
3. Save and reconnect

### Database Does Not Exist

**Problem**: Cannot find database `SqordiaDb`

**Solutions**:
1. Verify database exists:
   ```powershell
   gcloud sql databases list `
       --instance=sqordia-production-db `
       --project="project-b79ef08c-1eb8-47ea-80e"
   ```

2. Run database migrations if needed:
   ```powershell
   .\scripts\run-gcp-migrations.ps1 -ProjectId "project-b79ef08c-1eb8-47ea-80e" -Region "northamerica-northeast2" -UseDirectConnection -Username "sqordia_admin"
   ```

## Verifying Connection

After connecting, verify the database structure:

1. **PgAdmin**: Expand **Servers** → **Sqordia GCP Production** → **Databases** → **SqordiaDb** → **Schemas** → **public** → **Tables**
2. **DBeaver**: Expand **SqordiaDb** → **Schemas** → **public** → **Tables**

You should see:
- Application tables (if migrations have run)
- `__EFMigrationsHistory` table showing applied migrations

## Security Best Practices

1. **Limit IP Access**: Only authorize specific IP addresses
   - Use `/32` CIDR notation for single IP
   - Example: `24.200.142.208/32`

2. **Use Strong Passwords**: Ensure Cloud SQL password is strong and stored securely in `terraform.tfvars`

3. **Use SSL**: Always require SSL connections (already configured)

4. **Rotate Passwords**: Regularly rotate Cloud SQL passwords

5. **Monitor Access**: Review Cloud SQL logs for connection attempts

6. **Remove Unused IPs**: Remove IP addresses from authorized networks when no longer needed

## Alternative: Using psql Command Line

If you prefer command line:

```powershell
# Set password as environment variable
$env:PGPASSWORD = "YOUR_PASSWORD"

# Connect
psql -h 34.130.197.191 `
     -p 5432 `
     -U sqordia_admin `
     -d SqordiaDb `
     --set=sslmode=require
```

## Quick Reference

| Setting | Value |
|---------|-------|
| Host | `34.130.197.191` |
| Port | `5432` |
| Database | `SqordiaDb` |
| Username | `sqordia_admin` |
| Password | From `terraform.tfvars` |
| SSL Mode | `Require` |

## Related Documentation

- [GCP Completion Summary](./GCP_COMPLETION_SUMMARY.md) - Deployment overview
- [GCP Migration Status](./GCP_MIGRATION_STATUS.md) - Migration progress
- [Database Migration Guide](./MIGRATION_GUIDE.md) - Running migrations

