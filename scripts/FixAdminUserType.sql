-- Fix Admin User UserType
-- This script updates the existing admin user's UserType to a valid enum value
-- Run this if you already created the admin user with UserType='Admin'

UPDATE [Users]
SET [UserType] = 'Entrepreneur' -- Valid values: 'Entrepreneur', 'OBNL', 'Consultant'
WHERE [Email] = 'admin@sqordia.com'
  AND [UserType] = 'Admin';

PRINT 'Admin user UserType updated from Admin to Entrepreneur';
PRINT 'Valid UserType values: Entrepreneur, OBNL, Consultant';

