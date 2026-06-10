# Legacy Parity Report — Sales Module

**Date:** 2026-06-03  
**Legacy Reference:** `/Users/kavitamhaske/Documents/LegacySunElectro/sunelectronicserp/EasiSimpleClickERP/Transactions/ADD/`

---

## Delivery Challan (Delivery_Challan.aspx)

| ID | Legacy Behavior | Frontend Status | Notes |
|---|---|---|---|
| DC-01 | `ddlCustomer.SelectedIndex == 0` blocks submit | ✅ Implemented | Shows error banner, `onSubmit` not called |
| DC-02 | `txtOrderQty > txtStock` blocks submit | ✅ Implemented | Stock fetched via batch dropdown; same-or-less passes |
| DC-03 | `txtOrderQty == "" \|\| == "0.00"` blocks submit | ✅ Implemented | Catches 0, negative, and empty; `noValidate` prevents HTML5 interference |
| DC-04 | ViewState DataTable duplicate ITEM_CODE check | ✅ Implemented | `indexOf` dedup on `itemCode` array |
| DC-05 | Lock/Unlock workflow | ⏳ Deferred | Needs backend endpoint investigation |
| DC-06 | Print challan PDF | ⏳ Deferred | Needs backend print SP investigation |

---

## Labour Charge Invoice (LabourChargeInvoice.aspx)

| ID | Legacy Behavior | Frontend Status | Notes |
|---|---|---|---|
| LCI-01 | `ddlInvoiceType` — 4 types: As Per BOM / One To One / Rework Inward / W/O Process Invoice | ✅ Implemented | Exact 4 options, defaults to 0 |
| LCI-02 | Auto-generated invoice number displayed in form | ✅ Implemented | Shows "Auto-assigned" for new; actual number when editing |
| LCI-03 | Stock validation (job-work outward — SP handles) | ✅ N/A | SP `ERP_CreateTaxInvoice` with `@Type='OutJWINM'` manages stock |
| LCI-04 | `chkSupplementary` — bypasses stock deduction | ✅ Implemented | Full stack: checkbox → payload → command → SP `@IsSupplementary` |
| LCI-05 | Print invoice PDF | ⏳ Deferred | Backend print SP availability to be confirmed |

---

## Tax Invoice (TaxInvoice.aspx)

| Feature | Status | Notes |
|---|---|---|
| CRUD (create/edit/delete) | ✅ Complete | Implemented in prior sprint |
| Lock/Unlock | ✅ Complete | Implemented in prior sprint |
| Print PDF | ✅ Complete | `ERP_GetTaxInvoicePrintData_V2_FIXED` SP + PDF generation |
| Stock management | ✅ Complete | `ERP_ManageTaxInvoiceStock` SP |

---

## Purchase Order (CustomerPO.aspx)

| Feature | Status | Notes |
|---|---|---|
| CRUD | ✅ Complete | Implemented in prior sprint |
| Print PDF | ✅ Complete | Implemented in prior sprint |
| Debug button removal | ✅ Complete (PO-01) | Removed this sprint |

---

## Known Remaining Gaps

1. **DC-05 Lock/Unlock** — Delivery Challan does not have lock/unlock UI. Need to verify if backend controller has these endpoints before implementing.
2. **DC-06 Print** — Delivery Challan print not yet implemented. Requires backend SP and controller endpoint.
3. **LCI-05 Print** — Labour Charge Invoice print not yet implemented. Requires backend print SP.
4. **LCI-03 Stock** — LCI stock validation is SP-side (legacy pattern), not client-side. This matches legacy intent.

---

## Overall Sales Module Parity

| Module | Parity |
|---|---|
| Tax Invoice | 100% — all features implemented and tested |
| Purchase Order | ~98% — debug cleanup done; print implemented |
| Delivery Challan | ~75% — core validations done; lock/print deferred |
| Labour Charge Invoice | ~80% — invoice type/supplementary done; print deferred |
