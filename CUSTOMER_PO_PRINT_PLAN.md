# Customer PO (Sales Order) Print Implementation Plan

## Analysis from D:\Invoice\CustomerPO_page-0001.jpg

### Document Structure

**Header:**
- Title: "Sales Order" (centered, underlined, bold)
- Form Number: "FORM NO : MK/F/05,REV.0" (top-right box)

**Company Section:**
- Company Name: SUN ELECTRO DEVICES PVT LTD.
- Address: Full address with phone and fax
- Sale Order No: 1539
- Sale Order Date: 18 Oct 2025

**Customer Section:**
- Label: "Invoice To,"
- Customer Name: UNO MINDARIKA PVT LTD
- Customer Address
- PO No: 5500043854
- PO Date: 12/08/2025
- Consignee field
- Transport Through field

**Line Items Table:**
| Sr. No | Item Name | Qty | Unit | Rate | Amount |
|--------|-----------|-----|------|------|--------|
| 1 | SLEEVE 14SW110509-00017Y0 | 0.000 | NOS | 0.59 | 0.00 |

**Footer Section:**
- Total Qty: 0.000
- Assessable Value: 0.00
- Order Amount: Only (in words)
- Delivery Terms
- Narrations
- Central Tax: 0.00
- State/Union Territory Tax: 0.00
- Total Amount: 0.00

**Signature:**
- "For POOJA CASTING PVT. LTD."
- "Authorised Signatory"

**Company Details:**
- Factory address with phone
- Head Office address with phone, fax, email, website

## Implementation Tasks

### 1. Database & DTOs
- [ ] Analyze CustomerPO database structure
- [ ] Create `CustomerPoPrintDto.cs` with all required fields
- [ ] Create stored procedure `ERP_GetCustomerPoPrintData`
- [ ] Update `ICustomerPoRepository` with `GetPrintDataAsync` method
- [ ] Implement repository method

### 2. PDF Service
- [ ] Create `ICustomerPoPdfService` interface
- [ ] Create `CustomerPoPdfService.cs` with QuestPDF
- [ ] Implement exact layout matching the image
- [ ] Add Indian number formatting for amounts
- [ ] Add number-to-words conversion for total amount

### 3. Controller & Endpoints
- [ ] Add print endpoint to `CustomerPoController`
- [ ] Support copyType parameter (Original, Duplicate, Triplicate, Quadruplicate)
- [ ] Add batch print endpoint

### 4. Testing
- [ ] Test with sample PO data
- [ ] Verify exact layout match
- [ ] Test all copy types
- [ ] Test batch printing

## Key Differences from Tax Invoice

1. **Simpler Layout**: No dual borders, single border
2. **Different Header**: "Sales Order" instead of "Tax Invoice"
3. **No GSTIN**: Just customer PO details
4. **Simpler Footer**: No complex terms and conditions
5. **Company Details**: Factory + Head Office at bottom

## Database Mapping (To Be Verified)

- CUSTPO_MASTER table for header
- CUSTPODETAIL table for line items
- COMPANY_MASTER for company details
- PARTY_MASTER for customer details

## Timeline

This should take approximately 2-3 hours to implement and test thoroughly.

