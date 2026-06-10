# Remaining Implementation Tasks — Sales Module Parity

**Date:** 2026-06-03  
**Baseline:** 83% overall parity  
**Target:** 100% parity  

---

## Task 1: LCI-03 — Stock Validation in Labour Charge Invoice

**Priority:** HIGH — blocks correct financial data integrity  
**Gap:** Frontend allows any invoice quantity without checking available stock. Legacy blocks submission when `invoiceQty > stockQty` (from `STOCK_LEDGER`) unless `isSupplementary == true`.

### What Must Be Built

#### 1a. New backend endpoint — Stock qty per item (from STOCK_LEDGER)

The legacy source uses a different stock source than DC-02. DC-02 reads `ITEM_MASTER.I_CURRENT_BAL`; LCI-03 reads `STOCK_LEDGER` summed by item:

```sql
SELECT isnull(sum(STL_DOC_QTY),0) as STL_DOC_QTY
FROM   STOCK_LEDGER
WHERE  STL_STORE_TYPE = -2147483648
AND    STL_I_CODE     = @ItemCode
```

Options:
- Add `itemLciStocks` key to the existing batch dropdown API (using a subquery or expression column)  
- Or create a dedicated `/api/LabourChargeInvoice/item-stock?itemCode=X&companyCode=Y` endpoint

#### 1b. Frontend — LabourChargeInvoiceForm.tsx

Add a `Stock` column to the items table (read-only, fetched when item is selected):
- Fetch stock qty from `STOCK_LEDGER` per item when `itemCode` changes
- Display stock qty in the row
- Validate on submit: `if (!isSupplementary && invoiceQty > stockQty)` → show error banner
- Validate on submit: `if (!isSupplementary && stockQty === 0)` → show "Please check Stock Qty"

**Validation rules (from legacy `btnInsert_Click` and `txtVQty_TextChanged`):**

```
When isSupplementary == false:
  1. If invoiceQty == 0 → "Enter Invoice Qty"
  2. If stockQty == 0 or empty → "Please check Stock Qty"
  3. If invoiceQty > stockQty → "Please Enter Invoice Qty Less than stock"
  
When isSupplementary == true:
  → Skip all stock validation (allow any qty)
```

#### 1c. Tests Required

- LCI-03: blocks when `invoiceQty > stockQty` and `isSupplementary = false`
- LCI-03: allows when `isSupplementary = true` even if `invoiceQty > stockQty`
- LCI-03: blocks when `stockQty = 0` and `isSupplementary = false`
- LCI-03: shows stock qty in item row

---

## Task 2: LCI-05 — Labour Charge Invoice Print

**Priority:** HIGH — operational requirement  
**Gap:** No print functionality exists at any layer.

### What Must Be Built

#### 2a. Database SP — `ERP_GetLabourChargeInvoicePrintData`

Query should join:
- `INVOICE_MASTER` — header (invoice number, date, type, customer PO)
- `INVOICE_DETAIL` — line items (item code, qty, rate, GST %)
- `PARTY_MASTER` — customer name, address, GSTIN
- `ITEM_MASTER` — item name, HSN code
- `COMPANY_MASTER` — company name, address, GSTIN, phone

Reference: Legacy `LabourChargeInvoicePrint.aspx.cs` uses inline SQL against these same tables.

#### 2b. Backend — Query Handler + Controller Endpoint

Follow the Tax Invoice print pattern exactly:
- `GetLabourChargeInvoicePrintDataQuery` → handler → repository calls SP
- `LabourChargeInvoicePrintDto` DTO
- `IPdfService.GenerateLabourChargeInvoicePdf(dto)` in `PdfService`
- `GET /api/LabourChargeInvoice/{id}/print?companyCode=X` endpoint

PDF layout should match Tax Invoice PDF (same company letterhead, GST fields, line items table).

#### 2c. Frontend

- Add `print` method to `src/api/labour-charge-invoice.api.ts`
- Add print button to `LabourChargeInvoiceList.tsx` column actions  
- Call `window.open(pdfUrl)` or trigger PDF download (match Tax Invoice pattern)

---

## Task 3: DC-05 — Delivery Challan Lock/Unlock

**Priority:** MEDIUM — concurrency safety  
**Gap:** No lock/unlock at any layer for DC.

### What Must Be Built

#### 3a. Database SPs

**`ERP_LockDeliveryChallan.sql`:**
```sql
CREATE OR ALTER PROCEDURE ERP_LockDeliveryChallan
    @ChallanCode INT, @CompanyCode INT
AS
BEGIN
    UPDATE DELIVERY_CHALLAN_MASTER
    SET    MODIFY = 1
    WHERE  DCM_CODE    = @ChallanCode
      AND  DCM_CM_CODE = @CompanyCode
      AND  ES_DELETE   = 0
      AND  MODIFY      = 0;
    SELECT @@ROWCOUNT AS RowsAffected;
END
```

**`ERP_UnlockDeliveryChallan.sql`:**
```sql
CREATE OR ALTER PROCEDURE ERP_UnlockDeliveryChallan
    @ChallanCode INT, @CompanyCode INT
AS
BEGIN
    UPDATE DELIVERY_CHALLAN_MASTER
    SET    MODIFY = 0
    WHERE  DCM_CODE    = @ChallanCode
      AND  DCM_CM_CODE = @CompanyCode
      AND  ES_DELETE   = 0
      AND  MODIFY      = 1;
    SELECT @@ROWCOUNT AS RowsAffected;
END
```

#### 3b. Backend — Commands + Handlers + Repository + Controller

Follow LCI lock pattern exactly:
- `LockDeliveryChallanCommand` / `UnlockDeliveryChallanCommand` (MediatR)
- `LockDeliveryChallanCommandHandler` / `UnlockDeliveryChallanCommandHandler`
- `IDeliveryChallanRepository.LockAsync(int code, int companyCode)` + `UnlockAsync`
- Add to `DeliveryChallanController.cs`:
  ```csharp
  [HttpPost("{id}/lock")]
  [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
  public async Task<IActionResult> Lock(int id, [FromQuery] int companyCode) { ... }

  [HttpPost("{id}/unlock")]
  [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
  public async Task<IActionResult> Unlock(int id, [FromQuery] int companyCode) { ... }
  ```

#### 3c. Frontend

- Add `lock` and `unlock` methods to `src/api/delivery-challan.api.ts`
- Add `useLockDeliveryChallan` / `useUnlockDeliveryChallan` hooks in `useDeliveryChallanQueries.ts`
- Add `handleToggleLock` handler in `DeliveryChallanList.tsx` (match `LabourChargeInvoiceList.tsx` pattern exactly)
- Add lock action column to DC columns config

---

## Task 4: DC-06 — Delivery Challan Print

**Priority:** MEDIUM — operational requirement  
**Gap:** No print functionality exists at any layer.

### What Must Be Built

#### 4a. Database SP — `ERP_GetDeliveryChallanPrintData`

Based on legacy `GenerateReport()` in `DeliveryChallan.aspx.cs`:

```sql
-- For type 'DLC' (standard):
SELECT DCM_NO, DCM_DATE, DCM_VEH_NO,
       CASE WHEN DCM_IS_RETURNABLE = 1 THEN 'Returnable' ELSE 'NonReturnable' END,
       DCM_ORDER_NO, DCD_ORD_QTY, I_NAME, P_NAME, P_ADD1, I_CODENO, DCD_REMARK, P_LBT_NO
FROM   DELIVERY_CHALLAN_MASTER
JOIN   DELIVERY_CHALLAN_DETAIL ON DCM_CODE = DCD_DCM_CODE
JOIN   PARTY_MASTER            ON P_CODE   = DCM_P_CODE
JOIN   ITEM_MASTER             ON I_CODE   = DCD_I_CODE
WHERE  DCM_CODE = @ChallanCode
```

Also join `COMPANY_MASTER` for header info.

#### 4b. Backend — Query Handler + Controller Endpoint

Follow Tax Invoice print pattern:
- `GetDeliveryChallanPrintDataQuery` → handler → repository
- `DeliveryChallanPrintDto` DTO
- `IPdfService.GenerateDeliveryChallanPdf(dto)` method
- `GET /api/DeliveryChallan/{id}/print?companyCode=X` endpoint

#### 4c. Frontend

- Add `print` method to `src/api/delivery-challan.api.ts`
- Add print action to `DeliveryChallanList.tsx` column config

---

## Effort Estimate

| Task | Complexity | Est. Effort |
|---|---|---|
| LCI-03 Stock Validation (frontend only if batch dropdown is extended) | Medium | 4-6 hours |
| LCI-05 Print | High (full stack) | 8-12 hours |
| DC-05 Lock/Unlock | Medium (full stack, but follows LCI pattern) | 4-6 hours |
| DC-06 Print | High (full stack) | 6-10 hours |

**Total estimated effort: 22-34 hours**

---

## Parity Impact

| Task | Module Before | Module After | Overall Change |
|---|---|---|---|
| LCI-03 Stock Validation | 62.5% | 75% | +3.1% overall |
| LCI-05 Print | 75% | 87.5% | +3.1% overall |
| DC-05 Lock/Unlock | 71% | 85.7% | +2.9% overall |
| DC-06 Print | 85.7% | 100% | +2.9% overall |

**Overall: 83% → 100%** when all 4 tasks are complete.
