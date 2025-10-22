USE [db_a2ea4b_sunv2]
GO

-- Create SP_GetUserRoles stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUserRoles')
    DROP PROCEDURE [dbo].[SP_GetUserRoles]
GO

CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserName NVARCHAR(50),
    @CompanyId NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.RoleName
    FROM USER_MASTER um
    JOIN UserRoles ur ON um.UM_CODE = ur.UserId
    JOIN ROLES r ON ur.RoleId = r.RoleId
    WHERE um.UM_USERNAME = @UserName 
      AND um.UM_CM_ID = CAST(@CompanyId AS INT)
      AND ur.IsActive = 1
      AND r.IsActive = 1;
END
GO

PRINT 'SP_GetUserRoles stored procedure created successfully!';
