# Running CMS Seed Script Manually

Since automated execution encountered connection issues, here are manual methods to run the CMS seed script:

## Method 1: Using pgAdmin or DBeaver (Recommended)

1. **Connect to your database:**
   - Host: `localhost`
   - Port: `5433` (or `5432` if using default)
   - Database: `SqordiaDb`
   - Username: `sqordia` (or `postgres`)
   - Password: Check your `appsettings.json` or environment variables

2. **Open the script:**
   - Navigate to: `backend/cms-seed-data.sql`

3. **Execute the script:**
   - Copy the entire contents of `cms-seed-data.sql`
   - Paste into the SQL query editor
   - Execute (F5 or Run button)

## Method 2: Using psql Command Line

If you have PostgreSQL client tools installed:

```bash
# From the backend directory
psql -h localhost -p 5433 -U sqordia -d SqordiaDb -f cms-seed-data.sql
```

You'll be prompted for the password.

## Method 3: Using Docker (if database is in Docker)

```bash
# Copy script to container and execute
docker cp backend/cms-seed-data.sql sqordia-db:/tmp/cms-seed-data.sql
docker exec -i sqordia-db psql -U sqordia -d SqordiaDb -f /tmp/cms-seed-data.sql
```

## Method 4: Using .NET Application

You can modify the application to run this script on startup, similar to how `seed-all.sql` is executed.

## What the Script Does

1. **Cleanup**: Deletes existing CMS content blocks for version `17a4a74e-4782-4ca0-9493-aebbd22dcc95`
2. **Creates/Updates**: CMS Version record
3. **Inserts**: All CMS content blocks (Dashboard, Profile sections)
4. **Idempotent**: Safe to run multiple times

## Verification

After running, verify with:

```sql
SELECT COUNT(*) FROM "CmsContentBlocks" WHERE "CmsVersionId" = '17a4a74e-4782-4ca0-9493-aebbd22dcc95';
```

Expected: Should return the number of content blocks inserted.
