CREATE PROCEDURE [dbo].[SP_GetUserByUsername]
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    SELECT 
        UM_CODE AS UserId,
        UM_USERNAME AS Username,
        UM_NAME AS Name,
        UM_EMAIL AS Email,
        UM_CM_ID AS CompanyId,
        -2147483641 AS FinancialYearCode, -- Default value since UM_LEVEL is string
        CASE WHEN IS_ACTIVE = 1 THEN 1 ELSE 0 END AS IsActive,
        CASE WHEN UM_IS_ADMIN = 1 THEN 1 ELSE 0 END AS IsAdmin,
        UM_LASTLOGIN_DATETIME AS LastLoginDateTime,
        UM_IP_ADDRESS AS IpAddress
    FROM USER_MASTER
    WHERE UM_USERNAME = @Username
      AND (ES_DELETE = 0 OR ES_DELETE IS NULL);
END
