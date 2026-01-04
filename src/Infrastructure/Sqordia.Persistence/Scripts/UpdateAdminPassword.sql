-- PostgreSQL Script to Update Admin User Password Hash
-- This script updates the password hash for the admin user
-- Safe to run multiple times

-- Update admin user password hash
UPDATE "Users"
SET 
    "PasswordHash" = '$2a$11$pNDAz7j1AeB3q0xAzE8S2uOCePzCtYDajQ/jugHKo.u27dOSuQC4C', -- Password: Sqordia2025!
    "PasswordLastChangedAt" = NOW() AT TIME ZONE 'UTC',
    "RequirePasswordChange" = false,
    "AccessFailedCount" = 0,
    "LockoutEnd" = NULL
WHERE "Email" = 'admin@sqordia.com'
   OR "Id" = '1367e88c-d3a2-46c4-928b-40156092d0bf';

-- Display success message
DO $$
DECLARE
    updated_count INTEGER;
BEGIN
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    
    IF updated_count > 0 THEN
        RAISE NOTICE 'Admin password updated successfully!';
        RAISE NOTICE 'Email: admin@sqordia.com';
        RAISE NOTICE 'Password: Sqordia2025!';
        RAISE NOTICE 'Rows updated: %', updated_count;
    ELSE
        RAISE WARNING 'No admin user found to update.';
        RAISE WARNING 'Make sure the admin user exists in the database.';
    END IF;
END $$;

