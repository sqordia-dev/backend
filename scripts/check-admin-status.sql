SELECT 
    "Email",
    "UserName",
    "IsEmailConfirmed",
    "IsActive",
    "AccessFailedCount",
    "LockoutEnabled",
    "LockoutEnd",
    "RequirePasswordChange",
    LEFT("PasswordHash", 20) as "PasswordHashPreview"
FROM "Users"
WHERE "Email" = 'admin@sqordia.com';

