-- Drop and recreate RefreshTokens table to fix duplicate key violations
-- This script will:
-- 1. Drop the foreign key constraint
-- 2. Drop the table
-- 3. Recreate the table with proper structure
-- 4. Recreate the foreign key constraint

-- Connect to the correct database
\c SqordiaDb;

-- Drop the foreign key constraint first
ALTER TABLE IF EXISTS "RefreshTokens" 
DROP CONSTRAINT IF EXISTS "FK_RefreshTokens_Users_UserId";

-- Drop the table
DROP TABLE IF EXISTS "RefreshTokens";

-- Recreate the table with proper structure
CREATE TABLE "RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Token" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone NULL,
    "RevokedByIp" text NULL,
    "ReplacedByToken" text NULL,
    "CreatedByIp" text NOT NULL,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id")
);

-- Create index on UserId for better query performance
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

-- Recreate the foreign key constraint
ALTER TABLE "RefreshTokens"
ADD CONSTRAINT "FK_RefreshTokens_Users_UserId"
FOREIGN KEY ("UserId")
REFERENCES "Users" ("Id")
ON DELETE CASCADE;

-- Verify the table was created correctly
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns
WHERE table_name = 'RefreshTokens'
ORDER BY ordinal_position;
