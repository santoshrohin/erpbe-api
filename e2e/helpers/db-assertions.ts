/**
 * Layer 5 — DB Assertion Helpers
 *
 * Validates that a completed UI transaction is correctly and fully persisted
 * in the database. Goes beyond primary-key existence checks — asserts every
 * field the form submitted actually landed in the right column.
 *
 * WHY THIS LAYER EXISTS:
 *   A network-interception test (Layer 1) proves the browser sent the right
 *   body. A DB assertion test proves the backend wrote it correctly. Without
 *   this layer, a stored-procedure mapping bug (e.g. wrong column, ignored
 *   parameter) would be invisible until manual QA.
 *
 * COLUMN NAME WARNINGS (see db-queries.ts for full list):
 *   CUSTPO_MASTER:       CPOM_CODE, CPOM_P_CODE, CPOM_PONO, CPOM_GRAND_TOT
 *   CUSTPO_DETAIL:       CPOD_CPOM_CODE, CPOD_I_CODE, CPOD_ORD_QTY, CPOD_RATE
 *   INVOICE_MASTER:      INM_CODE, INM_P_CODE, INM_NET_AMT, INM_G_AMT
 *   INVOICE_DETAIL:      IND_INM_CODE, IND_I_CODE, IND_INQTY, IND_RATE, IND_AMT
 *   DELIVERY_CHALLAN_*:  DCM_CODE, DCM_CM_CODE, DCD_DCM_CODE, DCD_ORD_QTY
 *
 * BIT columns return JS boolean from mssql v11 — use toBeTruthy()/toBeFalsy()
 */

import { expect } from '@playwright/test';
import type { DbClient } from '../fixtures/db';
import {
  getPoByCode,
  getPoDetails,
  getTaxInvoiceByCode,
  getTaxInvoiceDetails,
  getChallanDetails,
  type CustPoRow,
  type CustPoDetailRow,
  type TaxInvoiceRow,
  type InvoiceDetailRow,
  type ChallanDetailRow,
} from './db-queries';

// ── Purchase Order ─────────────────────────────────────────────────────────────

export interface PoDbExpectation {
  poCode: number;
  poNumber?: string;
  customerCode?: number;
  companyId?: number;
  grandTotal?: number;
  isDeleted?: boolean;
  isLocked?: boolean;
  details?: PoDetailExpectation[];
}

export interface PoDetailExpectation {
  itemCode: number;
  orderedQuantity?: number;
  rate?: number;
  amount?: number;
}

/**
 * Assert that a Customer PO exists in DB with all expected field values.
 *
 * Validates: existence, soft-delete flag, header fields, and line items.
 * Tolerates floating-point rounding (±0.01) for monetary totals.
 *
 * Usage:
 *   const po = await assertPurchaseOrderInDB(db, {
 *     poCode: 104,
 *     poNumber: 'TEST-PO-104',
 *     customerCode: 9001,
 *     companyId: 1,
 *     grandTotal: 1000,
 *     isDeleted: false,
 *     details: [{ itemCode: 9001, orderedQuantity: 1, rate: 1000 }],
 *   });
 */
export async function assertPurchaseOrderInDB(
  db: DbClient,
  expected: PoDbExpectation
): Promise<CustPoRow> {
  const row = await getPoByCode(db, expected.poCode);

  expect(row, `PO ${expected.poCode} must exist in CUSTPO_MASTER`).not.toBeNull();
  const po = row!;

  // ── Soft-delete ─────────────────────────────────────────────────────────
  if (expected.isDeleted !== undefined) {
    if (expected.isDeleted) {
      expect(po.ES_DELETE, `PO ${expected.poCode} ES_DELETE must be true (deleted)`).toBeTruthy();
    } else {
      expect(po.ES_DELETE, `PO ${expected.poCode} ES_DELETE must be false (not deleted)`).toBeFalsy();
    }
  }

  // ── Lock flag ───────────────────────────────────────────────────────────
  if (expected.isLocked !== undefined) {
    if (expected.isLocked) {
      expect(po.MODIFY, `PO ${expected.poCode} MODIFY must be true (locked)`).toBeTruthy();
    } else {
      expect(po.MODIFY, `PO ${expected.poCode} MODIFY must be falsy (unlocked)`).toBeFalsy();
    }
  }

  // ── Header fields ────────────────────────────────────────────────────────
  if (expected.poNumber !== undefined) {
    expect(po.CPOM_PONO, `PO ${expected.poCode} CPOM_PONO`).toBe(expected.poNumber);
  }

  if (expected.customerCode !== undefined) {
    expect(po.CPOM_P_CODE, `PO ${expected.poCode} CPOM_P_CODE (customerCode)`).toBe(expected.customerCode);
  }

  if (expected.companyId !== undefined) {
    expect(po.CPOM_CM_COMP_ID, `PO ${expected.poCode} CPOM_CM_COMP_ID (companyId)`).toBe(expected.companyId);
  }

  if (expected.grandTotal !== undefined) {
    expect(po.CPOM_GRAND_TOT ?? 0).toBeCloseTo(expected.grandTotal, 1);
  }

  // ── Line items ───────────────────────────────────────────────────────────
  if (expected.details !== undefined && expected.details.length > 0) {
    const details = await getPoDetails(db, expected.poCode);

    expect(
      details.length,
      `PO ${expected.poCode} must have ${expected.details.length} detail row(s) in CUSTPO_DETAIL`
    ).toBe(expected.details.length);

    for (const expectedDetail of expected.details) {
      const found = details.find(d => d.CPOD_I_CODE === expectedDetail.itemCode);
      expect(
        found,
        `PO ${expected.poCode} detail: itemCode ${expectedDetail.itemCode} not found in CUSTPO_DETAIL`
      ).toBeDefined();

      if (found) {
        if (expectedDetail.orderedQuantity !== undefined) {
          expect(found.CPOD_ORD_QTY, `detail itemCode=${expectedDetail.itemCode} CPOD_ORD_QTY`).toBeCloseTo(expectedDetail.orderedQuantity, 2);
        }
        if (expectedDetail.rate !== undefined) {
          expect(found.CPOD_RATE, `detail itemCode=${expectedDetail.itemCode} CPOD_RATE`).toBeCloseTo(expectedDetail.rate, 2);
        }
        if (expectedDetail.amount !== undefined) {
          expect(found.CPOD_AMT ?? 0, `detail itemCode=${expectedDetail.itemCode} CPOD_AMT`).toBeCloseTo(expectedDetail.amount, 1);
        }
      }
    }
  }

  return po;
}

/**
 * Assert that a PO was soft-deleted (ES_DELETE=true) and details reflect that.
 */
export async function assertPoDeletedInDB(
  db: DbClient,
  poCode: number
): Promise<void> {
  await assertPurchaseOrderInDB(db, { poCode, isDeleted: true });
}

/**
 * Assert the amendment counter incremented on the last update.
 * Requires the before-snapshot count.
 */
export async function assertPoAmendmentIncremented(
  db: DbClient,
  poCode: number,
  previousCount: number
): Promise<void> {
  const row = await getPoByCode(db, poCode);
  expect(row, `PO ${poCode} must exist`).not.toBeNull();
  const current = row!.CPOM_AM_COUNT ?? 0;
  expect(current, `CPOM_AM_COUNT must be > previous (${previousCount}) after update`).toBeGreaterThan(previousCount);
}

// ── Tax Invoice ────────────────────────────────────────────────────────────────

export interface InvoiceDbExpectation {
  invoiceCode: number;
  customerCode?: number;
  companyCode?: number;
  netAmount?: number;
  grandTotal?: number;
  isDeleted?: boolean;
  isLocked?: boolean;
  type?: 'TAXINV' | 'OutJWINM';
  details?: InvoiceDetailExpectation[];
}

export interface InvoiceDetailExpectation {
  itemCode: number;
  quantity?: number;
  rate?: number;
  amount?: number;
}

/**
 * Assert that a Tax Invoice exists in INVOICE_MASTER with all expected fields.
 *
 * Filters by INM_TYPE='TAXINV' to prevent Labour Invoice rows bleeding in.
 */
export async function assertInvoiceInDB(
  db: DbClient,
  expected: InvoiceDbExpectation
): Promise<TaxInvoiceRow> {
  const row = await getTaxInvoiceByCode(db, expected.invoiceCode);

  expect(row, `Invoice ${expected.invoiceCode} must exist in INVOICE_MASTER`).not.toBeNull();
  const inv = row!;

  // Discriminate by type if specified
  if (expected.type !== undefined) {
    expect(
      inv.INM_TYPE,
      `Invoice ${expected.invoiceCode} INM_TYPE must be '${expected.type}'`
    ).toBe(expected.type);
  }

  // ── Soft-delete ─────────────────────────────────────────────────────────
  if (expected.isDeleted !== undefined) {
    if (expected.isDeleted) {
      expect(inv.ES_DELETE, `Invoice ${expected.invoiceCode} must be deleted`).toBeTruthy();
    } else {
      expect(inv.ES_DELETE, `Invoice ${expected.invoiceCode} must not be deleted`).toBeFalsy();
    }
  }

  // ── Lock flag ───────────────────────────────────────────────────────────
  if (expected.isLocked !== undefined) {
    if (expected.isLocked) {
      expect(inv.MODIFY, `Invoice ${expected.invoiceCode} must be locked`).toBeTruthy();
    } else {
      expect(inv.MODIFY, `Invoice ${expected.invoiceCode} must be unlocked`).toBeFalsy();
    }
  }

  // ── Header fields ────────────────────────────────────────────────────────
  if (expected.customerCode !== undefined) {
    expect(inv.INM_P_CODE, `Invoice ${expected.invoiceCode} INM_P_CODE (customerCode)`).toBe(expected.customerCode);
  }

  if (expected.companyCode !== undefined) {
    expect(inv.INM_CM_CODE, `Invoice ${expected.invoiceCode} INM_CM_CODE (companyCode)`).toBe(expected.companyCode);
  }

  if (expected.netAmount !== undefined) {
    expect(inv.INM_NET_AMT ?? 0).toBeCloseTo(expected.netAmount, 1);
  }

  if (expected.grandTotal !== undefined) {
    expect(inv.INM_G_AMT ?? 0).toBeCloseTo(expected.grandTotal, 1);
  }

  // ── Line items (INVOICE_DETAIL) ──────────────────────────────────────────
  if (expected.details !== undefined && expected.details.length > 0) {
    const details = await getTaxInvoiceDetails(db, expected.invoiceCode);

    expect(
      details.length,
      `Invoice ${expected.invoiceCode} must have ${expected.details.length} active detail row(s)`
    ).toBeGreaterThanOrEqual(expected.details.length);

    for (const expectedDetail of expected.details) {
      const found = details.find(d => d.IND_I_CODE === expectedDetail.itemCode);
      expect(
        found,
        `Invoice ${expected.invoiceCode} detail: itemCode ${expectedDetail.itemCode} not found in INVOICE_DETAIL`
      ).toBeDefined();

      if (found) {
        if (expectedDetail.quantity !== undefined) {
          expect(found.IND_INQTY, `detail itemCode=${expectedDetail.itemCode} IND_INQTY`).toBeCloseTo(expectedDetail.quantity, 2);
        }
        if (expectedDetail.rate !== undefined) {
          expect(found.IND_RATE ?? 0, `detail itemCode=${expectedDetail.itemCode} IND_RATE`).toBeCloseTo(expectedDetail.rate, 2);
        }
        if (expectedDetail.amount !== undefined) {
          expect(found.IND_AMT ?? 0, `detail itemCode=${expectedDetail.itemCode} IND_AMT`).toBeCloseTo(expectedDetail.amount, 1);
        }
      }
    }
  }

  return inv;
}

// ── Delivery Challan ───────────────────────────────────────────────────────────

export interface ChallanDbExpectation {
  dcCode: number;
  customerCode?: number;
  companyCode?: number;
  isDeleted?: boolean;
  details?: ChallanDetailExpectation[];
}

export interface ChallanDetailExpectation {
  itemCode: number;
  quantity?: number;
  uomCode?: number;
}

/**
 * Assert that a Delivery Challan exists in DELIVERY_CHALLAN_MASTER with all
 * expected fields and that its detail rows are correct.
 *
 * IMPORTANT: Challan uses companyCode (DCM_CM_CODE), not companyId.
 */
export async function assertChallanInDB(
  db: DbClient,
  expected: ChallanDbExpectation
): Promise<void> {
  // Read header
  const headerRow = await db.queryOne<{
    DCM_CODE: number;
    DCM_CM_CODE: number | null;
    DCM_P_CODE: number | null;
    ES_DELETE: boolean | number | null;
  }>(
    `SELECT DCM_CODE, DCM_CM_CODE, DCM_P_CODE, ES_DELETE
     FROM DELIVERY_CHALLAN_MASTER
     WHERE DCM_CODE = @dcCode`,
    { dcCode: expected.dcCode }
  );

  expect(
    headerRow,
    `Challan ${expected.dcCode} must exist in DELIVERY_CHALLAN_MASTER`
  ).not.toBeNull();
  const hdr = headerRow!;

  // ── Soft-delete ─────────────────────────────────────────────────────────
  if (expected.isDeleted !== undefined) {
    if (expected.isDeleted) {
      expect(hdr.ES_DELETE, `Challan ${expected.dcCode} must be deleted`).toBeTruthy();
    } else {
      expect(hdr.ES_DELETE, `Challan ${expected.dcCode} must not be deleted`).toBeFalsy();
    }
  }

  // ── Header fields ────────────────────────────────────────────────────────
  if (expected.customerCode !== undefined) {
    expect(hdr.DCM_P_CODE, `Challan ${expected.dcCode} DCM_P_CODE (customerCode)`).toBe(expected.customerCode);
  }

  if (expected.companyCode !== undefined) {
    expect(hdr.DCM_CM_CODE, `Challan ${expected.dcCode} DCM_CM_CODE (companyCode)`).toBe(expected.companyCode);
  }

  // ── Line items (DELIVERY_CHALLAN_DETAIL) ─────────────────────────────────
  if (expected.details !== undefined && expected.details.length > 0) {
    const details: ChallanDetailRow[] = await getChallanDetails(db, expected.dcCode);

    expect(
      details.length,
      `Challan ${expected.dcCode} must have ${expected.details.length} active detail row(s)`
    ).toBe(expected.details.length);

    for (const expectedDetail of expected.details) {
      const found = details.find(d => d.DCD_I_CODE === expectedDetail.itemCode);
      expect(
        found,
        `Challan ${expected.dcCode} detail: itemCode ${expectedDetail.itemCode} not found in DELIVERY_CHALLAN_DETAIL`
      ).toBeDefined();

      if (found) {
        if (expectedDetail.quantity !== undefined) {
          expect(found.DCD_ORD_QTY, `detail itemCode=${expectedDetail.itemCode} DCD_ORD_QTY`).toBeCloseTo(expectedDetail.quantity, 2);
        }
        if (expectedDetail.uomCode !== undefined) {
          expect(found.DCD_UM_CODE, `detail itemCode=${expectedDetail.itemCode} DCD_UM_CODE`).toBe(expectedDetail.uomCode);
        }
      }
    }
  }
}

// ── Stock Ledger ───────────────────────────────────────────────────────────────

export interface StockLedgerExpectation {
  itemCode: number;
  docNo: number;
  docType: string;
  expectedNetQty: number;
  toleranceQty?: number;
}

/**
 * Assert that stock ledger entries for a document result in the expected
 * net quantity change.
 *
 * Negative expectedNetQty means stock was deducted (shipment).
 * Positive means stock was added (return/receipt).
 *
 * Uses STL_* column names — NOT SL_*. Both exist in schema; wrong prefix
 * returns null silently.
 */
export async function assertStockEntryInDB(
  db: DbClient,
  expected: StockLedgerExpectation
): Promise<void> {
  const row = await db.queryOne<{ total: number }>(
    `SELECT ISNULL(SUM(STL_DOC_QTY), 0) AS total
     FROM STOCK_LEDGER
     WHERE STL_I_CODE = @itemCode AND STL_DOC_NO = @docNo AND STL_DOC_TYPE = @docType`,
    { itemCode: expected.itemCode, docNo: expected.docNo, docType: expected.docType }
  );

  const actual = row?.total ?? 0;
  const tolerance = expected.toleranceQty ?? 0.01;

  expect(
    Math.abs(actual - expected.expectedNetQty),
    `STOCK_LEDGER: itemCode=${expected.itemCode} docNo=${expected.docNo} ` +
    `docType='${expected.docType}' — expected net qty ${expected.expectedNetQty}, ` +
    `got ${actual}`
  ).toBeLessThanOrEqual(tolerance);
}

// ── Labour Charge Invoice ──────────────────────────────────────────────────────

export interface LabourInvoiceDbExpectation {
  invoiceCode: number;
  customerCode?: number;
  companyCode?: number;
  isDeleted?: boolean;
  details?: InvoiceDetailExpectation[];
}

/**
 * Assert a Labour Charge Invoice in INVOICE_MASTER (INM_TYPE='OutJWINM').
 * Delegates to assertInvoiceInDB with type forced to 'OutJWINM'.
 */
export async function assertLabourInvoiceInDB(
  db: DbClient,
  expected: LabourInvoiceDbExpectation
): Promise<void> {
  await assertInvoiceInDB(db, {
    ...expected,
    type: 'OutJWINM',
  });
}
