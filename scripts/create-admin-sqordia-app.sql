-- =============================================================================
-- Create Admin User Script
-- Creates a super admin user with email: admin@sqordia.app
-- Password: Admin@2026!
-- This user will have the Admin role with ALL permissions
-- =============================================================================
--
-- =============================================================================
-- Admin User Credentials:
--   Email: admin@sqordia.app
--   Password: Admin@2026!
--   Role: Admin (with ALL permissions - super user)
-- =============================================================================

BEGIN;

-- =============================================================================
-- STEP 1: Ensure Admin role exists
-- =============================================================================
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystemRole")
VALUES 
    ('c1b7baaa-ae55-43f6-a735-ae543d1502f5', 'Admin', 'System administrator with full access', true)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "IsSystemRole" = EXCLUDED."IsSystemRole";

-- =============================================================================
-- STEP 2: Ensure all permissions exist
-- =============================================================================
INSERT INTO "Permissions" ("Id", "Name", "Description", "Category")
VALUES 
    ('c2d82683-6776-442b-a676-c13890370555', 'Users.Read', 'Read user information', 'Users'),
    ('79bc4e3f-43a5-4c21-9304-6bac61924649', 'Users.Write', 'Create and update users', 'Users'),
    ('f8f4c48c-4aac-48cf-b6e1-6a08347c973d', 'Users.Delete', 'Delete users', 'Users'),
    ('bb755994-2099-47c0-8439-6b326413275e', 'Roles.Read', 'Read role information', 'Roles'),
    ('03d9bb24-243b-4276-a708-a33b3c92de0d', 'Roles.Write', 'Create and update roles', 'Roles'),
    ('e71f47ec-36ce-4eb3-aff1-3452c61d259f', 'BusinessPlans.Read', 'Read business plans', 'BusinessPlans'),
    ('33b45fa3-0922-4647-8622-005a3956d653', 'BusinessPlans.Write', 'Create and update business plans', 'BusinessPlans'),
    ('4f469809-2142-4cc0-9d0e-43c6e4e47bc6', 'BusinessPlans.Delete', 'Delete business plans', 'BusinessPlans')
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "Category" = EXCLUDED."Category";

-- =============================================================================
-- STEP 3: Create Admin User
-- =============================================================================
INSERT INTO "Users" (
    "Id", 
    "FirstName", 
    "LastName", 
    "Email", 
    "UserName", 
    "PasswordHash", 
    "IsEmailConfirmed", 
    "EmailConfirmedAt", 
    "IsActive", 
    "UserType", 
    "AccessFailedCount", 
    "LockoutEnabled", 
    "LockoutEnd",
    "PhoneNumberVerified",
    "RequirePasswordChange",
    "Provider",
    "PasswordLastChangedAt",
    "Created", 
    "IsDeleted"
)
VALUES (
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890'::uuid, -- Fixed User ID for admin@sqordia.app
    'Admin',
    'User',
    'admin@sqordia.app',
    'admin@sqordia.app',
    '$2a$11$gwJfpE.m/kTXnkrxNKeAhOmG5E9rbV8Fl95S076kPUIhSUwvjQ1xm', -- BCrypt hash for "Admin@2026!"
    true, -- IsEmailConfirmed
    NOW() AT TIME ZONE 'UTC', -- EmailConfirmedAt
    true, -- IsActive
    'Entrepreneur', -- UserType (valid values: Entrepreneur, OBNL, Consultant)
    0, -- AccessFailedCount
    true, -- LockoutEnabled
    NULL, -- LockoutEnd (not locked)
    false, -- PhoneNumberVerified
    false, -- RequirePasswordChange
    'local', -- Provider
    NOW() AT TIME ZONE 'UTC', -- PasswordLastChangedAt
    NOW() AT TIME ZONE 'UTC', -- Created
    false -- IsDeleted
)
ON CONFLICT ("Email") DO UPDATE SET
    "PasswordHash" = EXCLUDED."PasswordHash",
    "IsActive" = true,
    "IsEmailConfirmed" = true,
    "EmailConfirmedAt" = COALESCE("Users"."EmailConfirmedAt", NOW() AT TIME ZONE 'UTC'),
    "LastModified" = NOW() AT TIME ZONE 'UTC';

-- =============================================================================
-- STEP 4: Assign Admin Role to User
-- =============================================================================
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
SELECT 
    gen_random_uuid(),
    COALESCE(
        (SELECT "Id" FROM "Users" WHERE "Email" = 'admin@sqordia.app' AND "IsDeleted" = false),
        'a1b2c3d4-e5f6-7890-abcd-ef1234567890'::uuid
    ),
    'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid -- Admin role ID
WHERE NOT EXISTS (
    SELECT 1 FROM "UserRoles" ur 
    INNER JOIN "Users" u ON ur."UserId" = u."Id"
    WHERE u."Email" = 'admin@sqordia.app'
      AND u."IsDeleted" = false
      AND ur."RoleId" = 'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid
);

-- =============================================================================
-- STEP 5: Assign ALL Permissions to Admin Role
-- This ensures the Admin role has all permissions (super user)
-- =============================================================================
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT 
    gen_random_uuid() as "Id",
    'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid as "RoleId", -- Admin role
    "Id" as "PermissionId"
FROM "Permissions"
WHERE NOT EXISTS (
    SELECT 1 FROM "RolePermissions" 
    WHERE "RoleId" = 'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid 
      AND "PermissionId" = "Permissions"."Id"
);

-- =============================================================================
-- STEP 6: Verification
-- =============================================================================
DO $$
DECLARE
    v_user_id UUID;
    v_role_count INT;
    v_permission_count INT;
BEGIN
    -- Get user ID
    SELECT "Id" INTO v_user_id
    FROM "Users"
    WHERE "Email" = 'admin@sqordia.app' AND "IsDeleted" = false;
    
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'User with email admin@sqordia.app was not created';
    END IF;
    
    -- Check Admin role assignment
    SELECT COUNT(*) INTO v_role_count
    FROM "UserRoles" ur
    INNER JOIN "Roles" r ON ur."RoleId" = r."Id"
    WHERE ur."UserId" = v_user_id
      AND r."Name" = 'Admin';
    
    -- Check permissions count
    SELECT COUNT(*) INTO v_permission_count
    FROM "RolePermissions" rp
    WHERE rp."RoleId" = 'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid;
    
    RAISE NOTICE '✓ SUCCESS: Admin user created';
    RAISE NOTICE '  Email: admin@sqordia.app';
    RAISE NOTICE '  Password: Admin@2026!';
    RAISE NOTICE '  User ID: %', v_user_id;
    RAISE NOTICE '  Admin role assigned: %', CASE WHEN v_role_count > 0 THEN 'Yes' ELSE 'No' END;
    RAISE NOTICE '  Total permissions: %', v_permission_count;
END $$;

COMMIT;

-- =============================================================================
-- VERIFICATION QUERY (run separately if needed)
-- =============================================================================
/*
SELECT 
    u."Email",
    u."FirstName",
    u."LastName",
    u."IsActive",
    u."IsEmailConfirmed",
    r."Name" as "RoleName",
    COUNT(DISTINCT rp."PermissionId") as "PermissionCount"
FROM "Users" u
INNER JOIN "UserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN "Roles" r ON ur."RoleId" = r."Id"
LEFT JOIN "RolePermissions" rp ON r."Id" = rp."RoleId"
WHERE u."Email" = 'admin@sqordia.app'
GROUP BY u."Email", u."FirstName", u."LastName", u."IsActive", u."IsEmailConfirmed", r."Name";
*/
