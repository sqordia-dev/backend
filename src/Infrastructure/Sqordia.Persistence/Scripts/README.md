# Database Scripts

This folder contains SQL scripts for database initialization and seeding.

## 📋 Overview

The Sqordia Authentication API uses **Entity Framework Core Migrations** to create and manage the database schema. SQL scripts are only used for **seeding initial data** (admin user and roles).

## 🚀 Quick Start

### Step 1: Run Migrations (Required)
First, create the database schema using EF migrations:

```bash
# From project root
dotnet ef database update --project src/Infrastructure/Sqordia.Persistence --startup-project src/WebAPI
```

This will:
- Create the `SqordiaDb` database
- Create all authentication tables (Users, Roles, UserRoles, etc.)
- Apply all migrations

### Step 2: Seed Admin User (Recommended)
After migrations, seed the admin user and roles:

**Option A: Using Docker (Recommended)**
```bash
docker exec -i sqordia-db psql -U sqordia_user -d SqordiaDb -f /docker-entrypoint-initdb.d/SeedAdminUser.sql
```

**Option B: Using pgAdmin or DBeaver**
1. Connect to your PostgreSQL database
2. Open `SeedAdminUser.sql`
3. Execute the script

**Option C: Using psql directly**
```bash
psql -h localhost -U sqordia_user -d SqordiaDb -f src/Infrastructure/Sqordia.Persistence/Scripts/SeedAdminUser.sql
```

## 📁 Available Scripts

### `SeedAdminUser.sql`
**Purpose**: Seeds the database with essential roles and admin user

**What it does:**
- Creates 3 roles: Admin, Collaborateur, Lecteur
- Creates admin user (admin@sqordia.com)
- Assigns Admin role to the admin user

**When to use:**
- After running migrations for the first time
- After resetting the database
- When you need to recreate the admin user

**Credentials Created:**
- **Email**: `admin@sqordia.com`
- **Password**: `Sqordia2025!`

**Idempotent**: ✅ Safe to run multiple times (checks if data exists)

### `UpdateAdminPassword.sql`
**Purpose**: Updates the password hash for the existing admin user

**What it does:**
- Updates the password hash for admin@sqordia.com
- Resets `AccessFailedCount` to 0 (unlocks account if locked)
- Clears `LockoutEnd` (removes account lockout)
- Updates `PasswordLastChangedAt` timestamp
- Sets `RequirePasswordChange` to false`

**When to use:**
- When the admin password hash is invalid or corrupted
- After resetting the admin password
- When login fails due to invalid password hash

**Credentials Updated:**
- **Email**: `admin@sqordia.com`
- **Password**: `Sqordia2025!`

**Safe to run**: ✅ Can be run multiple times safely

**Usage:**
```powershell
# Using PowerShell script (recommended)
.\scripts\update-admin-password.ps1

# Or manually via psql
psql -h <host> -U <username> -d SqordiaDb -f src/Infrastructure/Sqordia.Persistence/Scripts/UpdateAdminPassword.sql
```

## 🔄 Complete Setup Workflow

### Local Development Setup

1. **Start Database** (if using Docker):
   ```bash
   docker-compose -f docker-compose.dev.yml up -d sqordia-db
   ```

2. **Run Migrations**:
   ```bash
   dotnet ef database update --project src/Infrastructure/Sqordia.Persistence --startup-project src/WebAPI
   ```

3. **Seed Admin User**:
   ```bash
   docker exec -i sqordia-db psql -U sqordia_user -d SqordiaDb -f /docker-entrypoint-initdb.d/SeedAdminUser.sql
   ```

4. **Verify Setup**:
   ```bash
   # Check tables exist
   docker exec sqordia-db psql -U sqordia_user -d SqordiaDb -c "\dt"
   ```

5. **Test Login**:
   - Use Postman collection
   - Login with admin@sqordia.com / Sqordia2025!

## 🗂️ Database Schema

### Tables Created by Migrations

| Table | Purpose |
|-------|---------|
| `Users` | User accounts |
| `Roles` | System roles |
| `UserRoles` | User-role associations |
| `Permissions` | Permission definitions |
| `RolePermissions` | Role-permission associations |
| `RefreshTokens` | JWT refresh tokens |
| `EmailVerificationTokens` | Email verification |
| `PasswordResetTokens` | Password reset |
| `AuditLogs` | Audit trail |
| `__EFMigrationsHistory` | EF migration tracking |

## 🔐 Default Credentials

After running `SeedAdminUser.sql`:

**Admin User:**
- Email: `admin@sqordia.com`
- Password: `Sqordi@2025!`
- Role: Admin (full access)

**Roles Created:**
1. **Admin** - System administrator with full access
2. **Collaborateur** - Standard user role
3. **Lecteur** - Read-only user role

## 🧪 Testing Database Setup

### Verify Tables Exist
```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;
```

Expected: 10+ tables

### Verify Admin User Exists
```sql
SELECT u."Email", u."UserName", r."Name" as "RoleName"
FROM "Users" u
LEFT JOIN "UserRoles" ur ON u."Id" = ur."UserId"
LEFT JOIN "Roles" r ON ur."RoleId" = r."Id"
WHERE u."Email" = 'admin@sqordia.com';
```

Expected: One row with Admin role

### Verify Roles Exist
```sql
SELECT "Name", "Description", "IsSystemRole" 
FROM "Roles" 
ORDER BY "Name";
```

Expected: 3 roles (Admin, Collaborateur, Lecteur)

## 🔄 Reset Database

If you need to start fresh:

### Option 1: Drop and Recreate (Clean Slate)
```bash
# Stop and remove containers
docker-compose -f docker-compose.dev.yml down

# Remove volume (this will delete all data)
docker volume rm sqordia_sqordia_db_data

# Start fresh
docker-compose -f docker-compose.dev.yml up -d sqordia-db

# Run migrations
dotnet ef database update --project src/Infrastructure/Sqordia.Persistence --startup-project src/WebAPI

# Seed admin user
docker exec -i sqordia-db psql -U sqordia_user -d SqordiaDb -f /docker-entrypoint-initdb.d/SeedAdminUser.sql
```

### Option 2: Delete Data Only (Keep Schema)
```sql
-- Delete all data but keep tables
DELETE FROM "UserRoles";
DELETE FROM "RolePermissions";
DELETE FROM "RefreshTokens";
DELETE FROM "EmailVerificationTokens";
DELETE FROM "PasswordResetTokens";
DELETE FROM "AuditLogs";
DELETE FROM "Users";
DELETE FROM "Roles";
DELETE FROM "Permissions";
```

Then re-run `SeedAdminUser.sql`

## 📝 Notes

- **Use EF Migrations**: Always use migrations for schema changes, not SQL scripts
- **Seed Scripts Only**: SQL scripts are only for seeding initial data
- **Idempotent Scripts**: All seed scripts can be run multiple times safely
- **Password Security**: Change admin password in production!
- **Backup Important**: Always backup before running scripts on production

## 🆘 Troubleshooting

### Error: "Database does not exist"
**Solution**: Run migrations first (`dotnet ef database update`)

### Error: "Cannot insert duplicate key"
**Solution**: The script is idempotent - this message is expected if data already exists

### Error: "Login failed"
**Solution**: Check your database connection string in secrets.json or user-secrets

### Admin Login Not Working
**Solution**: 
1. Verify admin user exists (see verification queries above)
2. Check password is correct: `Sqordi@2025!`
3. Ensure user has Admin role assigned

## 🔗 Related Documentation

- [DEVELOPER_SETUP.md](../../../DEVELOPER_SETUP.md) - Complete setup guide
- [DOCKER_SETUP.md](../../../DOCKER_SETUP.md) - Docker setup
- [SECRETS_SETUP.md](../../../SECRETS_SETUP.md) - Configuration guide
- [DATABASE_CLEANUP_SUMMARY.md](../../../DATABASE_CLEANUP_SUMMARY.md) - Database cleanup info

---

**Last Updated:** October 11, 2024  
**Version:** Authentication-Only API (Simplified)
