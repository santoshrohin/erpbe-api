-- =============================================
-- Stored Procedure: ERP_GetCustomerPoPrintData
-- Purpose: Fetch all data required for Customer PO (Sales Order) PDF printing
-- Returns: Multiple result sets for different sections of the PO
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetCustomerPoPrintData]
    @PoCode INT,
    @CompanyId INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result Set 1: Company Information
    SELECT
        CM.CM_NAME AS CompanyName,
        ISNULL(CM.CM_ADDRESS1, '') AS FullAddress,
        CM.CM_GST_NO AS GstinNo
    FROM COMPANY_MASTER CM
    WHERE CM.CM_ID = @CompanyId;
    
    -- Result Set 2: PO Header
    SELECT 
        ISNULL(CPM.CPOM_DOC_NO, CPM.CPOM_CODE) AS SaleOrderNo,
        CPM.CPOM_DATE AS SaleOrderDate,
        CPM.CPOM_PONO AS PoNo,
        CPM.CPOM_PO_DATE AS PoDate,
        CPM.CPOM_FINAL_DEST AS Consignee,
        CPM.CPOM_PRE_CARR_BY AS TransportThrough,
        CPM.CPOM_PLACE_DEL AS DeliveryTerms,
        CPM.CPOM_PAY_TERM AS Narrations
    FROM CUSTPO_MASTER CPM
    WHERE CPM.CPOM_CODE = @PoCode
        AND CPM.CPOM_CM_COMP_ID = @CompanyId;
    
    -- Result Set 3: Customer Information
    SELECT 
        P.P_NAME AS Name,
        CONCAT(
            ISNULL(P.P_ADD1, ''), 
            CASE WHEN P.P_CITY IS NOT NULL AND P.P_CITY != '' THEN ', ' + P.P_CITY ELSE '' END
        ) AS Address
    FROM CUSTPO_MASTER CPM
    INNER JOIN PARTY_MASTER P ON P.P_CODE = CPM.CPOM_P_CODE
    WHERE CPM.CPOM_CODE = @PoCode
        AND CPM.CPOM_CM_COMP_ID = @CompanyId;
    
    -- Result Set 4: PO Line Items (Ordered by order quantity descending)
    SELECT 
        ROW_NUMBER() OVER (ORDER BY d.CPOD_ORD_QTY DESC) AS SrNo,
        CONCAT(i.I_NAME, ' (', i.I_CODENO, ')') AS ItemName,
        d.CPOD_ORD_QTY AS Qty,
        u.I_UOM_NAME AS Unit,
        d.CPOD_RATE AS Rate,
        d.CPOD_AMT AS Amount
    FROM CUSTPO_DETAIL d
    INNER JOIN ITEM_MASTER i ON i.I_CODE = d.CPOD_I_CODE
    INNER JOIN ITEM_UNIT_MASTER u ON u.I_UOM_CODE = d.CPOD_UOM_CODE
    WHERE d.CPOD_CPOM_CODE = @PoCode
    ORDER BY d.CPOD_ORD_QTY DESC;
    
    -- Result Set 5: Totals
    -- Use CPOM_T_PER if available, otherwise default to 18% GST (9% CGST + 9% SGST)
    SELECT 
        ISNULL(SUM(d.CPOD_ORD_QTY), 0) AS TotalQty,
        ISNULL(SUM(d.CPOD_AMT), 0) AS AssessableValue,
        ISNULL((SUM(d.CPOD_AMT) * COALESCE(m.CPOM_T_PER, 18.0) / 100) / 2, 0) AS CentralTax,
        ISNULL((SUM(d.CPOD_AMT) * COALESCE(m.CPOM_T_PER, 18.0) / 100) / 2, 0) AS StateUnionTerritoryTax,
        ISNULL(SUM(d.CPOD_AMT) + (SUM(d.CPOD_AMT) * COALESCE(m.CPOM_T_PER, 18.0) / 100), 0) AS TotalAmount
    FROM CUSTPO_MASTER m
    LEFT JOIN CUSTPO_DETAIL d ON d.CPOD_CPOM_CODE = m.CPOM_CODE
    WHERE m.CPOM_CODE = @PoCode
        AND m.CPOM_CM_COMP_ID = @CompanyId
    GROUP BY m.CPOM_T_PER;
    
END
GO
