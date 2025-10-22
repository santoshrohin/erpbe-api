USE [db_a2ea4b_sunv2]
GO

-- Fix SP_RoleExists stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_RoleExists')
    DROP PROCEDURE [dbo].[SP_RoleExists]
GO

CREATE PROCEDURE [dbo].[SP_RoleExists]
    @RoleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN EXISTS (SELECT 1 FROM ROLES WHERE RoleName = @RoleName) THEN 1 ELSE 0 END AS RoleExists;
END
GO

PRINT 'SP_RoleExists fixed successfully!';
