CREATE OR ALTER PROCEDURE [dbo].[SP_GetUsersByRole]
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.[UM_CODE] as UserId,
        u.[UM_USERNAME] as Username,
        u.[UM_NAME] as Name,
        u.[UM_EMAIL] as Email,
        u.[UM_CM_ID] as CompanyId,
        u.[UM_LEVEL] as FinancialYearCode,
        u.[IS_ACTIVE] as IsActive,
        u.[UM_IS_ADMIN] as IsAdmin,
        u.[UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        u.[UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER] u
    INNER JOIN [UserRoles] ur ON u.[UM_CODE] = ur.[UserId]
    INNER JOIN [Roles] r ON ur.[RoleId] = r.RoleId
    WHERE r.[RoleName] = @RoleName AND u.[ES_DELETE] = 0
    ORDER BY u.[UM_NAME]
END