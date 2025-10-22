USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Complete Role-Based Authorization Setup>
-- =============================================

PRINT 'Starting Role-Based Authorization Setup...';
PRINT '==========================================';

-- Step 1: Create Roles table
PRINT 'Step 1: Creating Roles table...';
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Roles](
        [RoleId] [int] IDENTITY(1,1) NOT NULL,
        [RoleName] [nvarchar](50) NOT NULL,
        [RoleDescription] [nvarchar](255) NULL,
        [IsActive] [bit] NOT NULL DEFAULT(1),
        [CreatedDate] [datetime] NOT NULL DEFAULT(GETDATE()),
        [CreatedBy] [varchar](50) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleId] ASC),
        CONSTRAINT [UK_Roles_RoleName] UNIQUE ([RoleName])
    );
    PRINT 'Roles table created successfully.';
END
ELSE
BEGIN
    PRINT 'Roles table already exists.';
END

-- Step 2: Create UserRoles table
PRINT 'Step 2: Creating UserRoles table...';
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserRoles](
        [UserCode] [int] NOT NULL,
        [RoleId] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT(1),
        [CreatedDate] [datetime] NOT NULL DEFAULT(GETDATE()),
        [CreatedBy] [varchar](50) NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([UserCode] ASC, [RoleId] ASC),
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserCode]) REFERENCES [dbo].[USER_MASTER] ([UM_CODE])
    );
    PRINT 'UserRoles table created successfully.';
END
ELSE
BEGIN
    PRINT 'UserRoles table already exists.';
END

-- Step 3: Insert default roles
PRINT 'Step 3: Inserting default roles...';
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleName], [RoleDescription], [CreatedBy]) VALUES
    ('Admin', 'Full system access - can perform all operations', 'System'),
    ('SalesManager', 'Sales module access - can manage sales and placements', 'System'),
    ('StoreManager', 'Store module access - can manage inventory and store operations', 'System'),
    ('PurchaseManager', 'Purchase module access - can manage purchases and suppliers', 'System'),
    ('ReadOnlyManager', 'Read-only access - can view all data but cannot modify', 'System'),
    ('UtilityManager', 'Utility module access - can manage system utilities and configurations', 'System');
    PRINT 'Default roles inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Default roles already exist.';
END

-- Step 4: Create test users
PRINT 'Step 4: Creating test users...';

-- Sales Manager
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'SALESMGR')
BEGIN
    INSERT INTO USER_MASTER (UM_CODE, UM_CM_ID, UM_USERNAME, UM_PASSWORD, UM_LEVEL, IS_ACTIVE, ES_DELETE, UM_IS_ADMIN, UM_NAME, UM_EMAIL)
    VALUES (-2147483647, 1, 'SALESMGR', '870-880-890-900-540-390-400-410-420', 'Sales Manager', 1, 0, 0, 'Sales Manager', 'sales@company.com');
    PRINT 'SALESMGR user created.';
END

-- Store Manager
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'STOREMGR')
BEGIN
    INSERT INTO USER_MASTER (UM_CODE, UM_CM_ID, UM_USERNAME, UM_PASSWORD, UM_LEVEL, IS_ACTIVE, ES_DELETE, UM_IS_ADMIN, UM_NAME, UM_EMAIL)
    VALUES (-2147483646, 1, 'STOREMGR', '870-880-890-900-540-390-400-410-420', 'Store Manager', 1, 0, 0, 'Store Manager', 'store@company.com');
    PRINT 'STOREMGR user created.';
END

-- Purchase Manager
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'PURCHASEMGR')
BEGIN
    INSERT INTO USER_MASTER (UM_CODE, UM_CM_ID, UM_USERNAME, UM_PASSWORD, UM_LEVEL, IS_ACTIVE, ES_DELETE, UM_IS_ADMIN, UM_NAME, UM_EMAIL)
    VALUES (-2147483645, 1, 'PURCHASEMGR', '870-880-890-900-540-390-400-410-420', 'Purchase Manager', 1, 0, 0, 'Purchase Manager', 'purchase@company.com');
    PRINT 'PURCHASEMGR user created.';
END

-- Read Only Manager
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'READONLYMGR')
BEGIN
    INSERT INTO USER_MASTER (UM_CODE, UM_CM_ID, UM_USERNAME, UM_PASSWORD, UM_LEVEL, IS_ACTIVE, ES_DELETE, UM_IS_ADMIN, UM_NAME, UM_EMAIL)
    VALUES (-2147483644, 1, 'READONLYMGR', '870-880-890-900-540-390-400-410-420', 'Read Only Manager', 1, 0, 0, 'Read Only Manager', 'readonly@company.com');
    PRINT 'READONLYMGR user created.';
END

-- Utility Manager
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'UTILITYMGR')
BEGIN
    INSERT INTO USER_MASTER (UM_CODE, UM_CM_ID, UM_USERNAME, UM_PASSWORD, UM_LEVEL, IS_ACTIVE, ES_DELETE, UM_IS_ADMIN, UM_NAME, UM_EMAIL)
    VALUES (-2147483643, 1, 'UTILITYMGR', '870-880-890-900-540-390-400-410-420', 'Utility Manager', 1, 0, 0, 'Utility Manager', 'utility@company.com');
    PRINT 'UTILITYMGR user created.';
END

-- Step 5: Create SP_GetUserRoles stored procedure
PRINT 'Step 5: Creating SP_GetUserRoles stored procedure...';
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUserRoles')
    DROP PROCEDURE [dbo].[SP_GetUserRoles];

CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserName VARCHAR(50),
    @CompanyId VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT r.RoleName
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    INNER JOIN USER_MASTER u ON ur.UserCode = u.UM_CODE
    WHERE u.UM_USERNAME = @UserName 
    AND u.UM_CM_ID = @CompanyId
    AND ur.IsActive = 1
    AND r.IsActive = 1
    AND u.ES_DELETE = 0;
    
    IF @@ROWCOUNT = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = @UserName AND UM_CM_ID = @CompanyId AND ES_DELETE = 0)
        BEGIN
            SELECT 'ReadOnlyManager' AS RoleName;
        END
    END
END
PRINT 'SP_GetUserRoles stored procedure created successfully.';

-- Step 6: Assign roles to users
PRINT 'Step 6: Assigning roles to users...';

-- Admin role to ERPADMIN
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'ERPADMIN' AND r.RoleName = 'Admin'
AND NOT EXISTS (SELECT 1 FROM UserRoles ur WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId);

-- SalesManager role to SALESMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'SALESMGR' AND r.RoleName = 'SalesManager'
AND NOT EXISTS (SELECT 1 FROM UserRoles ur WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId);

-- StoreManager role to STOREMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'STOREMGR' AND r.RoleName = 'StoreManager'
AND NOT EXISTS (SELECT 1 FROM UserRoles ur WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId);

-- PurchaseManager role to PURCHASEMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'PURCHASEMGR' AND r.RoleName = 'PurchaseManager'
AND NOT EXISTS (SELECT 1 FROM UserRoles ur WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId);

-- ReadOnlyManager role to READONLYMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'READONLYMGR' AND r.RoleName = 'ReadOnlyManager'
AND NOT EXISTS (SELECT 1 FROM UserRoles ur WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId);

-- UtilityManager role to UTILITYMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'UTILITYMGR' AND r.RoleName = 'UtilityManager'
AND NOT EXISTS (SELECT 1 FROM UserRoles ur WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId);

PRINT 'Roles assigned successfully.';

-- Step 7: Display final results
PRINT 'Step 7: Final setup summary...';
PRINT '==========================================';
PRINT 'ROLE-BASED AUTHORIZATION SETUP COMPLETED!';
PRINT '==========================================';
PRINT 'Test Users Created:';
PRINT '- ERPADMIN (Admin) - Password: admin';
PRINT '- SALESMGR (SalesManager) - Password: admin';
PRINT '- STOREMGR (StoreManager) - Password: admin';
PRINT '- PURCHASEMGR (PurchaseManager) - Password: admin';
PRINT '- READONLYMGR (ReadOnlyManager) - Password: admin';
PRINT '- UTILITYMGR (UtilityManager) - Password: admin';
PRINT '==========================================';
PRINT 'You can now test the API with different user roles!';
PRINT '==========================================';

-- Display current user roles
SELECT 
    u.UM_USERNAME,
    u.UM_NAME,
    r.RoleName,
    r.RoleDescription
FROM USER_MASTER u
INNER JOIN UserRoles ur ON u.UM_CODE = ur.UserCode
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE u.ES_DELETE = 0
ORDER BY u.UM_USERNAME, r.RoleName;
