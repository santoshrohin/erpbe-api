-- =============================================
-- Tax Invoice Print Data - V2 FIXED
-- Matches EXACT invoice format with CORRECT column names
-- COMPANY_MASTER: CM_ID (identity), CM_CODE (fin-year), CM_STATE (int FK→STATE_MASTER.SM_CODE), CM_ACTIVE_IND (bit), no CM_PIN/CM_SM_CODE
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoicePrintData_V2]
    @InvoiceCode BIGINT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare variables
    DECLARE @CustomerCode INT;
    DECLARE @CompanyStateCode INT;
    DECLARE @CustomerStateCode INT;

    -- Get customer code and state codes
    SELECT
        @CustomerCode = INM_P_CODE,
        @CompanyStateCode = CM.CM_STATE,
        @CustomerStateCode = PM.P_STM_CODE
    FROM INVOICE_MASTER INM
    INNER JOIN COMPANY_MASTER CM ON CM.CM_ID = INM.INM_CM_CODE AND CM.CM_ID = @CompanyId
    INNER JOIN PARTY_MASTER PM ON PM.P_CODE = INM.INM_P_CODE
    WHERE INM.INM_CODE = @InvoiceCode
    AND ISNULL(INM.ES_DELETE, 0) = 0
    AND CM.CM_ACTIVE_IND = 1;

    -- ============================================
    -- Result Set 1: Company Information (Header)
    -- ============================================
    SELECT
        CM_NAME AS CompanyName,
        -- Build full address in one line
        ISNULL(CM_ADDRESS1, '') +
        CASE WHEN ISNULL(CM_ADDRESS2, '') <> '' THEN ', ' + CM_ADDRESS2 ELSE '' END +
        CASE WHEN ISNULL(CM_ADDRESS3, '') <> '' THEN ', ' + CM_ADDRESS3 ELSE '' END +
        CASE WHEN ISNULL(SM.SM_NAME, '') <> '' THEN ', ' + SM.SM_NAME ELSE '' END +
        ', India' AS FullAddress,
        ISNULL(CM_GST_NO, '') AS GstinNo
    FROM COMPANY_MASTER CM
    LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = CM.CM_STATE
    WHERE CM.CM_ID = @CompanyId
    AND CM.CM_ACTIVE_IND = 1;

    -- ============================================
    -- Result Set 2: Invoice Header (Left + Right columns)
    -- ============================================
    SELECT
        -- Left Column
        INM_DATE AS DateOfInvoice,
        ISNULL(INM_TNO, '') AS InvoiceSerialNo,
        ISNULL((SELECT TOP 1 CM_GST_NO FROM COMPANY_MASTER WHERE CM_ID = @CompanyId AND CM_ACTIVE_IND = 1), '') AS GstinNo,
        ISNULL(EwayBill, '') AS EWayBillNo,

        -- Right Column
        ISNULL(INM_TRANSPORT, '') AS TransporatationMode,
        ISNULL(INM_VEH_NO, '') AS VehicleNo,
        ISNULL((SELECT TOP 1 CPOM_PONO FROM CUSTPO_MASTER WHERE CPOM_CODE = INM_CPOM_CODE AND ES_DELETE = 0), '') AS PoNo,
        INM_DATE AS DateAndTimeOfSupply,
        ISNULL((SELECT TOP 1 SM_NAME FROM STATE_MASTER SM WHERE SM.SM_CODE = @CompanyStateCode), 'Maharashtra') AS PlaceOfSupply
    FROM INVOICE_MASTER
    WHERE INM_CODE = @InvoiceCode
    AND ISNULL(ES_DELETE, 0) = 0;

    -- ============================================
    -- Result Set 3: Recipient Details (Left side)
    -- ============================================
    SELECT
        ISNULL(P_NAME, '') AS Name,
        ISNULL(P_ADD1, '') +
        CASE WHEN ISNULL(P_CITY, '') <> '' THEN ',Dist-' + P_CITY ELSE '' END +
        CASE WHEN ISNULL(P_PIN_CODE, '') <> '' THEN '-' + P_PIN_CODE ELSE '' END AS Address,
        ISNULL(SM.SM_NAME, '') AS StateName,
        ISNULL(SM.SM_STATE_CODE, '') AS StateCode,
        ISNULL(P_GST_NO, '') AS GstinNo
    FROM PARTY_MASTER PM
    LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = PM.P_STM_CODE
    WHERE PM.P_CODE = @CustomerCode
    AND ISNULL(PM.ES_DELETE, 0) = 0;

    -- ============================================
    -- Result Set 4: Delivery Details (Right side)
    -- Usually same as recipient
    -- ============================================
    SELECT
        ISNULL(P_NAME, '') AS Name,
        ISNULL(P_ADD1, '') +
        CASE WHEN ISNULL(P_CITY, '') <> '' THEN ',Dist-' + P_CITY ELSE '' END +
        CASE WHEN ISNULL(P_PIN_CODE, '') <> '' THEN '-' + P_PIN_CODE ELSE '' END AS Address,
        ISNULL(SM.SM_NAME, '') AS StateName,
        ISNULL(SM.SM_STATE_CODE, '') AS StateCode,
        ISNULL(P_GST_NO, '') AS GstinNo
    FROM PARTY_MASTER PM
    LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = PM.P_STM_CODE
    WHERE PM.P_CODE = @CustomerCode
    AND ISNULL(PM.ES_DELETE, 0) = 0;

    -- ============================================
    -- Result Set 5: Line Items
    -- Sr. No | Description | HSN/SAC | UOM | Qty | Rate/Unit | Taxable Value
    -- ============================================
    SELECT
        ROW_NUMBER() OVER (ORDER BY IND.IND_SR_NO, IND.IND_I_CODE) AS SrNo,
        ISNULL(CAST(IM.I_CODE AS NVARCHAR(50)), '') + ' - ' + ISNULL(IM.I_NAME, '') AS DescriptionOfGoodsOrServices,
        ISNULL(IND.IND_HSN_CODE, '') AS HsnSac,
        ISNULL(UM.I_UOM_NAME, 'NOS') AS Uom,
        ISNULL(IND.IND_INQTY, 0) AS Qty,
        ISNULL(IND.IND_RATE, 0) AS RatePerUnit,
        ISNULL(IND.IND_AMT, 0) AS TaxableValue,
        ISNULL(IND.E_BASIC_CentralT, 0) AS CgstPercentage,
        ISNULL(IND.E_EDU_CESS_State, 0) AS SgstPercentage,
        ISNULL(IND.E_H_EDU_Integrated, 0) AS IgstPercentage
    FROM INVOICE_DETAIL IND
    INNER JOIN ITEM_MASTER IM ON IM.I_CODE = IND.IND_I_CODE
    LEFT JOIN ITEM_UNIT_MASTER UM ON UM.I_UOM_CODE = IND.IND_UOM_CODE
    WHERE IND.IND_INM_CODE = @InvoiceCode
    AND ISNULL(IND.ES_DELETE, 0) = 0
    ORDER BY IND.IND_SR_NO, IND.IND_I_CODE;

    -- ============================================
    -- Result Set 6: Totals and Tax Summary
    -- ============================================
    SELECT
        ISNULL(INM.INM_DISC_AMT, 0) AS Discount,
        ISNULL(INM.INM_PACK_AMT, 0) AS PackingAndForwardingCharges,
        ISNULL(INM.INM_FREIGHT, 0) + ISNULL(INM.INM_INSURANCE, 0) AS FrieghtAndInsurance,
        ISNULL(INM.INM_OTHER_AMT, 0) AS OtherCharges,

        ISNULL(INM.INM_TAXABLE_AMT, 0) AS TaxableValue,

        MAX(ISNULL(IND.E_BASIC_CentralT, 0)) AS CentralTaxPercentage,
        SUM(ROUND(ISNULL(IND.IND_AMT, 0) * ISNULL(IND.E_BASIC_CentralT, 0) / 100, 2)) AS CentralTaxAmount,

        MAX(ISNULL(IND.E_EDU_CESS_State, 0)) AS StateUnionTerritoryTaxPercentage,
        SUM(ROUND(ISNULL(IND.IND_AMT, 0) * ISNULL(IND.E_EDU_CESS_State, 0) / 100, 2)) AS StateUnionTerritoryTaxAmount,

        MAX(ISNULL(IND.E_H_EDU_Integrated, 0)) AS IntegratedTaxPercentage,
        SUM(ROUND(ISNULL(IND.IND_AMT, 0) * ISNULL(IND.E_H_EDU_Integrated, 0) / 100, 2)) AS IntegratedTaxAmount,

        ISNULL(INM.INM_G_AMT, 0) AS GrandTotal,
        '' AS AmountInWords  -- calculated in C#
    FROM INVOICE_MASTER INM
    INNER JOIN INVOICE_DETAIL IND ON IND.IND_INM_CODE = INM.INM_CODE
    WHERE INM.INM_CODE = @InvoiceCode
    AND ISNULL(INM.ES_DELETE, 0) = 0
    AND ISNULL(IND.ES_DELETE, 0) = 0
    GROUP BY
        INM.INM_DISC_AMT,
        INM.INM_PACK_AMT,
        INM.INM_FREIGHT,
        INM.INM_INSURANCE,
        INM.INM_OTHER_AMT,
        INM.INM_TAXABLE_AMT,
        INM.INM_G_AMT;

    -- ============================================
    -- Result Set 7: E-Invoice Information
    -- ============================================
    SELECT
        ISNULL(IRN, '') AS Irn,
        ISNULL(AckNo, '') AS AckNo,
        AckDate AS AckDate,
        NULL AS QrCodeImage
    FROM INVOICE_MASTER
    WHERE INM_CODE = @InvoiceCode
    AND ISNULL(ES_DELETE, 0) = 0;

    -- ============================================
    -- Result Set 8: Terms and Conditions
    -- ============================================
    SELECT
        '1) Goods Once Sold will not be taken back.' AS TermCondition
    UNION ALL
    SELECT '2)Unless informed at time of receipt, no shortages claim will be accepted.'
    UNION ALL
    SELECT '3) Interest Rate @24% P.A. applicable for Overdue Payment.';

END
GO
