# PO Number and GSTIN Fix - Complete ✅

## Issue Summary
The generated PDF was missing:
1. **PO Number** (should show: 4500177572)
2. **Recipient State Name** (should show: Maharashtra)
3. **Recipient State Code** (should show: 27)
4. **Recipient GSTIN** (should show: 27AAKCM9673Q1ZA)

## Root Cause Analysis

### 1. PO Number Issue
**Problem**: The stored procedure was looking for PO in `INVOICE_MASTER.INM_CPOM_CODE`, which was NULL.

**Discovery**: 
- PO is actually linked at the **LINE ITEM level**, not the invoice header level
- Found in: `INVOICE_DETAIL.IND_CPOM_CODE` → `CUSTPO_MASTER.CPOM_CODE` → `CUSTPO_MASTER.CPOM_PONO`

**Database Query Results**:
```sql
-- Invoice has NO PO at header level
SELECT INM_CPOM_CODE FROM INVOICE_MASTER WHERE INM_CODE = -2147419120
-- Result: NULL

-- But line items DO have PO linked
SELECT DISTINCT IND_CPOM_CODE FROM INVOICE_DETAIL WHERE IND_INM_CODE = -2147419120
-- Result: -2147482100

-- Which maps to the correct PO
SELECT CPOM_PONO FROM CUSTPO_MASTER WHERE CPOM_CODE = -2147482100
-- Result: 4500177572 ✅
```

### 2. Recipient GSTIN and State Issue
**Problem**: The stored procedure was looking for GSTIN in `PARTY_MASTER.P_GST_NO`, which was NULL.

**Discovery**: 
- Recipient GSTIN is stored directly in `INVOICE_MASTER.ReciptGSTIn`
- This is NOT the same as the party's default GSTIN
- State code can be derived from the first 2 digits of the GSTIN (e.g., "27" from "27AAKCM9673Q1ZA")

**Database Query Results**:
```sql
-- Party record has NO GSTIN
SELECT P_GST_NO FROM PARTY_MASTER WHERE P_CODE = -2147483101
-- Result: NULL

-- But invoice has the GSTIN directly
SELECT ReciptGSTIn FROM INVOICE_MASTER WHERE INM_CODE = -2147419120
-- Result: 27AAKCM9673Q1ZA ✅

-- State can be derived from first 2 digits
SELECT SM_NAME, SM_STATE_CODE 
FROM STATE_MASTER 
WHERE SM_CODE = 27  -- First 2 digits of GSTIN
-- Result: Maharashtra, 27 ✅
```

## Changes Made

### File: `Database_Scripts/StoredProcedures/TaxInvoice/ERP_GetTaxInvoicePrintData_FINAL.sql`

#### Change 1: PO Number (Result Set 2 - Invoice Header)
```sql
-- OLD (WRONG):
ISNULL(CPO.CPOM_PONO, '') AS PoNo,
FROM INVOICE_MASTER INM
LEFT JOIN CUSTPO_MASTER CPO ON CPO.CPOM_CODE = INM.INM_CPOM_CODE  -- This was NULL!

-- NEW (CORRECT):
-- Get PO from first line item (PO is linked at line item level, not invoice header)
ISNULL((SELECT TOP 1 CPO.CPOM_PONO 
        FROM INVOICE_DETAIL IND
        INNER JOIN CUSTPO_MASTER CPO ON CPO.CPOM_CODE = IND.IND_CPOM_CODE
        WHERE IND.IND_INM_CODE = INM.INM_CODE 
        AND ISNULL(IND.ES_DELETE, 0) = 0
        AND ISNULL(CPO.ES_DELETE, 0) = 0), '') AS PoNo,
```

#### Change 2: Recipient GSTIN and State (Result Set 3 - Recipient Details)
```sql
-- OLD (WRONG):
SELECT 
    ISNULL(PM.P_NAME, '') AS Name,
    ISNULL(SM.SM_NAME, '') AS StateName,
    ISNULL(SM.SM_STATE_CODE, '') AS StateCode,
    ISNULL(PM.P_GST_NO, '') AS GstinNo  -- This was NULL!
FROM PARTY_MASTER PM
LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = PM.P_STM_CODE  -- This was also NULL!
WHERE PM.P_CODE = @CustomerCode

-- NEW (CORRECT):
SELECT 
    ISNULL(PM.P_NAME, '') AS Name,
    -- State derived from ReciptGSTIn (first 2 digits)
    ISNULL(SM.SM_NAME, '') AS StateName,
    ISNULL(SM.SM_STATE_CODE, '') AS StateCode,
    ISNULL(INM.ReciptGSTIn, '') AS GstinNo  -- From INVOICE_MASTER!
FROM INVOICE_MASTER INM
INNER JOIN PARTY_MASTER PM ON PM.P_CODE = INM.INM_P_CODE AND ISNULL(PM.ES_DELETE, 0) = 0
LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = TRY_CAST(LEFT(INM.ReciptGSTIn, 2) AS INT)  -- Derive from GSTIN!
WHERE INM.INM_CODE = @InvoiceCode
```

#### Change 3: Delivery GSTIN and State (Result Set 4 - Delivery Details)
```sql
-- Same changes as Recipient section (Result Set 3)
-- Now uses INM.ReciptGSTIn instead of PM.P_GST_NO
-- State derived from first 2 digits of GSTIN
```

## Key Learnings

1. **PO Linking**: In this ERP system, POs are linked at the line item level, not the invoice header level. This allows different line items to reference different POs.

2. **GSTIN Storage**: The recipient's GSTIN is stored directly in the invoice (`INVOICE_MASTER.ReciptGSTIn`), not in the party master. This allows invoices to use different GSTINs for the same party (e.g., different branches).

3. **State Derivation**: GST numbers in India start with a 2-digit state code. We can derive the state by extracting the first 2 digits and joining with `STATE_MASTER`.

4. **Legacy System Quirks**: The legacy Crystal Reports implementation had access to these fields, but the mapping wasn't obvious from the table structure alone. Required extensive database investigation.

## Testing Instructions

1. **Start the API**:
   ```powershell
   cd D:\Santosh\Work\Projects\WebBased\API\API\ErpBE.API
   dotnet run
   ```

2. **Generate Test PDF**:
   ```powershell
   [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
   $response = Invoke-WebRequest -Uri "https://localhost:7095/api/Sales/TaxInvoice/print?invoiceCode=-2147419120&companyId=1" -Method GET
   [System.IO.File]::WriteAllBytes("D:\Invoice\test_invoice_fixed.pdf", $response.Content)
   ```

3. **Verify the PDF shows**:
   - ✅ PO No: **4500177572**
   - ✅ State Name: **Maharashtra**
   - ✅ State Code: **27**
   - ✅ GSTIN No: **27AAKCM9673Q1ZA**

## Status
✅ **Stored procedure updated and deployed**
⏳ **Awaiting user testing and confirmation**

## Next Steps
1. User to start the API and test the PDF generation
2. Verify all fields match the legacy invoice
3. If confirmed working, proceed with any remaining layout adjustments

