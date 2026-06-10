-- =============================================
-- Deploy Tax Invoice Lookup Stored Procedures
-- Description: 6 lookup SPs used by the Tax Invoice form dropdowns
-- Run in SSMS or Azure Data Studio against db_a2ea4b_sunv2
-- =============================================

PRINT 'Deploying ERP_GetTaxInvoiceCustomers...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoiceCustomers]
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        PM.P_CODE       AS Id,
        PM.P_NAME       AS DisplayName,
        PM.P_ADD1       AS Address,
        PM.P_SM_CODE    AS StateCode,
        PM.P_LBT_NO     AS GstNumber,
        PM.P_LBT_IND    AS GstApplicable
    FROM PARTY_MASTER PM
    INNER JOIN CUSTPO_MASTER CPM ON PM.P_CODE = CPM.CPOM_P_CODE
    WHERE PM.ES_DELETE = 0
      AND PM.P_TYPE = 1
      AND ISNULL(PM.P_ACTIVE_IND, 0) = 1
      AND PM.P_CM_COMP_ID = @CompanyCode
      AND CPM.ES_DELETE = 0
      AND CPM.CPOM_CM_COMP_ID = @CompanyCode
      AND EXISTS (
          SELECT 1
          FROM CUSTPO_DETAIL CPD
          WHERE CPD.CPOD_CPOM_CODE = CPM.CPOM_CODE
            AND (CPD.CPOD_ORD_QTY - ISNULL(CPD.CPOD_DISPACH, 0)) > 0
      )
    ORDER BY PM.P_NAME;
END
GO

PRINT 'Deploying ERP_GetTaxInvoiceItemsByCustomer...';
GO

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

PRINT 'Deploying ERP_GetTaxInvoiceItemDetails...';
GO

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

PRINT 'Deploying ERP_GetTaxInvoicePOsByItemCustomer...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoicePOsByItemCustomer]
    @ItemCode     INT,
    @CustomerCode INT,
    @CompanyCode  INT,
    @InvoiceCode  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CPM.CPOM_CODE                                                       AS PoCode,
        CPM.CPOM_PONO                                                       AS PoNumber,
        ISNULL(CPM.CPOM_DATE, CPM.CPOM_PO_DATE)                            AS PoDate,
        (CPD.CPOD_ORD_QTY - ISNULL(CPD.CPOD_DISPACH, 0))                  AS PendingQuantity,
        CPD.CPOD_ORD_QTY                                                    AS OrderedQuantity,
        ISNULL(CPD.CPOD_DISPACH, 0)                                        AS DispatchedQuantity,
        ISNULL(CPD.CPOD_RATE, 0)                                           AS Rate,
        ISNULL(CPD.CPOD_DISC_AMT, 0)                                       AS DiscountAmount,
        ISNULL(CPD.CPOD_RATE, 0)
            - ISNULL(CPD.CPOD_DISC_AMT / NULLIF(CPD.CPOD_ORD_QTY, 0), 0) AS NetRate,
        ISNULL(CPD.CPOD_AMORTRATE, 0)                                      AS AmortRate,
        ISNULL(CPD.CPOD_ST_CODE, 0)                                        AS TaxCode,
        CPD.CPOD_UOM_CODE                                                   AS UomCode
    FROM CUSTPO_MASTER CPM
    INNER JOIN CUSTPO_DETAIL CPD ON CPM.CPOM_CODE = CPD.CPOD_CPOM_CODE
    WHERE CPM.CPOM_P_CODE        = @CustomerCode
      AND CPD.CPOD_I_CODE        = @ItemCode
      AND CPM.CPOM_CM_COMP_ID    = @CompanyCode
      AND CPM.ES_DELETE          = 0
      AND (
          (CPD.CPOD_ORD_QTY - ISNULL(CPD.CPOD_DISPACH, 0)) > 0
          OR
          (@InvoiceCode IS NOT NULL AND EXISTS (
              SELECT 1 FROM INVOICE_DETAIL ID
              WHERE ID.IND_CPOM_CODE = CPM.CPOM_CODE
                AND ID.IND_I_CODE    = @ItemCode
                AND ID.IND_INM_CODE  = @InvoiceCode
                AND ISNULL(ID.ES_DELETE, 0) = 0
          ))
      )
    ORDER BY CPM.CPOM_DATE DESC;
END
GO

PRINT 'Deploying ERP_GetCompanyState...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetCompanyState]
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CM.CM_CODE  AS CompanyCode,
        CM.CM_STATE AS StateCode,
        SM.SM_NAME  AS StateName
    FROM COMPANY_MASTER CM
    LEFT JOIN STATE_MASTER SM ON CM.CM_STATE = SM.SM_CODE
    WHERE CM.CM_CODE = @CompanyCode;
END
GO

PRINT 'Deploying ERP_GetSalesTaxMaster...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetSalesTaxMaster]
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ST_CODE                     AS TaxCode,
        ST_TAX_NAME                 AS TaxName,
        ISNULL(ST_SALES_TAX, 0)     AS TaxRate
    FROM SALES_TAX_MASTER
    WHERE ISNULL(ES_DELETE, 0) = 0
      AND ST_CM_COMP_ID = @CompanyCode
    ORDER BY ST_TAX_NAME;
END
GO

PRINT 'All 6 Tax Invoice lookup SPs deployed successfully.';
GO
