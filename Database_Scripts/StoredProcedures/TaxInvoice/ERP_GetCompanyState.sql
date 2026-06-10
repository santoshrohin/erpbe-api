-- =============================================
-- Description: Get company state code from COMPANY_MASTER
-- Used in: Tax Invoice form - determine CGST+SGST vs IGST based on customer state
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetCompanyState]
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CM.CM_ID    AS CompanyCode,
        CM.CM_STATE AS StateCode,
        SM.SM_NAME  AS StateName
    FROM COMPANY_MASTER CM
    LEFT JOIN STATE_MASTER SM ON CM.CM_STATE = SM.SM_CODE
    WHERE CM.CM_ID = @CompanyCode
    AND CM.CM_ACTIVE_IND = 1;
END
GO
