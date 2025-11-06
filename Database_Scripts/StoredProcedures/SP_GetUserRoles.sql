IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetUserRoles]') AND type in (N'P', N'PC'))
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

