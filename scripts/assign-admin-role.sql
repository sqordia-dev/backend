-- Assign Admin role to admin user
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
SELECT 
    gen_random_uuid(),
    u."Id",
    '550e8400-e29b-41d4-a716-446655440000'::uuid
FROM "Users" u
WHERE u."Email" = 'admin@sqordia.com'
AND NOT EXISTS (
    SELECT 1 FROM "UserRoles" ur 
    WHERE ur."UserId" = u."Id" 
    AND ur."RoleId" = '550e8400-e29b-41d4-a716-446655440000'::uuid
);

