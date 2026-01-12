-- Update PostgreSQL password for user 'sqordia'
-- Run this script to change the database password to match appsettings.json
-- 
-- Usage:
--   If using Docker: docker exec -i sqordia-db-dev psql -U sqordia -d SqordiaDb < update-db-password.sql
--   If using local PostgreSQL: psql -h localhost -p 5433 -U sqordia -d SqordiaDb -f update-db-password.sql
--   Or connect with any PostgreSQL client and run the ALTER USER command

-- Change password for sqordia user
ALTER USER sqordia WITH PASSWORD 'SqordiaDev2025!';

-- Verify the change (optional - will show user info)
\du sqordia

