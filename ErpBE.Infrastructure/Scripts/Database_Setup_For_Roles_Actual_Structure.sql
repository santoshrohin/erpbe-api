USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Setup Role-Based Authorization for Actual USER_MASTER Structure>
-- =============================================

-- 1. Create Roles table (if not exists)
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
END
GO

-- 2. Create UserRoles table (if not exists)
-- Using UM_CODE as the unique identifier for users
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserRoles](
        [UserCode] [int] NOT NULL,  -- Using UM_CODE as the user identifier
        [RoleId] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT(1),
        [CreatedDate] [datetime] NOT NULL DEFAULT(GETDATE()),
        [CreatedBy] [varchar](50) NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([UserCode] ASC, [RoleId] ASC),
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserCode]) REFERENCES [dbo].[USER_MASTER] ([UM_CODE])
    );
END
GO

-- 3. Insert default roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO [dbo].[Roles] ([RoleName], [RoleDescription], [CreatedBy]) VALUES
    ('Admin', 'Full system access - can perform all operations', 'System'),
    ('SalesManager', 'Sales module access - can manage sales and placements', 'System'),
    ('StoreManager', 'Store module access - can manage inventory and store operations', 'System'),
    ('PurchaseManager', 'Purchase module access - can manage purchases and suppliers', 'System'),
    ('ReadOnlyManager', 'Read-only access - can view all data but cannot modify', 'System'),
    ('UtilityManager', 'Utility module access - can manage system utilities and configurations', 'System');
END
GO

-- 4. Create SP_GetUserRoles stored procedure
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
    -- Using UM_CODE as the user identifier
    SELECT r.RoleName
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    INNER JOIN USER_MASTER u ON ur.UserCode = u.UM_CODE
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

PRINT 'Role-based authorization setup completed successfully!';
PRINT 'Next step: Run the test users creation script.';
