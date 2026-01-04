# PgAdmin Setup Guide

Complete guide for connecting to RDS PostgreSQL database using PgAdmin.

## Prerequisites

1. **RDS must be publicly accessible** (see [Enabling Public Access](#enabling-public-access))
2. **Your IP address must be allowed** in the RDS security group
3. **PgAdmin installed** on your local machine ([Download pgAdmin](https://www.pgadmin.org/download/))

## Quick Setup

### Step 1: Get Connection Information

Run the helper script to get all connection details:

```powershell
.\scripts\get-rds-connection-info.ps1
```

This will display:
- Host/Address
- Port
- Database name
- Username
- Password (if available)

### Step 2: Install PgAdmin

1. Download pgAdmin from https://www.pgadmin.org/download/
2. Install pgAdmin on your local machine
3. Launch pgAdmin

### Step 3: Create Server Connection

1. In pgAdmin, right-click **"Servers"** in the left panel
2. Select **"Create"** → **"Server"**

3. **General Tab:**
   - **Name**: `Sqordia Production` (or any name you prefer)

4. **Connection Tab:**
   - **Host name/address**: `sqordia-db-production.c326icw2wbit.ca-central-1.rds.amazonaws.com`
   - **Port**: `5432`
   - **Maintenance database**: `SqordiaDb`
   - **Username**: `sqordia_admin`
   - **Password**: [Your RDS password - set in `TF_VAR_rds_password`]

5. **SSL Tab:**
   - **SSL mode**: Select **"Require"** from dropdown
   - This is **required** for RDS connections

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

- **Host**: `sqordia-db-production.c326icw2wbit.ca-central-1.rds.amazonaws.com`
- **Port**: `5432`
- **Database**: `SqordiaDb`
- **Username**: `sqordia_admin`
- **Password**: Set via `TF_VAR_rds_password` environment variable or Terraform

### Connection String Format

```
Host=sqordia-db-production.c326icw2wbit.ca-central-1.rds.amazonaws.com;Port=5432;Database=SqordiaDb;Username=sqordia_admin;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

## Enabling Public Access

By default, RDS is in a private subnet and not publicly accessible. To enable PgAdmin access:

### Option 1: Using Terraform Variables (Recommended)

1. Edit `infrastructure/terraform/terraform.tfvars`:

```hcl
rds_publicly_accessible = true
rds_allowed_ip_addresses = ["YOUR_IP_ADDRESS/32"]
```

   Replace `YOUR_IP_ADDRESS` with your public IP. To find your IP:
   ```powershell
   (Invoke-WebRequest -Uri "https://api.ipify.org").Content
   ```

2. Apply Terraform changes:

```powershell
cd infrastructure/terraform
terraform plan
terraform apply
```

3. Wait for RDS modification to complete (5-10 minutes)

### Option 2: Using AWS Console

1. Go to **AWS Console** → **RDS** → **Databases**
2. Select your RDS instance (`sqordia-db-production`)
3. Click **"Modify"**
4. Scroll to **"Connectivity"** section
5. Expand **"Additional connectivity configuration"**
6. Check **"Publicly accessible"**
7. Click **"Continue"** → **"Apply immediately"**

8. Update Security Group:
   - Go to **RDS** → **Security** → Click on the security group
   - Click **"Edit inbound rules"**
   - Add rule:
     - **Type**: PostgreSQL
     - **Port**: 5432
     - **Source**: Your IP address (e.g., `1.2.3.4/32`)
   - Click **"Save rules"**

## Troubleshooting

### Connection Timeout

**Problem**: Cannot connect to RDS from PgAdmin

**Solutions**:
1. Verify RDS is publicly accessible:
   ```powershell
   aws rds describe-db-instances --db-instance-identifier sqordia-db-production --query 'DBInstances[0].PubliclyAccessible'
   ```
   Should return `true`

2. Check security group rules:
   - Ensure your IP is allowed in the security group
   - Verify port 5432 is open

3. Check RDS status:
   - RDS must be in `available` state (not `modifying` or `backing-up`)

4. Test network connectivity:
   ```powershell
   Test-NetConnection -ComputerName sqordia-db-production.c326icw2wbit.ca-central-1.rds.amazonaws.com -Port 5432
   ```

### Authentication Failed

**Problem**: "FATAL: password authentication failed"

**Solutions**:
1. Verify username: Should be `sqordia_admin`
2. Verify password: Check `TF_VAR_rds_password` environment variable
3. Reset password if needed (via AWS Console or Terraform)

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

1. **Limit IP Access**: Only allow your specific IP address in security group**
   - Use `/32` CIDR notation for single IP
   - Example: `["1.2.3.4/32"]`

2. **Use Strong Passwords**: Ensure RDS password is strong and stored securely

3. **Disable Public Access When Not Needed**: 
   - Set `rds_publicly_accessible = false` when not using PgAdmin
   - Re-enable only when needed

4. **Use SSL**: Always require SSL connections (already configured)

5. **Rotate Passwords**: Regularly rotate RDS passwords

6. **Monitor Access**: Review CloudWatch logs for connection attempts

## Verifying Connection

After connecting, verify the database structure:

1. Expand **Servers** → **Sqordia Production** → **Databases** → **SqordiaDb** → **Schemas** → **public** → **Tables**
2. You should see application tables if migrations have run
3. Check `__EFMigrationsHistory` table to see applied migrations

## Alternative: Using psql Command Line

If you prefer command line:

```powershell
# Set password as environment variable
$env:PGPASSWORD = "YOUR_PASSWORD"

# Connect
psql -h sqordia-db-production.c326icw2wbit.ca-central-1.rds.amazonaws.com `
     -p 5432 `
     -U sqordia_admin `
     -d SqordiaDb `
     --set=sslmode=require
```

## Next Steps

1. ✅ Connect to RDS using PgAdmin
2. ✅ Verify database tables exist
3. ✅ Run migrations if needed: `.\scripts\run-db-migrations.ps1`
4. ✅ Check migration status: `.\scripts\check-migration-status.ps1`

## Related Documentation

- [Database Connection Guide](DATABASE_CONNECTION.md) - General database connection information
- [Migration Guide](MIGRATION_GUIDE.md) - Running database migrations
- [ECS Fargate Deployment](ECS_FARGATE_DEPLOYMENT.md) - Backend deployment information

