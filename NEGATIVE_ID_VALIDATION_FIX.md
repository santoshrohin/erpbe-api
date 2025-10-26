# Negative Company ID Validation Fix

## Problem

The API was rejecting requests with **negative Company IDs**, returning:
```json
{
  "message": "Validation failed",
  "errors": {
    "CompanyId": [
      "Company ID is required."
    ]
  },
  "statusCode": 400
}
```

**Example failing request**:
```
GET /api/TaxInvoice?CompanyId=-2147483640&IsDeleted=false
```

## Root Cause

The validators were using `.GreaterThan(0)` which only accepts positive integers, but the database uses **negative integers** for Company IDs (e.g., `-2147483640`).

## Solution

Changed all validators from `.GreaterThan(0)` to `.NotEqual(0)` to accept both positive and negative Company IDs.

### Files Modified

1. **TaxInvoice Validators**:
   - `ErpBE.Application/TaxInvoice/Validators/GetAllTaxInvoicesQueryValidator.cs`
   - `ErpBE.Application/TaxInvoice/Validators/CreateTaxInvoiceCommandValidator.cs`
   - `ErpBE.Application/TaxInvoice/Validators/UpdateTaxInvoiceCommandValidator.cs`
   - `ErpBE.Application/TaxInvoice/Validators/DeleteTaxInvoiceCommandValidator.cs`
   - `ErpBE.Application/TaxInvoice/Validators/GetTaxInvoiceByIdQueryValidator.cs`

2. **CustomerPO Validators**:
   - `ErpBE.Application/CustomerPo/Validators/GetAllCustomerPosQueryValidator.cs`
   - `ErpBE.Application/CustomerPo/Validators/CreateCustomerPoCommandValidator.cs`
   - `ErpBE.Application/CustomerPo/Validators/UpdateCustomerPoCommandValidator.cs`
   - `ErpBE.Application/CustomerPo/Validators/DeleteCustomerPoCommandValidator.cs`
   - `ErpBE.Application/CustomerPo/Validators/GetCustomerPoByIdQueryValidator.cs`

### Before
```csharp
RuleFor(x => x.CompanyId)
    .GreaterThan(0)
    .WithMessage("Company ID is required.");
```

### After
```csharp
RuleFor(x => x.CompanyId)
    .NotEqual(0)
    .WithMessage("Company ID is required.");
```

## Next Steps

**You need to restart the API** for the changes to take effect:

1. **Stop the current API** (press Ctrl+C in the terminal where it's running)
2. **Restart the API**: `dotnet run --project ErpBE.API`

## Testing

After restarting, test with:

```bash
# TaxInvoice with negative CompanyId
curl "http://localhost:5136/api/TaxInvoice?CompanyId=-2147483640&IsDeleted=false" -H "Authorization: Bearer YOUR_TOKEN"

# CustomerPO with negative CompanyId
curl "http://localhost:5136/api/CustomerPo?CompanyId=-2147483640" -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected**: Both should work without validation errors! ✅

## Summary

- **Issue**: Validators rejected negative Company IDs
- **Fix**: Changed `.GreaterThan(0)` to `.NotEqual(0)` in 10 validators
- **Scope**: All TaxInvoice and CustomerPO endpoints
- **Status**: ✅ Code changes complete, **restart API to apply**

