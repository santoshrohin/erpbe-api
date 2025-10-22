USE [db_a2ea4b_sunv2]
GO

-- Create SP_CreateRole stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CreateRole')
    DROP PROCEDURE [dbo].[SP_CreateRole]
GO

CREATE PROCEDURE [dbo].[SP_CreateRole]
    @RoleName NVARCHAR(100),
    @RoleDescription NVARCHAR(500) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ROLES (RoleName, RoleDescription, IsActive)
    VALUES (@RoleName, @RoleDescription, @IsActive);

    SELECT SCOPE_IDENTITY() AS RoleId;
END
GO

-- Create SP_UpdateRole stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_UpdateRole')
    DROP PROCEDURE [dbo].[SP_UpdateRole]
GO

CREATE PROCEDURE [dbo].[SP_UpdateRole]
    @RoleId INT,
    @RoleName NVARCHAR(100),
    @RoleDescription NVARCHAR(500) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ROLES 
    SET RoleName = @RoleName,
        RoleDescription = @RoleDescription,
        IsActive = @IsActive
    WHERE RoleId = @RoleId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Create SP_DeleteRole stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_DeleteRole')
    DROP PROCEDURE [dbo].[SP_DeleteRole]
GO

CREATE PROCEDURE [dbo].[SP_DeleteRole]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM ROLES WHERE RoleId = @RoleId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Create SP_RoleExists stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_RoleExists')
    DROP PROCEDURE [dbo].[SP_RoleExists]
GO

CREATE PROCEDURE [dbo].[SP_RoleExists]
    @RoleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN EXISTS (SELECT 1 FROM ROLES WHERE RoleName = @RoleName) THEN 1 ELSE 0 END AS Exists;
END
GO

PRINT 'Role management stored procedures created successfully!';
