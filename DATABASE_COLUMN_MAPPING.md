# Database Column Mapping - Tax Invoice Print

## ✅ CONFIRMED Column Mappings

### INVOICE_MASTER
| Purpose | Column Name | Sample Value | Type |
|---------|-------------|--------------|------|
| Invoice Code | `INM_CODE` | -2147418909 | int |
| Invoice Number | `INM_NO` | 5704 | int |
| Invoice Serial | `INM_TNO` | SUN252605704 | varchar |
| Invoice Date | `INM_DATE` | 10/18/2025 | datetime |
| Customer Code | `INM_P_CODE` | -2147483104 | int |
| Transport Mode | `INM_TRANSPORT` | (empty) | varchar |
| Vehicle Number | `INM_VEH_NO` | (empty) | varchar |
| LR Number | `INM_LR_NO` | (empty) | varchar |
| LR Date | `INM_LR_DATE` | 10/18/2025 | datetime |
| Net Amount | `INM_NET_AMT` | 15011.2 | decimal |
| Discount % | `INM_DISC` | 0 | decimal |
| Discount Amount | `INM_DISC_AMT` | 0 | decimal |
| Packing Amount | `INM_PACK_AMT` | 0 | decimal |
| Freight | `INM_FREIGHT` | 0 | decimal |
| Insurance | `INM_INSURANCE` | 0 | decimal |
| Other Amount | `INM_OTHER_AMT` | 0 | decimal |
| Rounding Amount | `INM_ROUNDING_AMT` | 0 | decimal |
| Grand Total | `INM_G_AMT` | 17713.2 | decimal |
| Taxable Amount | `INM_TAXABLE_AMT` | 15011.2 | decimal |
| TCS % | `INM_TAX_TCS` | 0 | decimal |
| TCS Amount | `INM_TAX_TCS_AMT` | 0 | decimal |
| Remarks | `INM_REMARK` | (empty) | varchar |
| Terms & Conditions | `INM_TERMSNCONDITIONS` | (empty) | text |
| **E-Invoice IRN** | `IRN` | ✅ EXISTS | varchar |
| **E-Invoice Ack No** | `AckNo` | ✅ EXISTS | varchar |
| **E-Invoice Ack Date** | `AckDate` | ✅ EXISTS | datetime |
| **E-Way Bill** | `EwayBill` | ✅ EXISTS | varchar |
| **QR Code** | `QRCode` | ✅ EXISTS | varchar/image |
| **E-Invoice Status** | `EInvStatus` | ✅ EXISTS | varchar |

### INVOICE_DETAIL
| Purpose | Column Name | Sample Value | Type |
|---------|-------------|--------------|------|
| Item Code | `IND_I_CODE` | -2147481573 | int |
| Quantity | `IND_INQTY` | 320 | decimal |
| UOM Code | `IND_UOM_CODE` | (NULL) | int |
| Rate | `IND_RATE` | 46.91 | decimal |
| Amount | `IND_AMT` | 15011.2 | decimal |
| HSN Code | `IND_HSN_CODE` | 85389000 | varchar |
| Number of Packages | `IND_NO_PACK` | 0 | int |
| Packing Description | `IND_PACK_DESC` | (empty) | varchar |
| Remark/Description | `IND_REMARK` | (NULL) | varchar |
| **CGST %** | `E_BASIC_CentralT` | 9 | decimal |
| **SGST %** | `E_EDU_CESS_State` | 9 | decimal |
| **IGST %** | `E_H_EDU_Integrated` | 0 | decimal |

### COMPANY_MASTER
| Purpose | Column Name | Confirmed |
|---------|-------------|-----------|
| Company Name | `CM_NAME` | ✅ |
| Address 1 | `CM_ADDRESS1` | ✅ |
| Address 2 | `CM_ADDRESS2` | ✅ |
| Address 3 | `CM_ADDRESS3` | ✅ |
| City | `CM_CITY` | ✅ |
| State | `CM_STATE` | ✅ |
| Phone | `CM_PHONENO1` | ✅ |
| Fax | `CM_FAXNO` | ✅ |
| Email | `CM_EMAILID` | ✅ |
| Website | `CM_WEBSITE` | ✅ |
| **GST Number** | `CM_GST_NO` | ✅ EXISTS |
| **PAN Number** | `CM_PAN_NO` | ✅ EXISTS |
| **CIN Number** | `CM_CIN_NO` | ✅ EXISTS |
| VAT TIN | `CM_VAT_TIN_NO` | ✅ |
| CST Number | `CM_CST_NO` | ✅ |

### PARTY_MASTER
| Purpose | Column Name | Confirmed |
|---------|-------------|-----------|
| Customer Code | `P_CODE` | ✅ |
| Customer Name | `P_NAME` | ✅ |
| Address | `P_ADD1` | ✅ |
| City | `P_CITY` | ✅ |
| Pin Code | `P_PIN_CODE` | ✅ |
| Phone | `P_PHONE` | ✅ |
| Email | `P_EMAIL` | ✅ |
| **GST Number** | `P_GST_NO` | ✅ EXISTS |
| **PAN Number** | `P_PAN` | ✅ EXISTS |
| Contact Person | `P_CONTACT` | ✅ EXISTS |

### ITEM_MASTER
| Purpose | Column Name |
|---------|-------------|
| Item Code | `I_CODE` |
| Item Name | `I_NAME` |
| UOM Code | `I_UOM_CODE` |

### ITEM_UNIT_MASTER
| Purpose | Column Name |
|---------|-------------|
| UOM Code | `I_UOM_CODE` |
| UOM Name | `I_UOM_NAME` |

## 🎯 Tax Calculation Logic

Based on actual data:
- **CGST Amount** = `IND_AMT` * `E_BASIC_CentralT` / 100
- **SGST Amount** = `IND_AMT` * `E_EDU_CESS_State` / 100
- **IGST Amount** = `IND_AMT` * `E_H_EDU_Integrated` / 100
- **Total Tax** = CGST Amount + SGST Amount + IGST Amount
- **Total Amount** = `IND_AMT` + Total Tax

## 📝 Key Insights

1. **Invoice Serial Number** is in `INM_TNO` (not generated, already formatted like "SUN252605704")
2. **Tax percentages** are stored, amounts need to be calculated
3. **E-Invoice columns exist** in INVOICE_MASTER (IRN, AckNo, AckDate, QRCode, EwayBill, EInvStatus)
4. **UOM might be NULL** in INVOICE_DETAIL, need to get from ITEM_MASTER
5. **Item description** might be in `IND_REMARK` or join from ITEM_MASTER
6. **State Code** needs to be retrieved from STATE_MASTER join

## ✅ Ready for Implementation

All required columns have been confirmed to exist in the database!

