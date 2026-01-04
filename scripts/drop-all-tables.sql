-- Drop all tables in the database
-- This script removes all foreign key constraints and tables

USE SqordiaDb;
GO

-- Disable all constraints
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
GO

-- Drop all tables
EXEC sp_MSforeachtable "DROP TABLE ?"
GO

-- Verify all tables are dropped
SELECT COUNT(*) AS RemainingTables FROM sys.tables WHERE type = 'U'
GO

