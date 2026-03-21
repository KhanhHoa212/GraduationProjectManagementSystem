-- ============================================================
-- GPMS: Seed Data Update Script
-- Password for ALL accounts: 123456 (bcrypt hashed)
-- Run this against your existing GPMS_Database
-- ============================================================
USE GPMS_Database;
GO

-- Step 1: Add missing accounts to UserCredentials
-- (Safe: only inserts if not already present)
INSERT INTO UserCredentials (UserID, AuthProvider, PasswordHash)
SELECT v.UserID, v.AuthProvider, v.PasswordHash
FROM (VALUES
    ('GV002',    'Internal', '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW'),
    ('GV003',    'Internal', '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW'),
    ('SE180002', 'Internal', '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW'),
    ('SE180003', 'Internal', '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW'),
    ('SE180004', 'Internal', '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW'),
    ('SE180005', 'Internal', '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW')
) AS v(UserID, AuthProvider, PasswordHash)
WHERE NOT EXISTS (
    SELECT 1 FROM UserCredentials uc
    WHERE uc.UserID = v.UserID AND uc.AuthProvider = v.AuthProvider
);
GO

-- Step 2: Apply EF Core migration (seed data changes)
-- Run this in Package Manager Console or terminal:
-- dotnet ef database update --project GPMS.Infrastructure --startup-project GPMS.Web
-- OR drop the database and re-run the full schema script:
-- dotnet ef database drop --force --project GPMS.Infrastructure --startup-project GPMS.Web
-- dotnet ef database update --project GPMS.Infrastructure --startup-project GPMS.Web
