-- =============================================================================
-- Make User Admin Script (Simple Version)
-- Assigns Admin role to a user by email address
-- Automatically finds Admin role by name (works regardless of role ID)
-- Idempotent: Safe to run multiple times
-- =============================================================================
--
-- INSTRUCTIONS:
--   1. Replace 'user@example.com' with the actual user email address
--   2. Run the script
--
-- =============================================================================

BEGIN;

-- Assign Admin role to user by email
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
SELECT 
    gen_random_uuid(),
    u."Id",
    r."Id" as "RoleId"
FROM "Users" u
CROSS JOIN "Roles" r
WHERE u."Email" = 'user@example.com'  -- ⚠️ CHANGE THIS EMAIL
  AND u."IsDeleted" = false
  AND r."Name" = 'Admin'
  AND NOT EXISTS (
      SELECT 1 FROM "UserRoles" ur 
      WHERE ur."UserId" = u."Id" 
        AND ur."RoleId" = r."Id"
  );

-- Verify assignment
DO $$
DECLARE
    v_user_email TEXT := 'user@example.com';  -- ⚠️ CHANGE THIS EMAIL
    v_user_count INT;
    v_role_count INT;
BEGIN
    -- Check if user exists
    SELECT COUNT(*) INTO v_user_count
    FROM "Users"
    WHERE "Email" = v_user_email AND "IsDeleted" = false;
    
    IF v_user_count = 0 THEN
        RAISE EXCEPTION 'User with email % not found', v_user_email;
    END IF;
    
    -- Check if Admin role exists
    SELECT COUNT(*) INTO v_role_count
    FROM "Roles"
    WHERE "Name" = 'Admin';
    
    IF v_role_count = 0 THEN
        RAISE EXCEPTION 'Admin role does not exist. Please run seed-database.sql first.';
    END IF;
    
    -- Check if role was assigned
    SELECT COUNT(*) INTO v_role_count
    FROM "UserRoles" ur
    INNER JOIN "Users" u ON ur."UserId" = u."Id"
    INNER JOIN "Roles" r ON ur."RoleId" = r."Id"
    WHERE u."Email" = v_user_email
      AND r."Name" = 'Admin';
    
    IF v_role_count > 0 THEN
        RAISE NOTICE '✓ SUCCESS: User % now has Admin role', v_user_email;
    ELSE
        RAISE WARNING '⚠ WARNING: Admin role was not assigned to user %', v_user_email;
    END IF;
END $$;

COMMIT;
