-- =============================================
-- Description: Get item details for Tax Invoice line:
--   - UOM code/name from ITEM_UNIT_MASTER
--   - Current stock from STOCK_LEDGER (STL_STORE_TYPE = -2147483648)
--   - HSN code, CGST%, SGST%, IGST% from EXCISE_TARIFF_MASTER
-- Used in: Tax Invoice form - auto-fill when item is selected
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoiceItemDetails]
    @ItemCode    INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IM.I_CODE                           AS ItemCode,
        IM.I_NAME                           AS ItemName,
        IM.I_CODENO                         AS ItemCodeDisplay,
        IM.I_UOM_CODE                       AS UomCode,
        ISNULL(UOM.I_UOM_NAME, '')          AS UomName,
        ISNULL(SUM(SL.STL_DOC_QTY), 0)     AS StockQuantity,
        ISNULL(ETM.E_TARIFF_NO, '')         AS HsnCode,
        ISNULL(ETM.E_BASIC,    0)           AS CgstPercentage,
        ISNULL(ETM.E_EDU_CESS, 0)           AS SgstPercentage,
        ISNULL(ETM.E_H_EDU,    0)           AS IgstPercentage
    FROM ITEM_MASTER IM
    LEFT JOIN ITEM_UNIT_MASTER   UOM ON IM.I_UOM_CODE = UOM.I_UOM_CODE
    LEFT JOIN EXCISE_TARIFF_MASTER ETM ON IM.I_E_CODE = ETM.E_CODE
    LEFT JOIN STOCK_LEDGER        SL  ON IM.I_CODE = SL.STL_I_CODE
                                      AND SL.STL_STORE_TYPE = -2147483648
    WHERE IM.I_CODE = @ItemCode
    GROUP BY
        IM.I_CODE, IM.I_NAME, IM.I_CODENO, IM.I_UOM_CODE,
        UOM.I_UOM_NAME, ETM.E_TARIFF_NO, ETM.E_BASIC, ETM.E_EDU_CESS, ETM.E_H_EDU;
END
GO
