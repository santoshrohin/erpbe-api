CREATE OR ALTER PROCEDURE [dbo].[SP_GetUsersByCompany]
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UM_CODE] as UserId,
        [UM_USERNAME] as Username,
        [UM_NAME] as Name,
        [UM_EMAIL] as Email,
        [UM_CM_ID] as CompanyId,
        [UM_LEVEL] as FinancialYearCode,
        [IS_ACTIVE] as IsActive,
        [UM_IS_ADMIN] as IsAdmin,
        [UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        [UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER]
    WHERE [UM_CM_ID] = @CompanyId AND [ES_DELETE] = 0
    ORDER BY [UM_NAME]
END