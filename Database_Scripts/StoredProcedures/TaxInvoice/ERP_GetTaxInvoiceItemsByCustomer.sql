-- =============================================
-- Description: Get items available for invoicing for a specific customer
-- Filters to items with pending PO quantities (CPOD_ORD_QTY - CPOD_DISPACH > 0)
-- Used in: Tax Invoice form - Item dropdown (after customer selected)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoiceItemsByCustomer]
    @CustomerCode INT,
    @CompanyCode  INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        IM.I_CODE       AS ItemCode,
        IM.I_NAME       AS ItemName,
        IM.I_CODENO     AS ItemCodeDisplay,
        IM.I_UOM_CODE   AS UomCode,
        UOM.I_UOM_NAME  AS UomName
    FROM ITEM_MASTER IM
    INNER JOIN CUSTPO_DETAIL CPD  ON IM.I_CODE = CPD.CPOD_I_CODE
    INNER JOIN CUSTPO_MASTER CPM  ON CPD.CPOD_CPOM_CODE = CPM.CPOM_CODE
    LEFT  JOIN ITEM_UNIT_MASTER UOM ON IM.I_UOM_CODE = UOM.I_UOM_CODE
    WHERE IM.ES_DELETE = 0
      AND IM.I_CM_COMP_ID = @CompanyCode
      AND CPM.CPOM_P_CODE = @CustomerCode
      AND CPM.ES_DELETE = 0
      AND CPM.CPOM_CM_COMP_ID = @CompanyCode
      AND (CPD.CPOD_ORD_QTY - ISNULL(CPD.CPOD_DISPACH, 0)) > 0
    ORDER BY IM.I_NAME;
END
GO
