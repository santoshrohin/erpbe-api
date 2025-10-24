# Tax Invoice - COMPLETE Implementation Analysis

## 🎯 User Decisions

1. **Approach**: ✅ **Full Implementation** (All 157 fields)
2. **Customer PO**: ✅ **MANDATORY** - Invoice must be linked to Customer PO
3. **E-Invoice**: ⏰ **Later** - Will implement GST E-Invoice integration after CRUD
4. **Export Invoices**: ⏰ **Later** - Will implement after domestic CRUD is complete
5. **Tax Types**: ✅ **Exactly as legacy** - All tax types (GST, Excise, TCS, etc.)
6. **Invoice Numbering**: ✅ **Exactly as legacy** - Company-wise auto-generation

---

## 📋 CRITICAL VALIDATIONS (from Legacy Application)

### **A. Form-Level Validations (btnSubmit_Click - Lines 123-174)**

```csharp
1. ✅ Customer Selection (MANDATORY)
   - ddlCustomer.SelectedIndex != 0
   - Error: "Select Customer Name"

2. ✅ Item Details Grid (MANDATORY)
   - dgInvoiceAddDetail.Enabled must be true
   - Error: "Please insert Item details."

3. ✅ Tax Name (MANDATORY)
   - ddlTaxName.SelectedIndex != 0
   - Error: "Select Tax Name"

4. ✅ Grid Must Have Records
   - dgInvoiceAddDetail.Rows.Count > 0
   - Error: "Record Not Found In Table"
```

### **B. Customer Selection Validations (Lines 684-751)**

```csharp
When Customer is selected:
1. ✅ Load Customer Address from PARTY_MASTER
2. ✅ Load State from STATE_MASTER (via P_SM_CODE)
3. ✅ Load GST No (P_LBT_NO) if P_LBT_IND = true
   - If GST applicable: Show GST No (white background)
   - If NOT applicable: Clear GST No (red background #FFDFDF)
4. ✅ Load Item Codes & Names (only items from Customer's POs)
5. ✅ Clear All Grid and Amount Fields
```

### **C. Item Selection Validations (Lines 571-625, 754-822)**

```csharp
When Item Code/Name is selected:
1. ✅ Load UOM from ITEM_UNIT_MASTER
2. ✅ Load Unit Weight (I_UWEIGHT)
3. ✅ Calculate Stock Quantity from STOCK_LEDGER
   - SUM(STL_DOC_QTY) WHERE STL_STORE_TYPE = -2147483648
4. ✅ Load Customer POs for this specific Item
   - Only POs where (CPOD_ORD_QTY - CPOD_DISPACH) > 0
   - OR CPOD_ORD_QTY = 0
   - AND CPOD_STATUS = 0 (Active POs only)
5. ✅ Load HSN Code from EXCISE_TARIFF_MASTER (via I_E_CODE)
```

### **D. PO Selection Validations (Lines 628-682)**

```csharp
When PO is selected:
1. ✅ Load Rate from CUSTPO_DETAIL (CPOD_RATE)
2. ✅ Load Amortization Rate (CPOD_AMORTRATE)
3. ✅ Calculate Pending Quantity
   - Pending Qty = CPOD_ORD_QTY - CPOD_DISPACH
4. ✅ Apply Discount from PO
   - Get CPOD_DISC_AMT and CPOD_DISC_PER
   - Rate = CPOD_RATE - (CPOD_DISC_AMT / CPOD_ORD_QTY)
5. ✅ Show PO Date (CPOM_DATE)
```

### **E. Item Insert Validations (btnInsert_Click - Will need to search)**

```csharp
1. ✅ Customer must be selected
2. ✅ Item Code must be selected (not 0)
3. ✅ PO must be selected (not 0)
4. ✅ Invoice Quantity (txtVQty) must be > 0
5. ✅ Invoice Qty cannot exceed Pending Qty
   - txtVQty <= txtPendingQty
6. ✅ Rate must be > 0
7. ✅ Tray Qty validation (if applicable)
   - txtTrayQty <= txtTrayStock
   - Error: "Please Enter Correct Tray Qty"
```

### **F. Quantity Validation (txtVQty_OnTextChanged)**

```csharp
1. ✅ Quantity format: 0.000 (3 decimal places)
2. ✅ Quantity must be numeric
3. ✅ Quantity validation against:
   - Stock Quantity (warning if exceeds)
   - Pending Order Quantity (error if exceeds)
4. ✅ Auto-calculate Amount = Qty × Rate
5. ✅ Auto-calculate Amortization Amount = Qty × AmortRate
```

---

## 💰 CALCULATION LOGIC (GetTotal Method - Lines 1656-1894)

### **1. Net Amount**
```
Net Amount = SUM(All Line Items Amount)
```

### **2. Amortization Amount**
```
Amort Amount = SUM(All Line Items Amort Amount)
(Excluded if Supplementary Invoice)
```

### **3. Discount**
```
Discount Amount = (Net Amount × Discount %) / 100
```

### **4. Accessible Amount**
```
Accessible Amount = Net Amount + Amort Amount - Discount Amount + Packing Amount
```

### **5. Taxable Amount**
```
Taxable Amount = Accessible Amount - Amort Amount 
                 + Other Charges + Freight + Insurance 
                 + Transport + Octri + TCS
```

### **6. GST Calculation (Based on State)**

#### **Intra-State (Company State == Customer State)**
```
CGST Amount = (Taxable Amount × CGST %) / 100
SGST Amount = (Taxable Amount × SGST %) / 100
IGST Amount = 0
```

#### **Inter-State (Company State != Customer State)**
```
CGST Amount = 0
SGST Amount = 0
IGST Amount = (Taxable Amount × IGST %) / 100
```

### **7. Sales Tax**
```
Sales Tax Amount = (Taxable Amount × Sales Tax %) / 100
(Currently set to 0 in code - line 1875-1877)
```

### **8. Grand Total**
```
Grand Total = Taxable Amount 
            + Sales Tax Amount 
            + CGST Amount 
            + SGST Amount 
            + IGST Amount 
            - Amort Amount
            + Rounding Amount
```

---

## 🔐 BUSINESS RULES

### **1. Customer PO Linkage (MANDATORY)**
```sql
-- Load Customers: Only those who have active POs
SELECT DISTINCT P_CODE, P_NAME 
FROM PARTY_MASTER, CUSTPO_MASTER 
WHERE CPOM_P_CODE = P_CODE 
  AND CUSTPO_MASTER.ES_DELETE = 0 
  AND P_CM_COMP_ID = @CompanyId 
  AND P_TYPE = '1' (Customer)
  AND P_ACTIVE_IND = 1
```

### **2. Item Availability**
```sql
-- Load Items: Only items from selected Customer's POs
SELECT DISTINCT I_CODE, I_CODENO 
FROM ITEM_MASTER, CUSTPO_DETAIL, CUSTPO_MASTER 
WHERE CUSTPO_MASTER.ES_DELETE = 0 
  AND ((CPOD_ORD_QTY - CPOD_DISPACH) > 0 OR CPOM_CODE = CPOD_CPOM_CODE)
  AND CPOD_I_CODE = I_CODE 
  AND CPOM_CODE = CPOD_CPOM_CODE 
  AND CPOM_P_CODE = @CustomerCode
```

### **3. Stock Management**
```sql
-- On Invoice Save (INSERT):
-- 1. Reduce Item Stock
UPDATE ITEM_MASTER 
SET I_CURRENT_BAL = I_CURRENT_BAL - @InvoiceQty 
WHERE I_CODE = @ItemCode

-- 2. Update Customer PO Dispatch
UPDATE CUSTPO_DETAIL 
SET CPOD_DISPACH = CPOD_DISPACH + @InvoiceQty 
WHERE CPOD_CPOM_CODE = @POCode 
  AND CPOD_I_CODE = @ItemCode

-- 3. Create Stock Ledger Entry
INSERT INTO STOCK_LEDGER 
(STL_DOC_NO, STL_DOC_TYPE, STL_I_CODE, STL_DOC_QTY, ...)
VALUES (@InvoiceCode, 'TAXINV', @ItemCode, -@InvoiceQty, ...)
```

### **4. Modification Lock**
```sql
-- When opening for Edit/View:
-- Set MODIFY flag to prevent concurrent editing
UPDATE INVOICE_MASTER 
SET MODIFY = 1 
WHERE INM_CODE = @InvoiceCode

-- On Cancel/Save:
-- Release the lock
UPDATE INVOICE_MASTER 
SET MODIFY = 0 
WHERE INM_CODE = @InvoiceCode
```

### **5. Invoice Number Generation**
```sql
-- Auto-generate Invoice Number (Company-wise)
-- Need to check exact logic from SaveRec method
SELECT ISNULL(MAX(INM_NO), 0) + 1 AS NextInvoiceNo
FROM INVOICE_MASTER 
WHERE INM_CM_CODE = @CompanyCode 
  AND INM_TYPE = 'TAXINV'
```

### **6. Tax Selection from PO**
```sql
-- Auto-select Tax from first line item's PO
SELECT CPOD_ST_CODE 
FROM CUSTPO_DETAIL, CUSTPO_MASTER 
WHERE CPOM_CODE = @POCode 
  AND CPOD_I_CODE = @ItemCode 
  AND CPOM_CODE = CPOD_CPOM_CODE
```

### **7. Supplementary Invoice**
- If `chkIsSuppliement.Checked = true`:
  - Amortization Amount is NOT included in totals
  - Stock is NOT updated
  - CUSTPO_DETAIL dispatch is NOT updated
  - INM_IS_SUPPLIMENT = 1

---

## 📊 MASTER/DETAIL DATA STRUCTURE

### **INVOICE_MASTER Fields (Priority Order)**

#### **Phase 1: Essential Fields (20)**
```
1. INM_CODE (PK, Identity) - Auto
2. INM_CM_CODE - Company Code (Session)
3. INM_NO - Invoice Number (Auto-generate)
4. INM_DATE - Invoice Date (User input)
5. INM_TYPE - 'TAXINV' (Fixed)
6. INM_INVOICE_TYPE - 0 for Tax Invoice
7. INM_P_CODE - Customer Code (MANDATORY)
8. INM_T_CODE - Tax Code (from Tax Master)
9. INM_NET_AMT - Net Amount (Calculated)
10. INM_DISC - Discount % (User input)
11. INM_DISC_AMT - Discount Amount (Calculated)
12. INM_PACK_AMT - Packing Amount (User input)
13. INM_TAXABLE_AMT - Taxable Amount (Calculated)
14. INM_T_AMT - Tax Amount (Calculated)
15. INM_G_AMT - Grand Total (Calculated)
16. INM_ROUNDING_AMT - Rounding (User input)
17. INM_REMARK - Remarks (User input)
18. ES_DELETE - 0 (default)
19. MODIFY - 0 (default, 1 when editing)
20. INM_STATE - State Code
```

#### **Phase 2: Tax & Charges (15)**
```
21. INM_S_TAX - Service Tax %
22. INM_S_TAX_AMT - Service Tax Amount
23. INM_BEXCISE - Basic Excise %  (CGST)
24. INM_BE_AMT - Basic Excise Amount (CGST Amt)
25. INM_EDUC_CESS - Education Cess % (SGST)
26. INM_EDUC_AMT - Education Cess Amount (SGST Amt)
27. INM_H_EDUC_CESS - Higher Edu Cess % (IGST)
28. INM_H_EDUC_AMT - Higher Edu Cess Amount (IGST Amt)
29. INM_FREIGHT - Freight Charges
30. INM_TAX_TCS - TCS %
31. INM_TAX_TCS_AMT - TCS Amount
32. INM_OTHER_AMT - Other Charges
33. INM_INSURANCE - Insurance Amount
34. INM_TRANS_AMT - Transport Amount
35. INM_OCTRI_AMT - Octri Amount
```

#### **Phase 3: Transport & Logistics (10)**
```
36. INM_VEH_NO - Vehicle Number
37. INM_TRANSPORT - Transport Name
38. INM_STO_LOC - Storage Location (Amort Amount)
39. INM_ISSUE_DATE - Issue Date
40. INM_REMOVAL_DATE - Removal Date
41. INM_ISSU_TIME - Issue Time
42. INM_REMOVEL_TIME - Removal Time
43. INM_LR_NO - LR Number
44. INM_LR_DATE - LR Date
45. INM_ACCESSIBLE_AMT - Accessible Amount
```

#### **Phase 4: Additional Details (10)**
```
46. INM_C_DAYS - Credit Days
47. INM_IS_SUPPLIMENT - Is Supplementary Invoice
48. INM_TRAY_CODE - Tray Item Code
49. INM_TRAY_QTY - Tray Quantity
50. INM_ADDRESS - Delivery Address
51. INM_HSN_CODE - HSN Code
52. INM_ELECTRREFNUM - Electronic Reference Number
53. INM_TERMSNCONDITIONS - Terms & Conditions
54. INM_AUTHORIZEDNAME - Authorized Signatory
55. INM_ADDRESS_SELECTED - Selected Address ID
```

#### **Phase 5: E-Invoice & E-Way Bill (7)** [LATER]
```
56. AckNo - E-Invoice Acknowledgement Number
57. AckDate - E-Invoice Acknowledgement Date
58. InvValue - Invoice Value
59. ReciptGSTIn - Recipient GSTIN
60. EInvStatus - E-Invoice Status
61. IRN - Invoice Reference Number
62. QRCode - QR Code Data
63. EwayBill - E-Way Bill Number
```

#### **Phase 6: Export Fields (94+)** [LATER]
```
64-157: All Export-related fields
(Will implement in Phase 6)
```

### **INVOICE_DETAIL Fields (25 essential)**

```
1. IND_INM_CODE - FK to INVOICE_MASTER
2. IND_I_CODE - Item Code
3. IND_UOM_CODE - UOM Code
4. IND_CPOM_CODE - Customer PO Code
5. IND_INQTY - Invoice Quantity (NOT NULL)
6. IND_RATE - Rate
7. IND_AMT - Amount
8. IND_CON_QTY - Conversion Quantity
9. IND_AMORT_RATE - Amortization Rate
10. IND_AMORTAMT - Amortization Amount
11. IND_HSN_CODE - HSN Code
12. IND_STORE_CODE - Store Code
13. E_BASIC_CentralT - CGST %
14. E_EDU_CESS_State - SGST %
15. E_H_EDU_Integrated - IGST %
16. IND_EX_AMT - CGST Amount
17. IND_E_CESS_AMT - SGST Amount
18. IND_SH_CESS_AMT - IGST Amount
19. IND_SUBHEADING - Sub Heading
20. IND_BACHNO - Batch Number
21. IND_NO_PACK - Number of Packages
22. IND_PAK_QTY - Packing Quantity
23. IND_PACK_DESC - Package Description
24. IND_SR_NO - Serial Number
25. IND_REMARK - Remarks
26. ES_DELETE - 0 (default)
```

---

## 🚨 CRITICAL DEPENDENCIES

### **Master Tables Required:**
1. ✅ PARTY_MASTER (Customer)
2. ✅ CUSTPO_MASTER (Customer PO Header)
3. ✅ CUSTPO_DETAIL (Customer PO Line Items)
4. ✅ ITEM_MASTER (Items/Products)
5. ✅ ITEM_UNIT_MASTER (UOM)
6. ✅ SALES_TAX_MASTER (Tax Configuration)
7. ✅ STATE_MASTER (States)
8. ✅ COMPANY_MASTER (Company details for tax calculation)
9. ✅ EXCISE_TARIFF_MASTER (HSN Code mapping)
10. ✅ STOCK_LEDGER (Stock movements)

---

## 📝 IMPLEMENTATION PHASES

### **Phase 1: Basic CRUD (Week 1)**
- Essential 20 fields in INVOICE_MASTER
- Essential 25 fields in INVOICE_DETAIL
- Basic validations
- Simple calculations (Net, Discount, Total)
- **NO GST, NO Stock Update**

### **Phase 2: Tax & Stock (Week 2)**
- Complete tax calculations (CGST/SGST/IGST)
- Stock management (ITEM_MASTER, CUSTPO_DETAIL, STOCK_LEDGER)
- All charges (Freight, Transport, etc.)
- Complete business rules
- Modification lock

### **Phase 3: Full Domestic Invoice (Week 3)**
- All remaining domestic fields
- Tray management
- Complete validations
- Print functionality
- Terms & Conditions

### **Phase 4: E-Invoice & E-Way Bill (Week 4+)**
- GST API integration
- IRN generation
- QR Code generation
- E-Way Bill

### **Phase 5: Export Invoice (Week 5+)**
- All 94+ export fields
- Export documentation
- Shipping details

---

## ⚠️ WARNINGS

1. **This is the MOST COMPLEX transaction in the entire ERP system**
2. **Estimated Time**: 3-4 weeks for full implementation
3. **157 fields** in master table - unprecedented scale
4. **Multiple interdependencies** with other modules
5. **Complex calculations** with state-based logic
6. **Stock management** with reversal logic
7. **Concurrency control** via MODIFY flag

---

## ✅ NEXT STEPS

**Please confirm you want to proceed with this massive implementation!**

Once confirmed, I will:
1. Create DTOs (47 properties for master, 26 for detail)
2. Create Commands & Queries (CQRS)
3. Create Validators (FluentValidation - extensive)
4. Create Handlers (Complex business logic)
5. Create Repository & Stored Procedures
6. Create API Controller
7. Create comprehensive test cases

**Ready to start?** This will be a **MAJOR undertaking**! 🚀

