USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Setup Role-Based Authorization Tables and Data>
-- =============================================

-- 1. Create Roles table (if not exists)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Roles](
        [RoleId] [int] IDENTITY(1,1) NOT NULL,
        [RoleName] [nvarchar](50) NOT NULL,
        [RoleDescription] [nvarchar](255) NULL,
        [IsActive] [bit] NOT NULL DEFAULT(1),
        CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleId] ASC),
        CONSTRAINT [UK_Roles_RoleName] UNIQUE ([RoleName])
    );
END
GO

-- 2. Create UserRoles table (if not exists)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserRoles](
        [UserId] [int] NOT NULL,
        [RoleId] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT(1),
        [CreatedDate] [datetime] NOT NULL DEFAULT(GETDATE()),
        [CreatedBy] [varchar](50) NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[USER_MASTER] ([UserId])
    );
END
GO

-- 3. Add UserId column to USER_MASTER if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('USER_MASTER') AND name = 'UserId')
BEGIN
    ALTER TABLE [dbo].[USER_MASTER] ADD [UserId] [int] IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[USER_MASTER] ADD CONSTRAINT [PK_USER_MASTER_UserId] PRIMARY KEY ([UserId]);
END
GO

-- 4. Insert default roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleName], [RoleDescription]) VALUES
    ('Admin', 'Full system access - can perform all operations'),
    ('SalesManager', 'Sales module access - can manage sales and placements'),
    ('StoreManager', 'Store module access - can manage inventory and store operations'),
    ('PurchaseManager', 'Purchase module access - can manage purchases and suppliers'),
    ('ReadOnlyManager', 'Read-only access - can view all data but cannot modify'),
    ('UtilityManager', 'Utility module access - can manage system utilities and configurations');
END
GO

-- 5. Create SP_GetUserRoles stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUserRoles')
    DROP PROCEDURE [dbo].[SP_GetUserRoles];
GO

CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserName VARCHAR(50),
    @CompanyId VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get user roles based on UserRoles and Roles tables
    SELECT r.RoleName
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    INNER JOIN USER_MASTER u ON ur.UserId = u.UserId
    WHERE u.UM_USERNAME = @UserName 
    AND u.UM_CM_ID = @CompanyId
    AND ur.IsActive = 1
    AND r.IsActive = 1
    AND u.ES_DELETE = 0;
    
    -- If no roles found, return default role
    IF @@ROWCOUNT = 0
    BEGIN
        -- Check if user exists and assign default role
        IF EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = @UserName AND UM_CM_ID = @CompanyId AND ES_DELETE = 0)
        BEGIN
            -- Assign ReadOnlyManager as default role if no specific roles found
            SELECT 'ReadOnlyManager' AS RoleName;
        END
    END
END
GO

-- 6. Sample data - Assign roles to existing users
-- You can modify this based on your requirements

-- Example: Assign Admin role to a specific user
-- INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
-- SELECT u.UserId, r.RoleId, 'System'
-- FROM USER_MASTER u, Roles r
-- WHERE u.UM_USERNAME = 'admin' -- Replace with actual admin username
-- AND r.RoleName = 'Admin';

-- Example: Assign SalesManager role to sales users
-- INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
-- SELECT u.UserId, r.RoleId, 'System'
-- FROM USER_MASTER u, Roles r
-- WHERE u.UM_USERNAME IN ('sales1', 'sales2') -- Replace with actual sales usernames
-- AND r.RoleName = 'SalesManager';

PRINT 'Role-based authorization setup completed successfully!';
PRINT 'Please assign roles to users by inserting records into UserRoles table.';
PRINT 'Example:';
PRINT 'INSERT INTO UserRoles (UserId, RoleId, CreatedBy)';
PRINT 'SELECT u.UserId, r.RoleId, ''System''';
PRINT 'FROM USER_MASTER u, Roles r';
PRINT 'WHERE u.UM_USERNAME = ''your_username''';
PRINT 'AND r.RoleName = ''Admin'';';
