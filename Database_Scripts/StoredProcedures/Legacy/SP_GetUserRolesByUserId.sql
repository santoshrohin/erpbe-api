CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserRolesByUserId]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get username and company ID from USER_MASTER
    DECLARE @UserName NVARCHAR(50);
    DECLARE @CompanyId NVARCHAR(50);

    SELECT
        @UserName = UM_USERNAME,
        @CompanyId = CAST(UM_CM_ID AS NVARCHAR(50))
    FROM USER_MASTER
    WHERE UM_CODE = @UserId;

    -- If user info is found, call SP_GetUserRoles
    IF @UserName IS NOT NULL AND @CompanyId IS NOT NULL
    BEGIN
        EXEC SP_GetUserRoles @UserName, @CompanyId;
    END
    ELSE
    BEGIN
        -- Return an empty result set if user not found
        SELECT CAST(NULL AS NVARCHAR(50)) AS RoleName WHERE 1=0;
    END
END