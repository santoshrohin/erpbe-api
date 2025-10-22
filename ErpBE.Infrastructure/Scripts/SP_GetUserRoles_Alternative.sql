USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Get User Roles for Authorization - Alternative Version>
-- =============================================

CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserName VARCHAR(50),
    @CompanyId VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Alternative approach if USER_MASTER doesn't have UserId field
    -- This assumes you'll add a UserId field to USER_MASTER or create a mapping
    
    -- Option 1: If you add UserId to USER_MASTER
    -- SELECT r.RoleName
    -- FROM UserRoles ur
    -- INNER JOIN Roles r ON ur.RoleId = r.RoleId
    -- INNER JOIN USER_MASTER u ON ur.UserId = u.UserId
    -- WHERE u.UM_USERNAME = @UserName 
    -- AND u.UM_CM_ID = @CompanyId
    -- AND ur.IsActive = 1
    -- AND r.IsActive = 1
    -- AND u.ES_DELETE = 0;
    
    -- Option 2: Temporary solution - assign roles based on username/company
    -- You can modify this logic based on your business requirements
    DECLARE @UserId INT;
    
    -- Try to get UserId from USER_MASTER (if you add this field)
    SELECT @UserId = UserId FROM USER_MASTER 
    WHERE UM_USERNAME = @UserName AND UM_CM_ID = @CompanyId AND ES_DELETE = 0;
    
    IF @UserId IS NOT NULL
    BEGIN
        -- Get roles from UserRoles table
        SELECT r.RoleName
        FROM UserRoles ur
        INNER JOIN Roles r ON ur.RoleId = r.RoleId
        WHERE ur.UserId = @UserId
        AND ur.IsActive = 1
        AND r.IsActive = 1;
    END
    
    -- If no roles found or UserId doesn't exist, return default role
    IF @@ROWCOUNT = 0
    BEGIN
        -- Check if user exists
        IF EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = @UserName AND UM_CM_ID = @CompanyId AND ES_DELETE = 0)
        BEGIN
            -- Assign ReadOnlyManager as default role
            SELECT 'ReadOnlyManager' AS RoleName;
        END
    END
END
