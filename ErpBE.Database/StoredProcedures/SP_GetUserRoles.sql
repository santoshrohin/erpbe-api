CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserName NVARCHAR(50),
    @CompanyId NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    SELECT r.RoleName
    FROM USER_MASTER um
    JOIN UserRoles ur ON um.UM_CODE = ur.UserId
    JOIN ROLES r ON ur.RoleId = r.RoleId
    WHERE um.UM_USERNAME = @UserName 
      AND um.UM_CM_ID = CAST(@CompanyId AS INT)
      AND ur.IsActive = 1
      AND r.IsActive = 1;
END
