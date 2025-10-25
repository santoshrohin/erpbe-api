# Tax Invoice Format Analysis - From Actual Invoice

## Invoice Details from Image

### Header Information
- **Company Name**: SUN ELECTRO DEVICES PVT LTD.
- **Invoice Type**: Tax Invoice (Original)
- **Compliance**: GST Act 2017 and Maharashtra State Goods & Service Tax Act 2017
- **Invoice Serial No**: SUN252605801
- **Date of Invoice**: 24/10/2025
- **GSTIN No**: 27AANCS2439P1ZL
- **E-Way Bill No**: (Empty in image)

### Recipient/Billing Details
- **Name**: LUCAS-TVS Limited
- **Address**: B-1/1MIDC Industrial Area, Chakan, Tal-Khed, Dist-Pune-410 501
- **State Name**: Maharashtra
- **State Code**: 27
- **GSTIN No**: 27AAACL3763E1ZN

### Delivery Details
- **Name**: LUCAS-TVS Limited
- **Address**: B-1/1MIDC Industrial Area, Chakan, Tal-Khed, Dist-Pune-410 501
- **State Name**: Maharashtra
- **State Code**: 27
- **GSTIN No**: 27AAACL3763E1ZN

### Transport Details
- **Transportation Mode**: (Empty)
- **Vehicle No**: (Empty)
- **PO No**: 1D03AE0023
- **Date & Time of Supply**: 24/10/2025 19:09
- **Place Of Supply**: Maharashtra

### Line Items
| Sr. No | Description | HSN/SAC | UOM | Qty | Rate/Unit | Taxable Value |
|--------|-------------|---------|-----|-----|-----------|---------------|
| 1 | 26728738 - BRUSH PLATE ASSEMBLY | 85443000 | NOS | 300.00 | 123.46 | 37,038.00 |

### Tax Summary
```
Less:     Discount                                    0.00
Add:      Packing & Forwarding Charges               0.00
Add:      Frieght & Insurance                        0.00
Add:      Other Charges                              0.00
          Taxable Value                         37,038.00
          Central Tax @ 9.00 %                   3,333.00
          State/Union Territory Tax @ 9.00 %     3,333.00
          Integrated Tax @ 0.00 %                    0.00
          ───────────────────────────────────────────────
          TOTAL                                 43,704.00
```

### Amount in Words
**Forty-Three Thousand Seven Hundred Four Only**

### E-Invoice Details
- **IRN**: f33f187000434cc9bb32e9836c56fdfa0e1de49f1d961d8b7f8355e722a5c1cb
- **Ack No**: 122529238217864
- **Ack Date**: 2025-10-24 19:17:00
- **QR Code**: Present (with embedded data)

### Terms and Conditions
1) Goods Once Sold will not be taken back.
2) Unless informed at time of receipt, no shortages claim will be accepted.
3) Interest Rate @24% P. A. applicable for Overdue Payment.

### Footer
- Signature / Digital Signature of Authorised Signatory

---

## Database Mapping Requirements

Based on the image, here's what we need to map:

### INVOICE_MASTER Columns Needed:
1. `INM_NO` → Invoice Serial No (SUN252605801)
2. `INM_DATE` → Date of Invoice
3. `INM_P_CODE` → Customer Code (join to PARTY_MASTER for customer details)
4. `INM_TRANSPORT` → Transportation Mode
5. `INM_VEH_NO` → Vehicle No
6. `INM_LR_NO` → LR No (not shown in image)
7. `INM_LR_DATE` → LR Date
8. `INM_NET_AMT` → Basic Amount (37,038.00)
9. `INM_DISC` → Discount Percentage
10. `INM_DISC_AMT` → Discount Amount (0.00)
11. `INM_PACK_AMT` → Packing & Forwarding Charges (0.00)
12. `INM_FREIGHT` → Freight & Insurance (0.00)
13. `INM_INSURANCE` → Insurance (part of freight)
14. `INM_OTHER_AMT` → Other Charges (0.00)
15. `INM_ROUNDING_AMT` → Rounding Amount
16. `INM_G_AMT` → Grand Total (43,704.00)
17. `INM_REMARK` → Remarks
18. `INM_TERMSNCONDITIONS` → Terms & Conditions
19. `INM_TNO` → Reference Number / PO No (1D03AE0023)
20. **GST fields** → Need to identify actual column names

### INVOICE_DETAIL Columns Needed:
1. `IND_I_CODE` → Item Code (join to ITEM_MASTER)
2. `IND_INQTY` → Quantity (300.00)
3. `IND_UOM_CODE` → UOM Code (join to ITEM_UNIT_MASTER for "NOS")
4. `IND_RATE` → Rate/Unit (123.46)
5. `IND_AMT` → Taxable Value (37,038.00)
6. `IND_HSN_CODE` → HSN Code (85443000)
7. `IND_NO_PACK` → Number of Packages
8. `IND_PACK_DESC` → Packing Description
9. **Tax columns** → E_BASIC_CentralT, E_EDU_CESS_State, E_H_EDU_Integrated
10. `IND_REMARK` → Description

### PARTY_MASTER Columns Needed:
1. `P_CODE` → Customer Code
2. `P_NAME` → Customer Name (LUCAS-TVS Limited)
3. `P_ADD1` → Address Line 1
4. `P_CITY` → City
5. `P_PIN_CODE` → Pin Code
6. `P_PHONE` → Phone
7. `P_EMAIL` → Email
8. `P_GST_NO` → GSTIN (27AAACL3763E1ZN)
9. `P_PAN` → PAN Number
10. `P_CONTACT` → Contact Person
11. State info → Need to join to STATE_MASTER

### COMPANY_MASTER Columns Needed:
1. `CM_NAME` → Company Name (SUN ELECTRO DEVICES PVT LTD.)
2. `CM_ADDRESS1`, `CM_ADDRESS2`, `CM_ADDRESS3` → Address
3. `CM_CITY` → City (Pune)
4. `CM_STATE` → State (Maharashtra)
5. `CM_PHONENO1` → Phone
6. `CM_FAXNO` → Fax
7. `CM_EMAILID` → Email
8. `CM_WEBSITE` → Website
9. `CM_GST_NO` → GSTIN (27AANCS2439P1ZL) - **Need to verify column name**
10. `CM_PAN_NO` → PAN - **Need to verify column name**

### E-Invoice Columns (if exist):
- IRN
- AckNo (Acknowledgement Number)
- AckDate (Acknowledgement Date)
- QRCode (QR Code data/image)
- EwayBill (E-way Bill Number)
- EInvStatus

---

## Key Observations

1. **Tax Structure**: Uses CGST (9%) + SGST (9%) = 18% total GST
2. **No IGST**: As both supplier and recipient are in Maharashtra (same state)
3. **E-Invoice**: Fully compliant with QR code, IRN, and Ack No
4. **Serial Number Format**: Prefix "SUN" + Date-based number
5. **HSN Code**: 8-digit HSN code (85443000)
6. **UOM**: Text format ("NOS")
7. **Amounts**: All in INR, 2 decimal places
8. **Layout**: Professional GST-compliant format with clear sections

---

## Next Steps

1. Query actual invoice data (Code: -2147418909) to see real column values
2. Identify GST column names (E_BASIC_CentralT = CGST?, E_EDU_CESS_State = SGST?, E_H_EDU_Integrated = IGST?)
3. Check if E-Invoice columns exist in INVOICE_MASTER
4. Map company GST/PAN column names
5. Create corrected stored procedure
6. Design QuestPDF layout to match the image exactly

