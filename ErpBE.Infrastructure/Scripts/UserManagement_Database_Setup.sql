-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-19
-- Description: Create User Management tables and stored procedures
-- Database: db_a2ea4b_farmerplacement
-- =============================================

USE [db_a2ea4b_sunv2]
GO

-- Create UserRoles table if it doesn't exist (for many-to-many relationship)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserRoles](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [RoleId] [int] NOT NULL,
        [AssignedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [AssignedBy] [int] NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY ([UserId]) REFERENCES [USER_MASTER]([UM_CODE]),
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]),
        CONSTRAINT [UK_UserRoles_User_Role] UNIQUE ([UserId], [RoleId])
    )
    
    CREATE NONCLUSTERED INDEX [IX_UserRoles_UserId] ON [dbo].[UserRoles] ([UserId])
    CREATE NONCLUSTERED INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles] ([RoleId])
    
    PRINT 'UserRoles table created successfully'
END
ELSE
BEGIN
    PRINT 'UserRoles table already exists'
END
GO

-- =============================================
-- User Management Stored Procedures
-- =============================================

-- Create User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CreateUser')
    DROP PROCEDURE [dbo].[SP_CreateUser]
GO

CREATE PROCEDURE [dbo].[SP_CreateUser]
    @Username VARCHAR(50),
    @Password VARCHAR(100),
    @Name VARCHAR(100),
    @Email VARCHAR(100) = NULL,
    @CompanyId INT,
    @FinancialYearCode INT,
    @IsActive BIT = 1,
    @IsAdmin BIT = 0,
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO [USER_MASTER] (
            [UM_USERNAME],
            [UM_PASSWORD],
            [UM_NAME],
            [UM_EMAIL],
            [UM_CM_ID],
            [UM_LEVEL],
            [IS_ACTIVE],
            [UM_IS_ADMIN],
            [ES_DELETE]
        )
        VALUES (
            @Username,
            @Password,
            @Name,
            @Email,
            @CompanyId,
            'User',
            @IsActive,
            @IsAdmin,
            0
        )
        
        SET @UserId = SCOPE_IDENTITY()
        
        SELECT @UserId as UserId
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Update User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_UpdateUser')
    DROP PROCEDURE [dbo].[SP_UpdateUser]
GO

CREATE PROCEDURE [dbo].[SP_UpdateUser]
    @UserId INT,
    @Name VARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @IsActive BIT = NULL,
    @IsAdmin BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [USER_MASTER]
        SET 
            [UM_NAME] = ISNULL(@Name, [UM_NAME]),
            [UM_EMAIL] = ISNULL(@Email, [UM_EMAIL]),
            [IS_ACTIVE] = ISNULL(@IsActive, [IS_ACTIVE]),
            [UM_IS_ADMIN] = ISNULL(@IsAdmin, [UM_IS_ADMIN])
        WHERE [UM_CODE] = @UserId
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Delete User (Soft Delete)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_DeleteUser')
    DROP PROCEDURE [dbo].[SP_DeleteUser]
GO

CREATE PROCEDURE [dbo].[SP_DeleteUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [USER_MASTER]
        SET [ES_DELETE] = 1
        WHERE [UM_CODE] = @UserId
        
        -- Remove all role assignments
        DELETE FROM [UserRoles] WHERE [UserId] = @UserId
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Get User by ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUserById')
    DROP PROCEDURE [dbo].[SP_GetUserById]
GO

CREATE PROCEDURE [dbo].[SP_GetUserById]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UM_CODE] as UserId,
        [UM_USERNAME] as Username,
        [UM_NAME] as Name,
        [UM_EMAIL] as Email,
        [UM_CM_ID] as CompanyId,
        [UM_LEVEL] as FinancialYearCode,
        [IS_ACTIVE] as IsActive,
        [UM_IS_ADMIN] as IsAdmin,
        [UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        [UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER]
    WHERE [UM_CODE] = @UserId AND [ES_DELETE] = 0
END
GO

-- Get User by Username
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUserByUsername')
    DROP PROCEDURE [dbo].[SP_GetUserByUsername]
GO

CREATE PROCEDURE [dbo].[SP_GetUserByUsername]
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UM_CODE] as UserId,
        [UM_USERNAME] as Username,
        [UM_NAME] as Name,
        [UM_EMAIL] as Email,
        [UM_CM_ID] as CompanyId,
        [UM_LEVEL] as FinancialYearCode,
        [IS_ACTIVE] as IsActive,
        [UM_IS_ADMIN] as IsAdmin,
        [UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        [UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER]
    WHERE [UM_USERNAME] = @Username AND [ES_DELETE] = 0
END
GO

-- Get All Users
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAllUsers')
    DROP PROCEDURE [dbo].[SP_GetAllUsers]
GO

CREATE PROCEDURE [dbo].[SP_GetAllUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UM_CODE] as UserId,
        [UM_USERNAME] as Username,
        [UM_NAME] as Name,
        [UM_EMAIL] as Email,
        [UM_CM_ID] as CompanyId,
        [UM_LEVEL] as FinancialYearCode,
        [IS_ACTIVE] as IsActive,
        [UM_IS_ADMIN] as IsAdmin,
        [UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        [UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER]
    WHERE [ES_DELETE] = 0
    ORDER BY [UM_NAME]
END
GO

-- Get Users by Company
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUsersByCompany')
    DROP PROCEDURE [dbo].[SP_GetUsersByCompany]
GO

CREATE PROCEDURE [dbo].[SP_GetUsersByCompany]
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UM_CODE] as UserId,
        [UM_USERNAME] as Username,
        [UM_NAME] as Name,
        [UM_EMAIL] as Email,
        [UM_CM_ID] as CompanyId,
        [UM_LEVEL] as FinancialYearCode,
        [IS_ACTIVE] as IsActive,
        [UM_IS_ADMIN] as IsAdmin,
        [UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        [UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER]
    WHERE [UM_CM_ID] = @CompanyId AND [ES_DELETE] = 0
    ORDER BY [UM_NAME]
END
GO

-- Change Password
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ChangePassword')
    DROP PROCEDURE [dbo].[SP_ChangePassword]
GO

CREATE PROCEDURE [dbo].[SP_ChangePassword]
    @UserId INT,
    @NewPassword VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [USER_MASTER]
        SET [UM_PASSWORD] = @NewPassword
        WHERE [UM_CODE] = @UserId AND [ES_DELETE] = 0
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Activate User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ActivateUser')
    DROP PROCEDURE [dbo].[SP_ActivateUser]
GO

CREATE PROCEDURE [dbo].[SP_ActivateUser]
    @UserId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [USER_MASTER]
        SET [IS_ACTIVE] = @IsActive
        WHERE [UM_CODE] = @UserId AND [ES_DELETE] = 0
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- =============================================
-- Role Management Stored Procedures
-- =============================================

-- Create Role
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CreateRole')
    DROP PROCEDURE [dbo].[SP_CreateRole]
GO

CREATE PROCEDURE [dbo].[SP_CreateRole]
    @RoleName VARCHAR(50),
    @Description VARCHAR(200) = NULL,
    @IsActive BIT = 1,
    @RoleId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO [Roles] (
            [RoleName],
            RoleDescription,
            [IsActive]
        )
        VALUES (
            @RoleName,
            @Description,
            @IsActive
        )
        
        SET @RoleId = SCOPE_IDENTITY()
        
        SELECT @RoleId as RoleId
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Update Role
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_UpdateRole')
    DROP PROCEDURE [dbo].[SP_UpdateRole]
GO

CREATE PROCEDURE [dbo].[SP_UpdateRole]
    @RoleId INT,
    @RoleName VARCHAR(50) = NULL,
    @Description VARCHAR(200) = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [Roles]
        SET 
            [RoleName] = ISNULL(@RoleName, [RoleName]),
            RoleDescription = ISNULL(@Description, RoleDescription),
            [IsActive] = ISNULL(@IsActive, [IsActive])
        WHERE RoleId = @RoleId
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Delete Role
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_DeleteRole')
    DROP PROCEDURE [dbo].[SP_DeleteRole]
GO

CREATE PROCEDURE [dbo].[SP_DeleteRole]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Remove all user assignments first
        DELETE FROM [UserRoles] WHERE [RoleId] = @RoleId
        
        -- Delete the role
        DELETE FROM [Roles] WHERE RoleId = @RoleId
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Get Role by ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetRoleById')
    DROP PROCEDURE [dbo].[SP_GetRoleById]
GO

CREATE PROCEDURE [dbo].[SP_GetRoleById]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RoleId as RoleId,
        [RoleName],
        RoleDescription,
        [IsActive]
    FROM [Roles]
    WHERE RoleId = @RoleId
END
GO

-- Get Role by Name
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetRoleByName')
    DROP PROCEDURE [dbo].[SP_GetRoleByName]
GO

CREATE PROCEDURE [dbo].[SP_GetRoleByName]
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RoleId as RoleId,
        [RoleName],
        RoleDescription,
        [IsActive]
    FROM [Roles]
    WHERE [RoleName] = @RoleName
END
GO

-- Get All Roles
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAllRoles')
    DROP PROCEDURE [dbo].[SP_GetAllRoles]
GO

CREATE PROCEDURE [dbo].[SP_GetAllRoles]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RoleId as RoleId,
        [RoleName],
        RoleDescription,
        [IsActive]
    FROM [Roles]
    ORDER BY [RoleName]
END
GO

-- Get Active Roles
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetActiveRoles')
    DROP PROCEDURE [dbo].[SP_GetActiveRoles]
GO

CREATE PROCEDURE [dbo].[SP_GetActiveRoles]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RoleId as RoleId,
        [RoleName],
        RoleDescription,
        [IsActive]
    FROM [Roles]
    WHERE [IsActive] = 1
    ORDER BY [RoleName]
END
GO

-- =============================================
-- User-Role Assignment Stored Procedures
-- =============================================

-- Assign Role to User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_AssignRoleToUser')
    DROP PROCEDURE [dbo].[SP_AssignRoleToUser]
GO

CREATE PROCEDURE [dbo].[SP_AssignRoleToUser]
    @UserId INT,
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @RoleId INT
        
        -- Get Role ID by name
        SELECT @RoleId = RoleId FROM [Roles] WHERE [RoleName] = @RoleName
        
        IF @RoleId IS NULL
        BEGIN
            RAISERROR('Role not found: %s', 16, 1, @RoleName)
            RETURN
        END
        
        -- Insert if not exists
        IF NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId)
        BEGIN
            INSERT INTO [UserRoles] ([UserId], [RoleId], [IsActive])
            VALUES (@UserId, @RoleId, 1)
        END
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Remove Role from User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_RemoveRoleFromUser')
    DROP PROCEDURE [dbo].[SP_RemoveRoleFromUser]
GO

CREATE PROCEDURE [dbo].[SP_RemoveRoleFromUser]
    @UserId INT,
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @RoleId INT
        
        -- Get Role ID by name
        SELECT @RoleId = RoleId FROM [Roles] WHERE [RoleName] = @RoleName
        
        IF @RoleId IS NOT NULL
        BEGIN
            DELETE FROM [UserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId
        END
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Remove All Roles from User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_RemoveAllRolesFromUser')
    DROP PROCEDURE [dbo].[SP_RemoveAllRolesFromUser]
GO

CREATE PROCEDURE [dbo].[SP_RemoveAllRolesFromUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DELETE FROM [UserRoles] WHERE [UserId] = @UserId
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Get User Roles
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUserRoles')
    DROP PROCEDURE [dbo].[SP_GetUserRoles]
GO

CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT r.[RoleName]
    FROM [UserRoles] ur
    INNER JOIN [Roles] r ON ur.[RoleId] = r.RoleId
    WHERE ur.[UserId] = @UserId AND r.[IsActive] = 1
    ORDER BY r.[RoleName]
END
GO

-- Get Users by Role
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUsersByRole')
    DROP PROCEDURE [dbo].[SP_GetUsersByRole]
GO

CREATE PROCEDURE [dbo].[SP_GetUsersByRole]
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.[UM_CODE] as UserId,
        u.[UM_USERNAME] as Username,
        u.[UM_NAME] as Name,
        u.[UM_EMAIL] as Email,
        u.[UM_CM_ID] as CompanyId,
        u.[UM_LEVEL] as FinancialYearCode,
        u.[IS_ACTIVE] as IsActive,
        u.[UM_IS_ADMIN] as IsAdmin,
        u.[UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        u.[UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER] u
    INNER JOIN [UserRoles] ur ON u.[UM_CODE] = ur.[UserId]
    INNER JOIN [Roles] r ON ur.[RoleId] = r.RoleId
    WHERE r.[RoleName] = @RoleName AND u.[ES_DELETE] = 0
    ORDER BY u.[UM_NAME]
END
GO

-- =============================================
-- Validation Stored Procedures
-- =============================================

-- Check Username Exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CheckUsernameExists')
    DROP PROCEDURE [dbo].[SP_CheckUsernameExists]
GO

CREATE PROCEDURE [dbo].[SP_CheckUsernameExists]
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM [USER_MASTER]
    WHERE [UM_USERNAME] = @Username AND [ES_DELETE] = 0
END
GO

-- Check Email Exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CheckEmailExists')
    DROP PROCEDURE [dbo].[SP_CheckEmailExists]
GO

CREATE PROCEDURE [dbo].[SP_CheckEmailExists]
    @Email VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM [USER_MASTER]
    WHERE [UM_EMAIL] = @Email AND [ES_DELETE] = 0
END
GO

-- Check Role Exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CheckRoleExists')
    DROP PROCEDURE [dbo].[SP_CheckRoleExists]
GO

CREATE PROCEDURE [dbo].[SP_CheckRoleExists]
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM [Roles]
    WHERE [RoleName] = @RoleName
END
GO

PRINT 'User Management database setup completed successfully'
PRINT 'Created tables: UserRoles'
PRINT 'Created procedures: SP_CreateUser, SP_UpdateUser, SP_DeleteUser, SP_GetUserById, SP_GetUserByUsername, SP_GetAllUsers, SP_GetUsersByCompany, SP_ChangePassword, SP_ActivateUser'
PRINT 'Created procedures: SP_CreateRole, SP_UpdateRole, SP_DeleteRole, SP_GetRoleById, SP_GetRoleByName, SP_GetAllRoles, SP_GetActiveRoles'
PRINT 'Created procedures: SP_AssignRoleToUser, SP_RemoveRoleFromUser, SP_RemoveAllRolesFromUser, SP_GetUserRoles, SP_GetUsersByRole'
PRINT 'Created procedures: SP_CheckUsernameExists, SP_CheckEmailExists, SP_CheckRoleExists'
