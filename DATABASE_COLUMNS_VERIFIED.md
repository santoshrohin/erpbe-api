# ✅ Verified Database Columns for Tax Invoice Print

**Date:** October 24, 2025  
**Status:** VERIFIED from actual database

---

## COMPANY_MASTER
| Column Name | Type | Purpose |
|-------------|------|---------|
| `CM_CODE` | PK | Company code |
| `CM_ID` | int | Company ID (used in app) |
| `CM_NAME` | nvarchar | Company name → "SUN ELECTRO DEVICES PVT LTD." |
| `CM_ADDRESS1` | nvarchar | Full address line 1 → "Plot No. A-44/1/2/19-20, Chakan MIDC..." |
| `CM_ADDRESS2` | nvarchar | Address line 2 |
| `CM_ADDRESS3` | nvarchar | Address line 3 |
| `CM_CITY` | nvarchar | City |
| `CM_STATE` | nvarchar | State name |
| `CM_GST_NO` | nvarchar | GSTIN → "27AANCS2439P1ZL" ✅ |
| `CM_PAN_NO` | nvarchar | PAN Number |
| `CM_CIN_NO` | nvarchar | CIN Number |

**Note:** Company address is already complete in `CM_ADDRESS1` for company ID 1

---

## PARTY_MASTER (Customer/Vendor)
| Column Name | Type | Purpose |
|-------------|------|---------|
| `P_CODE` | PK | Party code |
| `P_NAME` | nvarchar | Party name → "LUCAS-TVS Limited" |
| `P_ADD1` | nvarchar | Address line 1 |
| `P_CITY` | nvarchar | City → "Pune" |
| `P_DISTRICT` | nvarchar | District |
| `P_PIN_CODE` | nvarchar | Pin code → "410 501" |
| `P_STM_CODE` | FK | State Master Code (links to STATE_MASTER) ✅ |
| `P_GST_NO` | nvarchar | GSTIN → "27AAACL3763E1ZN" ✅ |
| `P_PHONE` | nvarchar | Phone number |
| `P_EMAIL` | nvarchar | Email |
| `P_CONTACT` | nvarchar | Contact person |

---

## STATE_MASTER
| Column Name | Type | Purpose |
|-------------|------|---------|
| `SM_CODE` | PK | State master code |
| `SM_NAME` | nvarchar | State name → "Maharashtra" ✅ |
| `SM_STATE_CODE` | nvarchar | GST State Code → "27" ✅ |
| `SM_CM_COMP_ID` | FK | Company ID |

**Example Data:**
- SM_CODE: -2147483647
- SM_NAME: "Maharashtra"
- SM_STATE_CODE: "27"

---

## INVOICE_MASTER
| Column Name | Type | Purpose |
|-------------|------|---------|
| `INM_CODE` | PK | Invoice code |
| `INM_NO` | int | Invoice number |
| `INM_TNO` | nvarchar | **Invoice Serial No** → "SUN252605801" ✅ |
| `INM_DATE` | datetime | Invoice date + time |
| `INM_P_CODE` | FK | Party/Customer code |
| `INM_CPOM_CODE` | FK | Customer PO code |
| `INM_TRANSPORT` | nvarchar | Transport mode |
| `INM_VEH_NO` | nvarchar | Vehicle number |
| `INM_LR_NO` | nvarchar | LR number |
| `INM_LR_DATE` | datetime | LR date |
| `INM_NET_AMT` | decimal | Net amount |
| `INM_DISC_AMT` | decimal | **Discount amount** ✅ |
| `INM_PACK_AMT` | decimal | **Packing amount** ✅ |
| `INM_FREIGHT` | decimal | **Freight amount** ✅ |
| `INM_INSURANCE` | decimal | **Insurance amount** ✅ |
| `INM_OTHER_AMT` | decimal | **Other charges** ✅ |
| `INM_TAXABLE_AMT` | decimal | Taxable amount |
| `INM_G_AMT` | decimal | Grand total |
| `IRN` | nvarchar | E-Invoice IRN ✅ |
| `AckNo` | nvarchar | E-Invoice Ack No ✅ |
| `AckDate` | datetime | E-Invoice Ack Date ✅ |
| `EwayBill` | nvarchar | E-Way Bill number |

---

## INVOICE_DETAIL
| Column Name | Type | Purpose |
|-------------|------|---------|
| `IND_INM_CODE` | FK | Invoice master code |
| `IND_I_CODE` | FK | Item code |
| `IND_SR_NO` | int | **Serial number (for ordering)** ✅ |
| `IND_UOM_CODE` | FK | UOM code |
| `IND_INQTY` | decimal | Quantity → "300.00" ✅ |
| `IND_RATE` | decimal | Rate per unit → "123.46" ✅ |
| `IND_AMT` | decimal | **Taxable amount** → "37,038.00" ✅ |
| `IND_HSN_CODE` | nvarchar | HSN code → "85443000" ✅ |
| `E_BASIC_CentralT` | decimal | **CGST %** → "9.00" ✅ |
| `E_EDU_CESS_State` | decimal | **SGST %** → "9.00" ✅ |
| `E_H_EDU_Integrated` | decimal | **IGST %** → "0.00" ✅ |
| `IND_NO_PACK` | int | Number of packages |
| `IND_PACK_DESC` | nvarchar | Packing description |
| `IND_REMARK` | nvarchar | Item remark/description |

---

## ITEM_MASTER
| Column Name | Type | Purpose |
|-------------|------|---------|
| `I_CODE` | PK | Item code → "26728738" |
| `I_NAME` | nvarchar | Item name → "BRUSH PLATE ASSEMBLY" |
| `I_UOM_CODE` | FK | UOM code |

---

## ITEM_UNIT_MASTER
| Column Name | Type | Purpose |
|-------------|------|---------|
| `I_UOM_CODE` | PK | UOM code |
| `I_UOM_NAME` | nvarchar | UOM name → "NOS" |

---

## CUSTPO_MASTER
| Column Name | Type | Purpose |
|-------------|------|---------|
| `CPOM_CODE` | PK | PO code |
| `CPOM_PONO` | nvarchar | PO number → "1D03AE0023" ✅ |
| `CPOM_PO_DATE` | datetime | PO date |

---

## Key Findings

### ✅ All Required Columns Exist:
1. **Invoice Serial No:** `INM_TNO` ✅
2. **State Code:** Via `P_STM_CODE` → `STATE_MASTER.SM_STATE_CODE` ✅
3. **State Name:** Via `P_STM_CODE` → `STATE_MASTER.SM_NAME` ✅
4. **GSTIN:** `CM_GST_NO` and `P_GST_NO` ✅
5. **Discount, Packing, Freight, Insurance, Other:** All exist ✅
6. **E-Invoice:** `IRN`, `AckNo`, `AckDate` ✅
7. **Tax %:** `E_BASIC_CentralT`, `E_EDU_CESS_State`, `E_H_EDU_Integrated` ✅

### 📝 Important Notes:
1. Company address is already formatted in `CM_ADDRESS1` for company 1
2. State code requires JOIN with `STATE_MASTER` using `P_STM_CODE`
3. Serial number for line items ordering is `IND_SR_NO`
4. Item description needs to combine `I_CODE` + " - " + `I_NAME`

---

**Status:** ✅ Ready to create stored procedure

