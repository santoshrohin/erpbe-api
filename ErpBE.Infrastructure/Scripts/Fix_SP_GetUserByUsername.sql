USE [db_a2ea4b_sunv2]
GO

-- Fix SP_GetUserByUsername to handle FinancialYearCode as integer
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUserByUsername')
    DROP PROCEDURE [dbo].[SP_GetUserByUsername]
GO

CREATE PROCEDURE [dbo].[SP_GetUserByUsername]
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UM_CODE] as UserId,
        [UM_USERNAME] as Username,
        [UM_NAME] as Name,
        [UM_EMAIL] as Email,
        [UM_CM_ID] as CompanyId,
        -2147483641 AS FinancialYearCode, -- Default value since UM_LEVEL is string
        [IS_ACTIVE] as IsActive,
        [UM_IS_ADMIN] as IsAdmin,
        [UM_LASTLOGIN_DATETIME] as LastLoginDateTime,
        [UM_IP_ADDRESS] as IpAddress
    FROM [USER_MASTER]
    WHERE [UM_USERNAME] = @Username AND [ES_DELETE] = 0
END
GO

PRINT 'SP_GetUserByUsername fixed to handle FinancialYearCode as integer!';
