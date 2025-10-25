# 📄 ACTUAL Invoice Format - Detailed Analysis

**Based on:** `taxinvoice_page-0001.jpg`

---

## 📐 Layout Structure

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                          Tax Invoice                              Original          │
│ (issued under section 31 of central goods & service tax act 2017 and               │
│  maharashtra state goods & service tax act 2017)                                    │
│                     SUN ELECTRO DEVICES PVT LTD.                                    │
│  Plot No. A-44/1/2/19-20, Chakan MIDC,Phase II Wasuli, Pune – 410501,             │
│  Maharashtra, India                                                                 │
├─────────────────────────────────────────────┬───────────────────────────────────────┤
│ Date Of Invoice  : 24/10/2025               │ Transporatation Mode  :               │
│ Invoice Serial No: SUN252605801             │ Vehicle No            :  PO No.:1D03AE0023│
│ GSTIN No         : 27AANCS2439P1ZL          │ Date & Time of Supply : 24/10/2025  19:09│
│ E-Way Bill No    :                          │ Place Of Supply       : Maharashtra   │
├─────────────────────────────────────────────┼───────────────────────────────────────┤
│          Details Of Recipient               │         Details Of Delivery           │
├─────────────────────────────────────────────┼───────────────────────────────────────┤
│ Name    : LUCAS-TVS Limited                 │ Name    : LUCAS-TVS Limited           │
│ Address : B-1/1MIDC Industrial              │ Address : B-1/1MIDC Industrial        │
│           Area,Chakan,Tal-Khed,             │           Area,Chakan,Tal-Khed,       │
│           Dist-Pune-410 501                 │           Dist-Pune-410 501           │
│ State Name : Maharashtra                    │ State Name : Maharashtra              │
│ State Code : 27                             │ State Code : 27                       │
│ GSTIN No   : 27AAACL3763E1ZN                │ GSTIN No   : 27AAACL3763E1ZN          │
├──────┬──────────────────────────────────────┼─────────┬──────┬──────┬─────────┬─────┤
│Sr.No │  Description Of Goods Or Services    │ HSN/SAC │ UOM  │ Qty  │Rate/Unit│Taxable Value│
├──────┼──────────────────────────────────────┼─────────┼──────┼──────┼─────────┼─────┤
│  1   │26728738 - BRUSH PLATE ASSEMBLY       │ 85443000│ NOS  │300.00│  123.46 │37,038.00│
│      │                                      │         │      │      │         │     │
│      │                                      │         │      │      │         │     │
│      │                                      │         │      │      │         │     │
├──────┴──────────────────────────────────────┴─────────┴──────┴──────┴─────────┴─────┤
│ Less:    Discount                                                              0.00 │
│ Add:     Packing & Forwarding Charges                                         0.00 │
│ Add:     Frieght & Insurance                                                  0.00 │
│ Add:     Other Charges                                                        0.00 │
│          Taxable Value                                                   37,038.00 │
│          Central Tax @ 9.00 %                                             3,333.00 │
│          State/Union Territory Tax @ 9.00 %                               3,333.00 │
│          Integrated Tax @ 0.00 %                                              0.00 │
├──────────────────────────────────────────────────────────────────────────┬─────────┤
│ Forty-Three Thousand Seven Hundred Four Only                            │43,704.00│
├──────────────────────────────────────────────────────────────────────────┴─────────┤
│ ( Certify that particular given are true and correct amount represents the price  │
│   actually charged & there is no flow of additional consideration directly or     │
│   indirectly from the buyer )                                                      │
│                                                                                    │
│ Terms And Conditions:                                                              │
│ 1) Goods Once Sold will not be taken back.                                        │
│ 2)Unless informed at time of receipt, no shortages claim will be accepted.       │
│ 3) Interest Rate @24% P.A. applicable for Overdue Payment.                       │
│                                                                                    │
│  ┌──────────┐                                                                      │
│  │   QR     │   IRN:- f33f18700034cc9bb32e9836c56fdfa0e1de49f1d961d8b7f8355e722a5c1cb│
│  │  CODE    │   Ack No:- 122529238217864                                          │
│  └──────────┘   Ack Date:- 2025-10-24 19:17:00                                    │
│                                                                                    │
│                                                   Signature / Digital Signature of │
│                                                   Authorised Signatory             │
└────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Field Mapping (Exact Order)

### **Section 1: Header**
| Field | Value | Position |
|-------|-------|----------|
| Title | "Tax Invoice" | Center, Bold, Large |
| Copy Type | "Original" | Top Right |
| Legal Text | "(issued under section 31...)" | Below title, small font |
| Company Name | "SUN ELECTRO DEVICES PVT LTD." | Center, Bold |
| Address | "Plot No. A-44/1/2/19-20..." | Center, smaller font |

### **Section 2: Invoice Header (Left Column)**
| Field | Label | Value |
|-------|-------|-------|
| Date Of Invoice | "Date Of Invoice" | 24/10/2025 |
| Invoice Serial No | "Invoice Serial No" | SUN252605801 |
| GSTIN No | "GSTIN No" | 27AANCS2439P1ZL |
| E-Way Bill No | "E-Way Bill No" | (empty) |

### **Section 2: Invoice Header (Right Column)**
| Field | Label | Value |
|-------|-------|-------|
| Transportation Mode | "Transporatation Mode" | (empty) |
| Vehicle No | "Vehicle No" | (empty) |
| PO No | "PO No." | 1D03AE0023 |
| Date & Time of Supply | "Date & Time of Supply" | 24/10/2025  19:09 |
| Place Of Supply | "Place Of Supply" | Maharashtra |

### **Section 3: Recipient Details (Left)**
| Field | Label | Value |
|-------|-------|-------|
| Name | "Name" | LUCAS-TVS Limited |
| Address | "Address" | B-1/1MIDC Industrial Area,Chakan,Tal-Khed,Dist-Pune-410 501 |
| State Name | "State Name" | Maharashtra |
| State Code | "State Code" | 27 |
| GSTIN No | "GSTIN No" | 27AAACL3763E1ZN |

### **Section 3: Delivery Details (Right)**
| Field | Label | Value |
|-------|-------|-------|
| Name | "Name" | LUCAS-TVS Limited |
| Address | "Address" | B-1/1MIDC Industrial Area,Chakan,Tal-Khed,Dist-Pune-410 501 |
| State Name | "State Name" | Maharashtra |
| State Code | "State Code" | 27 |
| GSTIN No | "GSTIN No" | 27AAACL3763E1ZN |

### **Section 4: Line Items Table**
| Column | Header |
|--------|--------|
| 1 | Sr. No |
| 2 | Description Of Goods Or Services |
| 3 | HSN/SAC |
| 4 | UOM |
| 5 | Qty |
| 6 | Rate/Unit |
| 7 | Taxable Value |

**Example Row:**
- Sr. No: 1
- Description: 26728738 - BRUSH PLATE ASSEMBLY
- HSN/SAC: 85443000
- UOM: NOS
- Qty: 300.00
- Rate/Unit: 123.46
- Taxable Value: 37,038.00

### **Section 5: Totals**
| Label | Value |
|-------|-------|
| Less: Discount | 0.00 |
| Add: Packing & Forwarding Charges | 0.00 |
| Add: Frieght & Insurance | 0.00 |
| Add: Other Charges | 0.00 |
| Taxable Value | 37,038.00 |
| Central Tax @ 9.00 % | 3,333.00 |
| State/Union Territory Tax @ 9.00 % | 3,333.00 |
| Integrated Tax @ 0.00 % | 0.00 |

### **Section 6: Amount in Words + Grand Total**
| Field | Value |
|-------|-------|
| Amount in Words | Forty-Three Thousand Seven Hundred Four Only |
| Grand Total | 43,704.00 |

### **Section 7: Declaration**
```
( Certify that particular given are true and correct amount represents the price 
  actually charged & there is no flow of additional consideration directly or 
  indirectly from the buyer )
```

### **Section 8: Terms and Conditions**
1. Goods Once Sold will not be taken back.
2. Unless informed at time of receipt, no shortages claim will be accepted.
3. Interest Rate @24% P.A. applicable for Overdue Payment.

### **Section 9: E-Invoice Section (Bottom Left)**
| Field | Label | Value |
|-------|-------|-------|
| QR Code | [QR CODE IMAGE] | (large QR code) |
| IRN | "IRN:-" | f33f18700034cc9bb32e9836c56fdfa0e1de49f1d961d8b7f8355e722a5c1cb |
| Ack No | "Ack No:-" | 122529238217864 |
| Ack Date | "Ack Date:-" | 2025-10-24 19:17:00 |

### **Section 10: Signature (Bottom Right)**
```
Signature / Digital Signature of
Authorised Signatory
```

---

## 🎨 Design Specifications

### Borders:
- Entire invoice: **Double border** (thick outer, thin inner)
- Section dividers: **Single lines**
- Table: **Full grid borders**

### Fonts:
- Title: **Large, Bold**
- Headers: **Bold**
- Body text: **Regular**
- Legal text: **Small font**

### Alignment:
- Company info: **Center**
- Labels: **Left aligned**
- Values: **Left aligned (after colon)**
- Numbers: **Right aligned**
- Amount in words: **Left aligned**

### Spacing:
- Compact layout (minimal padding)
- Clear section separation with borders

---

## ❌ Key Differences from My Generated PDF

### Missing Fields in My Version:
1. ❌ Legal text "(issued under section 31...)"
2. ❌ Invoice Serial No (I used Invoice Number)
3. ❌ Date & Time of Supply (I only had date)
4. ❌ Place Of Supply
5. ❌ State Code (for both recipient and delivery)
6. ❌ "Details Of Recipient" vs "Details Of Delivery" (two separate sections)
7. ❌ Discount, Packing, Freight, Other Charges rows
8. ❌ Tax labels: "Central Tax" and "State/Union Territory Tax" (I used CGST/SGST)
9. ❌ Declaration text "(Certify that particular...)"
10. ❌ Double border design

### Wrong Structure:
1. ❌ Header layout completely different
2. ❌ Left/Right columns not matching
3. ❌ Table structure different
4. ❌ Totals section layout different
5. ❌ E-Invoice section positioning wrong

---

## 📊 Database Column Mapping

### Company Information:
- Company Name: `CM_NAME`
- Address: `CM_ADDRESS1`, `CM_ADDRESS2`, `CM_ADDRESS3`, `CM_CITY`, `CM_STATE`
- GSTIN: `CM_GST_NO`

### Invoice Header:
- Date: `INM_DATE`
- Serial No: `INM_TNO` ⭐ (This is the serial number!)
- GSTIN: From Company Master
- E-Way Bill: `INM_EWAY_BILL_NO` (if exists)
- PO No: From `CUSTPO_MASTER.CPOM_PONO`
- Date & Time: `INM_DATE` + time component
- Place of Supply: `INM_PLACE_OF_SUPPLY` or State

### Recipient/Delivery:
- Name: `PM_PARTY_NAME`
- Address: `PM_ADDRESS1`, `PM_ADDRESS2`, `PM_CITY`
- State: `PM_STATE`
- State Code: Need to map state to code
- GSTIN: `PM_GSTN_NO`

### Line Items:
- Description: `IM_ITEM_NAME` or item description
- HSN: Item HSN code
- UOM: `UM_UNIT` (from unit master)
- Qty: `IND_QTY`
- Rate: `IND_RATE`
- Taxable Value: `IND_AMT`

### Totals:
- Discount: `INM_DISC_AMT`
- Packing: `INM_PACK_AMT`
- Freight: `INM_FREIGHT`
- Other: `INM_OTHER_AMT`
- Taxable Value: Sum of line items after adjustments
- CGST: Sum of CGST from line items
- SGST: Sum of SGST from line items
- IGST: Sum of IGST from line items
- Grand Total: `INM_G_AMT`

### E-Invoice:
- IRN: E-Invoice IRN field
- Ack No: E-Invoice Acknowledgement Number
- Ack Date: E-Invoice Acknowledgement Date

---

## 🎯 Action Plan

1. **Completely rewrite** `TaxInvoicePdfService.cs`
2. Match **exact layout** from image
3. Add **all missing fields**
4. Use **correct labels** (not CGST/SGST, but "Central Tax" and "State/Union Territory Tax")
5. Implement **double border** design
6. Match **exact spacing and alignment**
7. Add **declaration text**
8. Fix **E-Invoice section** positioning

---

**Status:** Ready to implement CORRECT version

