USE [db_a2ea4b_sunv2]
GO

-- Create SP_GetRoleById stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetRoleById')
    DROP PROCEDURE [dbo].[SP_GetRoleById]
GO

CREATE PROCEDURE [dbo].[SP_GetRoleById]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        RoleId,
        RoleName,
        RoleDescription AS Description,
        IsActive
    FROM ROLES
    WHERE RoleId = @RoleId;
END
GO

PRINT 'SP_GetRoleById stored procedure created successfully!';
