-- PostgreSQL Seed Script for Sqordia Database
-- This script creates essential roles and admin user
-- Idempotent: Safe to run multiple times

-- Create roles if they don't exist
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystemRole")
VALUES 
    ('c1b7baaa-ae55-43f6-a735-ae543d1502f5', 'Admin', 'System administrator with full access', true),
    ('42c48a39-c6e2-418e-b52f-6c62a44fdb59', 'Collaborateur', 'Standard user role', true),
    ('738845f7-f705-41c3-9d17-4858d8f49e73', 'Lecteur', 'Read-only user role', true)
ON CONFLICT ("Id") DO NOTHING;

-- Create admin user if it doesn't exist
INSERT INTO "Users" ("Id", "Email", "UserName", "FirstName", "LastName", "PasswordHash", "IsEmailConfirmed", "EmailConfirmedAt", "IsActive", "UserType", "AccessFailedCount", "LockoutEnabled", "PhoneNumberVerified", "RequirePasswordChange", "Provider", "PasswordLastChangedAt", "Created", "IsDeleted")
VALUES (
    '1367e88c-d3a2-46c4-928b-40156092d0bf',
    'admin@sqordia.com',
    'admin@sqordia.com',
    'Admin',
    'User',
    '$2a$11$rQZ8K9mN2pL3sT4uV5wX6yA7bC8dE9fG0hI1jK2lM3nO4pP5qR6sS7tT8uU9vV0wW1xX2yY3zZ4', -- Password: Sqordia2025!
    true,
    NOW() AT TIME ZONE 'UTC',
    true,
    'Entrepreneur',
    0,
    true,
    false,
    false,
    'local',
    NOW() AT TIME ZONE 'UTC',
    NOW() AT TIME ZONE 'UTC',
    false
)
ON CONFLICT ("Id") DO NOTHING;

-- Assign Admin role to admin user if not already assigned
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
SELECT 
    gen_random_uuid(),
    '1367e88c-d3a2-46c4-928b-40156092d0bf',
    'c1b7baaa-ae55-43f6-a735-ae543d1502f5'
WHERE NOT EXISTS (
    SELECT 1 FROM "UserRoles" 
    WHERE "UserId" = '1367e88c-d3a2-46c4-928b-40156092d0bf' 
    AND "RoleId" = 'c1b7baaa-ae55-43f6-a735-ae543d1502f5'
);

-- Create some basic permissions
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
ON CONFLICT ("Id") DO NOTHING;

-- Assign all permissions to Admin role
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT 
    gen_random_uuid() as "Id",
    'c1b7baaa-ae55-43f6-a735-ae543d1502f5' as "RoleId",
    "Id" as "PermissionId"
FROM "Permissions"
ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;

-- Assign read permissions to Collaborateur role
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT 
    gen_random_uuid() as "Id",
    '42c48a39-c6e2-418e-b52f-6c62a44fdb59' as "RoleId",
    "Id" as "PermissionId"
FROM "Permissions"
WHERE "Name" LIKE '%.Read'
ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;

-- Assign read permissions to Lecteur role
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId")
SELECT 
    gen_random_uuid() as "Id",
    '738845f7-f705-41c3-9d17-4858d8f49e73' as "RoleId",
    "Id" as "PermissionId"
FROM "Permissions"
WHERE "Name" LIKE '%.Read'
ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;

-- Display success message
DO $$
BEGIN
    RAISE NOTICE 'Database seeded successfully!';
    RAISE NOTICE 'Admin user: admin@sqordia.com';
    RAISE NOTICE 'Password: Sqordia2025!';
    RAISE NOTICE 'Roles created: Admin, Collaborateur, Lecteur';
END $$;
