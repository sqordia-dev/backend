-- ============================================================================
-- Sqordia Database Seed Script (PostgreSQL)
-- ============================================================================
-- This script seeds the database with essential data:
--   - Roles (Admin, Collaborateur, Lecteur)
--   - Permissions (Users, Roles, BusinessPlans)
--   - Admin User (admin@sqordia.com)
--   - Role-Permission assignments
--   - User-Role assignments
--
-- Password: Sqordia2025!
-- Idempotent: Safe to run multiple times (uses ON CONFLICT)
-- ============================================================================

BEGIN;

-- ============================================================================
-- 1. CREATE ROLES
-- ============================================================================
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystemRole")
VALUES 
    ('c1b7baaa-ae55-43f6-a735-ae543d1502f5', 'Admin', 'System administrator with full access', true),
    ('42c48a39-c6e2-418e-b52f-6c62a44fdb59', 'Collaborateur', 'Standard user role', true),
    ('738845f7-f705-41c3-9d17-4858d8f49e73', 'Lecteur', 'Read-only user role', true)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "IsSystemRole" = EXCLUDED."IsSystemRole";

-- ============================================================================
-- 2. CREATE PERMISSIONS
-- ============================================================================
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

-- ============================================================================
-- 3. CREATE ADMIN USER
-- ============================================================================
-- Password: Sqordia2025!
-- Hash generated using BCrypt.Net.BCrypt.HashPassword("Sqordia2025!")
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
    '1367e88c-d3a2-46c4-928b-40156092d0bf'::uuid,
    'Admin',
    'User',
    'admin@sqordia.com',
    'admin@sqordia.com',
    '$2a$11$A0bIb8AicZpummTj/P1R0ulYPvlEBsanmULeaf7m2969WfbBDcdWm', -- BCrypt hash for: Sqordia2025! (generated fresh)
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
ON CONFLICT ("Id") DO UPDATE SET
    "PasswordHash" = EXCLUDED."PasswordHash",
    "PasswordLastChangedAt" = EXCLUDED."PasswordLastChangedAt",
    "AccessFailedCount" = 0,
    "LockoutEnd" = NULL,
    "IsActive" = true,
    "IsEmailConfirmed" = true,
    "EmailConfirmedAt" = COALESCE("Users"."EmailConfirmedAt", NOW() AT TIME ZONE 'UTC');

-- ============================================================================
-- 4. ASSIGN ADMIN ROLE TO ADMIN USER
-- ============================================================================
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
SELECT 
    gen_random_uuid(),
    '1367e88c-d3a2-46c4-928b-40156092d0bf'::uuid,
    'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid -- Admin role
WHERE NOT EXISTS (
    SELECT 1 FROM "UserRoles" 
    WHERE "UserId" = '1367e88c-d3a2-46c4-928b-40156092d0bf'::uuid 
    AND "RoleId" = 'c1b7baaa-ae55-43f6-a735-ae543d1502f5'::uuid
);

-- ============================================================================
-- 5. ASSIGN ALL PERMISSIONS TO ADMIN ROLE
-- ============================================================================
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

-- ============================================================================
-- 6. ASSIGN READ PERMISSIONS TO COLLABORATEUR ROLE
-- ============================================================================
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT 
    gen_random_uuid() as "Id",
    '42c48a39-c6e2-418e-b52f-6c62a44fdb59'::uuid as "RoleId", -- Collaborateur role
    "Id" as "PermissionId"
FROM "Permissions"
WHERE "Name" LIKE '%.Read'
AND NOT EXISTS (
    SELECT 1 FROM "RolePermissions" 
    WHERE "RoleId" = '42c48a39-c6e2-418e-b52f-6c62a44fdb59'::uuid 
    AND "PermissionId" = "Permissions"."Id"
);

-- ============================================================================
-- 7. ASSIGN READ PERMISSIONS TO LECTEUR ROLE
-- ============================================================================
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT 
    gen_random_uuid() as "Id",
    '738845f7-f705-41c3-9d17-4858d8f49e73'::uuid as "RoleId", -- Lecteur role
    "Id" as "PermissionId"
FROM "Permissions"
WHERE "Name" LIKE '%.Read'
AND NOT EXISTS (
    SELECT 1 FROM "RolePermissions" 
    WHERE "RoleId" = '738845f7-f705-41c3-9d17-4858d8f49e73'::uuid 
    AND "PermissionId" = "Permissions"."Id"
);

-- ============================================================================
-- 8. VERIFICATION (Run separately if needed)
-- ============================================================================
-- Admin User: admin@sqordia.com / Password: Sqordia2025!
-- User ID: 1367e88c-d3a2-46c4-928b-40156092d0bf
-- Roles: Admin, Collaborateur, Lecteur
-- Permissions: 8 permissions created and assigned

COMMIT;

-- ============================================================================
-- END OF SEED SCRIPT
-- ============================================================================

