SELECT u."Email", r."Name" as "RoleName"
FROM "Users" u
INNER JOIN "UserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN "Roles" r ON ur."RoleId" = r."Id"
WHERE u."Email" = 'admin@sqordia.com';

