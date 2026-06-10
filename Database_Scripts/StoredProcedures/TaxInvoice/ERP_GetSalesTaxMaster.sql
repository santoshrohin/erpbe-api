-- =============================================
-- Description: Get Sales Tax names/codes for Tax Invoice header Tax Name dropdown
-- Used in: Tax Invoice form - Tax Name dropdown
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetSalesTaxMaster]
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ST_CODE         AS TaxCode,
        ST_TAX_NAME     AS TaxName,
        ISNULL(ST_SALES_TAX, 0) AS TaxRate
    FROM SALES_TAX_MASTER
    WHERE ISNULL(ES_DELETE, 0) = 0
      AND ST_CM_COMP_ID = @CompanyCode
    ORDER BY ST_TAX_NAME;
END
GO
