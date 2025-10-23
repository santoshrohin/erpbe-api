-- =============================================
-- Author:      ERP Development Team  
-- Create date: 2025-01-23
-- Description: Get user roles by user ID (replaces inline query in GetUserRolesAsync)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserRolesByUserId]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get username and company ID for the user
    DECLARE @UserName NVARCHAR(100);
    DECLARE @CompanyId NVARCHAR(50);

    SELECT 
        @UserName = UM_USERNAME,
        @CompanyId = CAST(UM_CM_ID AS NVARCHAR(50))
    FROM USER_MASTER
    WHERE UM_CODE = @UserId;

    -- If user not found, return empty result
    IF @UserName IS NULL
        RETURN;

    -- Get roles using the existing SP_GetUserRoles stored procedure
    EXEC SP_GetUserRoles @UserName, @CompanyId;
END
GO

