-- =============================================================================
-- Make User Admin Script
-- This script assigns the Admin role to a user by email address
-- Idempotent: Safe to run multiple times (handles conflicts)
-- =============================================================================
--
-- Usage:
--   1. Replace 'user@example.com' with the actual user email
--   2. Or use the user ID directly by modifying the WHERE clause
--
-- Example:
--   - By email: Change 'user@example.com' to the target email
--   - By user ID: Replace the WHERE clause with: WHERE u."Id" = 'user-id-here'::uuid
--
-- =============================================================================

BEGIN;

-- =============================================================================
-- STEP 1: Ensure Admin role exists (create if missing)
-- =============================================================================
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystemRole")
VALUES 
    ('c1b7baaa-ae55-43f6-a735-ae543d1502f5', 'Admin', 'System administrator with full access', true)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "IsSystemRole" = EXCLUDED."IsSystemRole";

-- =============================================================================
-- STEP 2: Assign Admin role to user by email
-- Replace 'user@example.com' with the actual user email
-- =============================================================================
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
SELECT 
    gen_random_uuid(),
    u."Id",
    'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid -- Admin role ID
FROM "Users" u
WHERE u."Email" = 'user@example.com'  -- ⚠️ CHANGE THIS EMAIL
  AND u."IsDeleted" = false
  AND NOT EXISTS (
      SELECT 1 FROM "UserRoles" ur 
      WHERE ur."UserId" = u."Id" 
        AND ur."RoleId" = 'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid
  );

-- =============================================================================
-- STEP 3: Verify the assignment
-- =============================================================================
DO $$
DECLARE
    v_user_email TEXT := 'user@example.com';  -- ⚠️ CHANGE THIS EMAIL
    v_user_id UUID;
    v_role_assigned BOOLEAN;
BEGIN
    -- Get user ID
    SELECT "Id" INTO v_user_id
    FROM "Users"
    WHERE "Email" = v_user_email AND "IsDeleted" = false;
    
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'User with email % not found', v_user_email;
    END IF;
    
    -- Check if role was assigned
    SELECT EXISTS (
        SELECT 1 FROM "UserRoles" ur
        INNER JOIN "Roles" r ON ur."RoleId" = r."Id"
        WHERE ur."UserId" = v_user_id
          AND r."Name" = 'Admin'
    ) INTO v_role_assigned;
    
    IF v_role_assigned THEN
        RAISE NOTICE '✓ SUCCESS: User % (ID: %) now has Admin role', v_user_email, v_user_id;
    ELSE
        RAISE WARNING '⚠ WARNING: Admin role was not assigned to user %', v_user_email;
    END IF;
END $$;

COMMIT;

-- =============================================================================
-- ALTERNATIVE: Make user admin by User ID instead of email
-- =============================================================================
-- Uncomment and modify the following if you want to use User ID:
/*
BEGIN;

-- Ensure Admin role exists
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystemRole")
VALUES 
    ('c1b7baaa-ae55-43f6-a735-ae543d1502f5', 'Admin', 'System administrator with full access', true)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "IsSystemRole" = EXCLUDED."IsSystemRole";

-- Assign Admin role by User ID
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
SELECT 
    gen_random_uuid(),
    '00000000-0000-0000-0000-000000000000'::uuid,  -- ⚠️ REPLACE WITH ACTUAL USER ID
    'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid
WHERE NOT EXISTS (
    SELECT 1 FROM "UserRoles" ur 
    WHERE ur."UserId" = '00000000-0000-0000-0000-000000000000'::uuid  -- ⚠️ REPLACE WITH ACTUAL USER ID
      AND ur."RoleId" = 'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid
);

COMMIT;
*/

-- =============================================================================
-- VERIFICATION QUERY (run after the script to verify)
-- =============================================================================
-- Uncomment to check if user has Admin role:
/*
SELECT 
    u."Email",
    u."FirstName",
    u."LastName",
    r."Name" as "RoleName",
    r."Description" as "RoleDescription"
FROM "Users" u
INNER JOIN "UserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN "Roles" r ON ur."RoleId" = r."Id"
WHERE u."Email" = 'user@example.com'  -- ⚠️ CHANGE THIS EMAIL
  AND r."Name" = 'Admin';
*/
