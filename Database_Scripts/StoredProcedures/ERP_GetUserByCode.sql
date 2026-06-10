-- =============================================================================
-- ERP_GetUserByCode
-- Fetches a user record by UM_CODE. Used during refresh token flow to
-- reload user context (company, FY, permissions) for new JWT generation.
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserByCode]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_GetUserByCode]
GO

CREATE PROCEDURE [dbo].[ERP_GetUserByCode]
    @UserCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Returns the most recent active company context for this user.
    -- During refresh we use the same company/FY context as the original login.
    SELECT TOP 1
        um.UM_CODE          AS UserCode,
        um.UM_USERNAME      AS Username,
        um.UM_NAME          AS DisplayName,
        um.UM_EMAIL         AS UserEmail,
        ISNULL(um.UM_IS_ADMIN, 0) AS IsAdmin,
        um.UM_CM_ID         AS CompanyId,
        cm.CM_CODE          AS FinancialYearCode,
        cm.CM_NAME          AS CompanyName,
        cm.CM_EMAILID       AS CompanyEmail,
        CONVERT(VARCHAR(10), cm.CM_OPENING_DATE, 103) AS OpeningDate,
        CONVERT(VARCHAR(10), cm.CM_CLOSING_DATE, 103) AS ClosingDate
    FROM USER_MASTER um
    INNER JOIN COMPANY_MASTER cm ON um.UM_CM_ID = cm.CM_ID
    WHERE um.UM_CODE     = @UserCode
      AND um.IS_ACTIVE   = 1
      AND um.ES_DELETE   = 0
      AND cm.CM_ACTIVE_IND = 1;
END
GO
