# PDF Copy Type Implementation

## Overview

Updated the Tax Invoice PDF generation to generate **multiple copies** based on the `copyType` parameter.

## Copy Type Behavior

| copyType | Value | Copies Generated | Labels on Each Copy |
|----------|-------|------------------|---------------------|
| **Original** | 0 | 1 copy | Original |
| **Duplicate** | 1 | 2 copies | Original, Duplicate |
| **Triplicate** | 2 | 3 copies | Original, Duplicate, Triplicate |
| **Quadruplicate** | 3 | 4 copies | Original, Duplicate, Triplicate, Quadruplicate |

## Changes Made

### 1. Updated `InvoiceCopyType` Enum
**File**: `ErpBE.Application/DTOs/TaxInvoicePrintDto.cs`

```csharp
public enum InvoiceCopyType
{
    Original = 0,        // 1 copy: Original
    Duplicate = 1,       // 2 copies: Original, Duplicate
    Triplicate = 2,      // 3 copies: Original, Duplicate, Triplicate
    Quadruplicate = 3    // 4 copies: Original, Duplicate, Triplicate, Quadruplicate
}
```

### 2. Updated PDF Generation Logic
**File**: `ErpBE.Infrastructure/Services/TaxInvoicePdfService.cs`

#### Added Helper Method
```csharp
private static List<InvoiceCopyType> GetCopyTypesForGeneration(InvoiceCopyType copyType)
{
    return copyType switch
    {
        InvoiceCopyType.Original => new List<InvoiceCopyType> { InvoiceCopyType.Original },
        InvoiceCopyType.Duplicate => new List<InvoiceCopyType> { InvoiceCopyType.Original, InvoiceCopyType.Duplicate },
        InvoiceCopyType.Triplicate => new List<InvoiceCopyType> { InvoiceCopyType.Original, InvoiceCopyType.Duplicate, InvoiceCopyType.Triplicate },
        InvoiceCopyType.Quadruplicate => new List<InvoiceCopyType> { InvoiceCopyType.Original, InvoiceCopyType.Duplicate, InvoiceCopyType.Triplicate, InvoiceCopyType.Quadruplicate },
        _ => new List<InvoiceCopyType> { InvoiceCopyType.Original }
    };
}
```

#### Updated `GenerateTaxInvoicePdf` Method
- Now generates multiple pages (one for each copy)
- Each page has the appropriate label (Original, Duplicate, etc.)

#### Updated `GenerateBatchTaxInvoicePdf` Method
- Handles multiple invoices with multiple copies each

#### Updated `GetCopyTypeName` Method
- Added "Quadruplicate" case

## API Endpoint

### Single Invoice Print
```
GET /api/TaxInvoice/{invoiceCode}/print?companyId={companyId}&copyType={copyType}
```

**Parameters**:
- `invoiceCode`: Invoice ID (e.g., `-2147419120`)
- `companyId`: Company ID (e.g., `1`)
- `copyType`: Copy type (0, 1, 2, or 3)

### Examples

```bash
# Original (1 copy)
http://localhost:5136/api/TaxInvoice/-2147419120/print?companyId=1&copyType=0

# Duplicate (2 copies: Original + Duplicate)
http://localhost:5136/api/TaxInvoice/-2147419120/print?companyId=1&copyType=1

# Triplicate (3 copies: Original + Duplicate + Triplicate)
http://localhost:5136/api/TaxInvoice/-2147419120/print?companyId=1&copyType=2

# Quadruplicate (4 copies: Original + Duplicate + Triplicate + Quadruplicate)
http://localhost:5136/api/TaxInvoice/-2147419120/print?companyId=1&copyType=3
```

## PDF Output

### copyType=0 (Original)
- **Pages**: 1
- **Page 1**: "Original" label

### copyType=1 (Duplicate)
- **Pages**: 2
- **Page 1**: "Original" label
- **Page 2**: "Duplicate" label

### copyType=2 (Triplicate)
- **Pages**: 3
- **Page 1**: "Original" label
- **Page 2**: "Duplicate" label
- **Page 3**: "Triplicate" label

### copyType=3 (Quadruplicate)
- **Pages**: 4
- **Page 1**: "Original" label
- **Page 2**: "Duplicate" label
- **Page 3**: "Triplicate" label
- **Page 4**: "Quadruplicate" label

## Testing

To test the implementation:

1. **Start the API** (already running on `http://localhost:5136`)

2. **Test each copy type**:
   - Open each URL in a browser
   - Download the PDF
   - Verify the number of pages matches the copy type
   - Verify each page has the correct label in the top-right corner

3. **Verify PDF Quality**:
   - All invoice data should be identical across all copies
   - Only the label (Original/Duplicate/etc.) should be different
   - Formatting, layout, and data should remain consistent

## Notes

- Each copy is a **separate page** in the same PDF file
- All copies contain the **same invoice data**
- Only the **label** in the top-right corner changes for each copy
- The batch print endpoint also respects this behavior for each invoice

## Status

✅ **COMPLETE** - Implementation finished and compiled successfully

