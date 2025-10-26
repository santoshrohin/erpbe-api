-- =============================================
-- Tax Invoice Print Data - FINAL CORRECT VERSION
-- ALL column names verified from actual database
-- Matches EXACT invoice format from taxinvoice_page-0001.jpg
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoicePrintData_V2]
    @InvoiceCode INT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Declare variables
    DECLARE @CustomerCode INT;
    
    -- Get customer code
    SELECT @CustomerCode = INM_P_CODE
    FROM INVOICE_MASTER
    WHERE INM_CODE = @InvoiceCode
    AND ISNULL(ES_DELETE, 0) = 0;
    
    -- ============================================
    -- Result Set 1: Company Information (Header)
    -- Simple format: Company name, full address, GSTIN
    -- ============================================
    SELECT 
        ISNULL(CM_NAME, '') AS CompanyName,
        -- Address is already complete in CM_ADDRESS1 for company 1
        ISNULL(CM_ADDRESS1, '') AS FullAddress,
        ISNULL(CM_GST_NO, '') AS GstinNo
    FROM COMPANY_MASTER
    WHERE CM_ID = @CompanyId 
    AND ISNULL(CM_DELETE_FLAG, 0) = 0;
    
    -- ============================================
    -- Result Set 2: Invoice Header (Left + Right columns)
    -- Left: Date, Serial No, GSTIN, E-Way Bill
    -- Right: Transport, Vehicle, PO No, Date/Time of Supply, Place
    -- ============================================
    SELECT 
        -- Left Column
        INM.INM_DATE AS DateOfInvoice,
        ISNULL(INM.INM_TNO, '') AS InvoiceSerialNo,  -- e.g., "SUN252605801"
        ISNULL(CM.CM_GST_NO, '') AS GstinNo,
        ISNULL(INM.EwayBill, '') AS EWayBillNo,
        
        -- Right Column
        ISNULL(INM.INM_TRANSPORT, '') AS TransporatationMode,  -- Note: keeping typo from actual invoice
        ISNULL(INM.INM_VEH_NO, '') AS VehicleNo,
        -- Get PO from first line item (PO is linked at line item level, not invoice header)
        ISNULL((SELECT TOP 1 CPO.CPOM_PONO 
                FROM INVOICE_DETAIL IND
                INNER JOIN CUSTPO_MASTER CPO ON CPO.CPOM_CODE = IND.IND_CPOM_CODE
                WHERE IND.IND_INM_CODE = INM.INM_CODE 
                AND ISNULL(IND.ES_DELETE, 0) = 0
                AND ISNULL(CPO.ES_DELETE, 0) = 0), '') AS PoNo,
        INM.INM_DATE AS DateAndTimeOfSupply,  -- Date with time
        ISNULL(CM_STATE.SM_NAME, 'Maharashtra') AS PlaceOfSupply
    FROM INVOICE_MASTER INM
    INNER JOIN COMPANY_MASTER CM ON CM.CM_CODE = INM.INM_CM_CODE AND CM.CM_ID = @CompanyId
    LEFT JOIN STATE_MASTER CM_STATE ON CM_STATE.SM_CODE = CM.CM_STATE AND ISNULL(CM_STATE.ES_DELETE, 0) = 0
    WHERE INM.INM_CODE = @InvoiceCode 
    AND ISNULL(INM.ES_DELETE, 0) = 0
    AND ISNULL(CM.CM_DELETE_FLAG, 0) = 0;
    
    -- ============================================
    -- Result Set 3: Recipient Details (Left side)
    -- "Details Of Recipient"
    -- Uses ReciptGSTIn from INVOICE_MASTER (not from PARTY_MASTER)
    -- ============================================
    SELECT 
        ISNULL(PM.P_NAME, '') AS Name,
        -- Build address: Area,City,Dist-City-PinCode
        ISNULL(PM.P_ADD1, '') + 
        CASE WHEN ISNULL(PM.P_CITY, '') <> '' THEN ',Dist-' + PM.P_CITY ELSE '' END +
        CASE WHEN ISNULL(PM.P_PIN_CODE, '') <> '' THEN '-' + PM.P_PIN_CODE ELSE '' END AS Address,
        -- State derived from ReciptGSTIn (first 2 digits)
        ISNULL(SM.SM_NAME, '') AS StateName,
        ISNULL(SM.SM_STATE_CODE, '') AS StateCode,  -- e.g., "27"
        ISNULL(INM.ReciptGSTIn, '') AS GstinNo  -- From INVOICE_MASTER, not PARTY_MASTER
    FROM INVOICE_MASTER INM
    INNER JOIN PARTY_MASTER PM ON PM.P_CODE = INM.INM_P_CODE AND ISNULL(PM.ES_DELETE, 0) = 0
    LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = TRY_CAST(LEFT(INM.ReciptGSTIn, 2) AS INT) AND ISNULL(SM.ES_DELETE, 0) = 0
    WHERE INM.INM_CODE = @InvoiceCode
    AND ISNULL(INM.ES_DELETE, 0) = 0;
    
    -- ============================================
    -- Result Set 4: Delivery Details (Right side)
    -- "Details Of Delivery" - Usually same as recipient
    -- Uses ReciptGSTIn from INVOICE_MASTER (not from PARTY_MASTER)
    -- ============================================
    SELECT 
        ISNULL(PM.P_NAME, '') AS Name,
        ISNULL(PM.P_ADD1, '') + 
        CASE WHEN ISNULL(PM.P_CITY, '') <> '' THEN ',Dist-' + PM.P_CITY ELSE '' END +
        CASE WHEN ISNULL(PM.P_PIN_CODE, '') <> '' THEN '-' + PM.P_PIN_CODE ELSE '' END AS Address,
        -- State derived from ReciptGSTIn (first 2 digits)
        ISNULL(SM.SM_NAME, '') AS StateName,
        ISNULL(SM.SM_STATE_CODE, '') AS StateCode,
        ISNULL(INM.ReciptGSTIn, '') AS GstinNo  -- From INVOICE_MASTER, not PARTY_MASTER
    FROM INVOICE_MASTER INM
    INNER JOIN PARTY_MASTER PM ON PM.P_CODE = INM.INM_P_CODE AND ISNULL(PM.ES_DELETE, 0) = 0
    LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = TRY_CAST(LEFT(INM.ReciptGSTIn, 2) AS INT) AND ISNULL(SM.ES_DELETE, 0) = 0
    WHERE INM.INM_CODE = @InvoiceCode
    AND ISNULL(INM.ES_DELETE, 0) = 0;
    
    -- ============================================
    -- Result Set 5: Line Items
    -- Columns: Sr. No | Description Of Goods Or Services | HSN/SAC | UOM | Qty | Rate/Unit | Taxable Value
    -- ============================================
    SELECT 
        ROW_NUMBER() OVER (ORDER BY ISNULL(IND.IND_SR_NO, 999999), IND.IND_I_CODE) AS SrNo,
        -- Description format: "26728738 - BRUSH PLATE ASSEMBLY" (using I_CODENO, not I_CODE which is PK)
        ISNULL(IM.I_CODENO, '') + ' - ' + ISNULL(IM.I_NAME, '') AS DescriptionOfGoodsOrServices,
        ISNULL(IND.IND_HSN_CODE, '') AS HsnSac,
        ISNULL(UM.I_UOM_NAME, 'NOS') AS Uom,
        ISNULL(IND.IND_INQTY, 0) AS Qty,
        ISNULL(IND.IND_RATE, 0) AS RatePerUnit,
        ISNULL(IND.IND_AMT, 0) AS TaxableValue,
        -- Tax percentages (for tax calculation)
        ISNULL(IND.E_BASIC_CentralT, 0) AS CgstPercentage,
        ISNULL(IND.E_EDU_CESS_State, 0) AS SgstPercentage,
        ISNULL(IND.E_H_EDU_Integrated, 0) AS IgstPercentage
    FROM INVOICE_DETAIL IND
    INNER JOIN ITEM_MASTER IM ON IM.I_CODE = IND.IND_I_CODE
    LEFT JOIN ITEM_UNIT_MASTER UM ON UM.I_UOM_CODE = IND.IND_UOM_CODE
    WHERE IND.IND_INM_CODE = @InvoiceCode
    AND ISNULL(IND.ES_DELETE, 0) = 0
    ORDER BY ISNULL(IND.IND_SR_NO, 999999), IND.IND_I_CODE;
    
    -- ============================================
    -- Result Set 6: Totals and Tax Summary
    -- Format: Less/Add sections, then Taxable Value, then Taxes, then Grand Total
    -- ============================================
    SELECT TOP 1
        -- Less/Add sections
        ISNULL(INM.INM_DISC_AMT, 0) AS Discount,
        ISNULL(INM.INM_PACK_AMT, 0) AS PackingAndForwardingCharges,
        ISNULL(INM.INM_FREIGHT, 0) + ISNULL(INM.INM_INSURANCE, 0) AS FrieghtAndInsurance,  -- Note: keeping typo "Frieght" from actual invoice
        ISNULL(INM.INM_OTHER_AMT, 0) AS OtherCharges,
        
        -- Taxable Value (after Less/Add)
        ISNULL(INM.INM_TAXABLE_AMT, 0) AS TaxableValue,
        
        -- Tax percentages and amounts
        -- IMPORTANT: Labels are "Central Tax" and "State/Union Territory Tax", NOT CGST/SGST
        MAX(ISNULL(IND.E_BASIC_CentralT, 0)) AS CentralTaxPercentage,
        SUM(ROUND(ISNULL(IND.IND_AMT, 0) * ISNULL(IND.E_BASIC_CentralT, 0) / 100, 2)) AS CentralTaxAmount,
        
        MAX(ISNULL(IND.E_EDU_CESS_State, 0)) AS StateUnionTerritoryTaxPercentage,
        SUM(ROUND(ISNULL(IND.IND_AMT, 0) * ISNULL(IND.E_EDU_CESS_State, 0) / 100, 2)) AS StateUnionTerritoryTaxAmount,
        
        MAX(ISNULL(IND.E_H_EDU_Integrated, 0)) AS IntegratedTaxPercentage,
        SUM(ROUND(ISNULL(IND.IND_AMT, 0) * ISNULL(IND.E_H_EDU_Integrated, 0) / 100, 2)) AS IntegratedTaxAmount,
        
        -- Grand Total
        ISNULL(INM.INM_G_AMT, 0) AS GrandTotal,
        '' AS AmountInWords  -- Will be calculated in C# using NumberToWordsConverter
    FROM INVOICE_MASTER INM
    INNER JOIN INVOICE_DETAIL IND ON IND.IND_INM_CODE = INM.INM_CODE AND ISNULL(IND.ES_DELETE, 0) = 0
    WHERE INM.INM_CODE = @InvoiceCode
    AND ISNULL(INM.ES_DELETE, 0) = 0
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
    -- Bottom left section with QR code
    -- ============================================
    SELECT 
        ISNULL(IRN, '') AS Irn,
        ISNULL(AckNo, '') AS AckNo,
        AckDate AS AckDate,
        NULL AS QrCodeImage  -- QR code will be generated from IRN in C# using QRCoder
    FROM INVOICE_MASTER
    WHERE INM_CODE = @InvoiceCode
    AND ISNULL(ES_DELETE, 0) = 0;
    
    -- ============================================
    -- Result Set 8: Terms and Conditions
    -- Fixed list for now (can be made configurable later)
    -- ============================================
    SELECT '1) Goods Once Sold will not be taken back.' AS TermCondition
    UNION ALL
    SELECT '2)Unless informed at time of receipt, no shortages claim will be accepted.'
    UNION ALL
    SELECT '3) Interest Rate @24% P.A. applicable for Overdue Payment.';
    
END
GO

-- Grant execute permission
GRANT EXECUTE ON [dbo].[ERP_GetTaxInvoicePrintData_V2] TO PUBLIC;
GO

