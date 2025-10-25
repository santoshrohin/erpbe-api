# Tax Invoice Print - Analysis & Implementation Plan

## 📋 Overview

**Current Implementation**: Legacy ASP.NET Web Forms with Crystal Reports (.rpt files)  
**Target**: Modern Clean Architecture API with PDF generation  
**Date**: October 25, 2025

---

## 🔍 Legacy Implementation Analysis

### Files Identified:
1. **ViewTaxInvoice.aspx/.cs** - List view with Print button
2. **TaxInvoicePrint.aspx/.cs** - Print page that loads Crystal Report
3. **Crystal Reports**:
   - `rptTaxInvoice.rpt` - Standard Tax Invoice
   - `rptTaxInvoiceGST.rpt` - GST Tax Invoice  
   - `rptETaxInvoice.rpt` - E-Invoice format
   - Multiple other variations (Labour, Register, Plain Text, etc.)

### Key Features:

#### 1. **Print Options**
- **Single Invoice** - Print one invoice
- **Multiple Invoices** - Print range of invoices (from INM_NO to toNo)

#### 2. **Copy Types** (Multiple copies of same invoice)
- ✅ **Original** - For buyer/customer
- ✅ **Duplicate** - Office copy
- ✅ **Triplicate** - Accounts copy  
- ✅ **Extra Copy** - Additional copy

User can select which copies to print (checkboxes).

#### 3. **Report Parameters** (Company Information)
From session:
- Company Phone
- Company Fax
- Company VAT/TIN
- Company CST/TIN
- Company ECC No
- Company VAT WEF Date
- Company CST WEF Date
- Company ISO
- Company Registration
- Company Website
- Company Email
- Company GST No

#### 4. **Data Query**
**Massive SQL Query** joining multiple tables:
- INVOICE_MASTER
- INVOICE_DETAIL
- CUSTPO_MASTER
- CUSTPO_DETAIL
- PARTY_MASTER (Customer)
- ITEM_MASTER
- ITEM_UNIT_MASTER
- EXCISE_TARIFF_MASTER
- STATE_MASTER (2 instances - company state & customer state)
- COMPANY_MASTER

**Includes**:
- Invoice header data
- Line items with HSN codes
- Customer details with GST
- Tax calculations (GST, Excise, Cess, etc.)
- E-Invoice data (IRN, AckNo, QR Code)
- Amortization details
- Packing details

#### 5. **E-Invoice Integration**
- **QR Code Generation** using ZXing library
- QR Code contains IRN data
- Embedded in PDF as image
- E-Way Bill number
- Acknowledgement number & date

#### 6. **GST Compliance**
- State codes (company & customer)
- GST numbers
- HSN codes for items
- IGST/CGST/SGST calculations
- Place of supply

---

## 🚫 Challenges with Legacy Approach

### 1. **Crystal Reports Dependency**
- ❌ Requires Crystal Reports runtime
- ❌ Not cross-platform (.NET Framework only)
- ❌ Licensing issues
- ❌ Hard to maintain .rpt files
- ❌ Not suitable for REST API

### 2. **Session Dependency**
- ❌ Relies heavily on Session state
- ❌ Not stateless (REST principle violated)

### 3. **Direct Database Access**
- ❌ Inline SQL queries (not using stored procedures)
- ❌ No repository pattern

---

## ✅ Modern Implementation Approach

### Option 1: **HTML to PDF** (RECOMMENDED)
**Libraries**: 
- `iText7` - PDF generation
- `DinkToPdf` - HTML to PDF conversion
- `QuestPDF` - Fluent PDF API

**Advantages**:
- ✅ Full control over layout
- ✅ Modern, maintainable
- ✅ Cross-platform (.NET 9)
- ✅ No licensing issues
- ✅ Easy to test
- ✅ Can generate HTML preview first
- ✅ Responsive design possible

**Implementation**:
1. Create Razor template for invoice
2. Populate with data from API
3. Convert HTML → PDF
4. Return as file download or base64

### Option 2: **QuestPDF** (Alternative)
**Library**: `QuestPDF`

**Advantages**:
- ✅ Fluent C# API (no HTML needed)
- ✅ Very fast
- ✅ Great for complex layouts
- ✅ Free for commercial use

**Implementation**:
1. Create PDF document using fluent API
2. Define layout programmatically
3. Generate PDF directly

### Option 3: **Telerik Reporting** (Enterprise)
**Not Recommended** - Similar issues to Crystal Reports

---

## 🎯 Recommended Solution: **HTML to PDF with Razor**

### Architecture:

```
API Layer (Controller)
    ↓
CQRS Query (GetTaxInvoicePrintData)
    ↓
Query Handler
    ↓
Repository (Get Invoice + Company Data)
    ↓
PDF Service
    ↓
Razor Template Engine
    ↓
HTML → PDF Converter
    ↓
Return PDF (byte[] or FileStream)
```

---

## 📊 Data Requirements for Print

### Invoice Data (from existing GetTaxInvoiceByIdQuery)
- Invoice Master (all fields)
- Invoice Details (all line items)
- Customer details
- Tax calculations
- E-Invoice data (IRN, QR, Ack)

### Additional Data Needed:
1. **Company Master** - Company information for header
2. **State Master** - State names and codes  
3. **E-Invoice QR Code** - As image

---

## 🏗️ Implementation Plan

### Phase 1: Create Print Query & DTO
- [ ] Create `GetTaxInvoicePrintDataQuery`
- [ ] Create `TaxInvoicePrintDto` (comprehensive DTO)
- [ ] Include company, customer, items, taxes, e-invoice

### Phase 2: Create PDF Service
- [ ] Install `DinkToPdf` or `QuestPDF`
- [ ] Create `IPdfService` interface
- [ ] Implement `PdfService`
- [ ] QR Code generation service

### Phase 3: Create Razor Template
- [ ] Create `TaxInvoiceTemplate.cshtml`
- [ ] Match legacy invoice layout
- [ ] Include all sections:
  - Company header
  - Invoice details
  - Customer details  
  - Line items table
  - Tax summary
  - E-Invoice section (QR, IRN, Ack)
  - Terms & conditions

### Phase 4: Create Print Endpoint
- [ ] Create `PrintTaxInvoiceQuery`
- [ ] Support copy types (Original, Duplicate, etc.)
- [ ] Support multiple invoices
- [ ] Return PDF as FileResult

### Phase 5: Testing
- [ ] Unit tests for PDF service
- [ ] Integration tests for print endpoint
- [ ] Verify PDF output matches legacy format

---

## 🎨 Invoice Template Requirements

### Header Section:
- Company logo
- Company name, address
- Company GST, PAN, CIN
- Company contact (phone, email, website)

### Invoice Details:
- Invoice number
- Invoice date
- Customer PO number & date
- Transport details (LR No, Vehicle No, etc.)

### Customer Section:
- Bill To address
- Ship To address
- Customer GST number
- State code

### Line Items Table:
| S.No | Item Description | HSN Code | Qty | UOM | Rate | Amount | Tax | Total |
|------|------------------|----------|-----|-----|------|--------|-----|-------|

### Tax Summary:
- Taxable Amount
- CGST / SGST / IGST
- Total Tax
- Grand Total
- Amount in words

### E-Invoice Section (if applicable):
- IRN number
- QR Code
- Acknowledgement No & Date
- E-Way Bill No

### Footer:
- Bank details
- Terms & conditions
- Authorized signatory

---

## 📝 Copy Types Implementation

```csharp
public enum InvoiceCopyType
{
    Original = 1,
    Duplicate = 2,
    Triplicate = 3,
    ExtraCopy = 4
}

public class PrintTaxInvoiceRequest
{
    public int InvoiceCode { get; set; }
    public int CompanyId { get; set; }
    public List<InvoiceCopyType> CopyTypes { get; set; }
    public bool PrintMultiple { get; set; }
    public int? ToInvoiceNumber { get; set; }
}
```

**Each copy** gets a watermark/header indicating the copy type.

---

## 🔧 Technical Stack

### NuGet Packages Required:
```xml
<PackageReference Include="QuestPDF" Version="2024.10.0" />
<PackageReference Include="QRCoder" Version="1.6.0" />
<PackageReference Include="RazorLight" Version="2.3.0" />
```

OR

```xml
<PackageReference Include="DinkToPdf" Version="1.0.8" />
<PackageReference Include="QRCoder" Version="1.6.0" />
```

---

## 📊 Sample API Endpoint

### GET /api/TaxInvoice/{id}/print
**Query Parameters**:
- `companyId` (required)
- `copyTypes` (comma-separated: 1,2,3,4)
- `format` (pdf, html) - for preview

**Response**:
- Content-Type: `application/pdf`
- File download with name: `TaxInvoice_{InvoiceNo}.pdf`

### POST /api/TaxInvoice/print/batch
**Request Body**:
```json
{
  "invoiceCodes": [1, 2, 3],
  "companyId": 1,
  "copyTypes": [1, 2],
  "mergeIntoSingle": true
}
```

**Response**:
- Single merged PDF or ZIP of individual PDFs

---

## ✅ Advantages of Modern Approach

1. ✅ **No Crystal Reports** - Modern, maintainable
2. ✅ **Stateless** - No session dependency
3. ✅ **Cross-platform** - Works on Linux/Docker
4. ✅ **Easy to modify** - HTML/CSS templates
5. ✅ **Testable** - Unit & integration tests
6. ✅ **API-first** - Can be consumed by any client
7. ✅ **Brandable** - Easy to customize per company
8. ✅ **Version control** - Templates in Git
9. ✅ **No licensing** - Open source libraries
10. ✅ **Fast** - Direct PDF generation

---

## 🎯 Next Steps

### Step 1: Choose PDF Library
**Recommendation**: Start with **QuestPDF** for ease of use

### Step 2: Create Stored Procedure
Create `ERP_GetTaxInvoicePrintData` that returns:
- Invoice master
- Invoice details  
- Customer details
- Company details
- Tax calculations

### Step 3: Create DTO & Query
- `TaxInvoicePrintDto`
- `GetTaxInvoicePrintDataQuery`

### Step 4: Implement PDF Service
- Layout invoice sections
- Add QR code
- Add company logo
- Format currency, dates

### Step 5: Create Controller Endpoint
- `PrintController` or add to `TaxInvoiceController`
- Return `FileResult`

### Step 6: Test
- Generate sample invoices
- Compare with legacy output
- Verify GST compliance
- Test E-Invoice integration

---

## 📋 Deliverables

1. ✅ `TaxInvoicePrintDto.cs` - Comprehensive DTO
2. ✅ `GetTaxInvoicePrintDataQuery.cs` - CQRS Query
3. ✅ `GetTaxInvoicePrintDataQueryHandler.cs` - Handler
4. ✅ `IPdfService.cs` - PDF service interface
5. ✅ `QuestPdfService.cs` - QuestPDF implementation
6. ✅ `TaxInvoiceDocument.cs` - QuestPDF document class
7. ✅ `ERP_GetTaxInvoicePrintData.sql` - Stored procedure
8. ✅ Print endpoint in controller
9. ✅ Integration tests

---

## 🎉 Expected Outcome

A modern, maintainable, testable PDF generation system that:
- ✅ Matches legacy invoice layout
- ✅ Supports all copy types
- ✅ Includes E-Invoice compliance
- ✅ Works in Clean Architecture
- ✅ Can be consumed by any client (Web, Mobile, Desktop)

**Ready to implement!** 🚀

