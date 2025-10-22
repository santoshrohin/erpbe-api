USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Get User Roles for Authorization>
-- =============================================

CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserName VARCHAR(50),
    @CompanyId VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get user roles based on UserRoles and Roles tables
    -- Assuming USER_MASTER has a UserId field that links to UserRoles
    SELECT r.RoleName
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    INNER JOIN USER_MASTER u ON ur.UserId = u.UserId  -- Assuming USER_MASTER has UserId field
    WHERE u.UM_USERNAME = @UserName 
    AND u.UM_CM_ID = @CompanyId
    AND ur.IsActive = 1
    AND r.IsActive = 1
    AND u.ES_DELETE = 0;
    
    -- If no roles found, return a default role based on user type
    -- You can modify this logic based on your business requirements
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
