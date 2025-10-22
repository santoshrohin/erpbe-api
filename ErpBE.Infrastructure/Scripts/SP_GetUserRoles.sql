CREATE PROCEDURE SP_GetUserRoles
    @UserName NVARCHAR(50),
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- This is a sample implementation. You'll need to adjust based on your actual user-role table structure
    -- Assuming you have a UserRoles table with UserId, RoleId, and a Roles table with RoleName
    
    SELECT r.RoleName
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    INNER JOIN Users u ON ur.UserId = u.UserId
    WHERE u.UserName = @UserName 
    AND u.CompanyId = @CompanyId
    AND ur.IsActive = 1
    AND r.IsActive = 1;
    
    -- If you don't have a proper role system yet, you can return a default role based on user type
    -- For example:
    -- IF EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName AND UserType = 'Admin')
    --     SELECT 'Admin'
    -- ELSE IF EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName AND UserType = 'Manager')
    --     SELECT 'SalesManager'
    -- ELSE
    --     SELECT 'ReadOnlyManager'
END
