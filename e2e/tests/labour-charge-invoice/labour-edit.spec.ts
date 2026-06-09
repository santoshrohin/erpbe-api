/**
 * Labour Charge Invoice — Edit workflow
 *
 * Parity gaps covered:
 *   - INVOICE_DETAIL rows replaced on edit (old deleted, new inserted).
 *     Orphan rows in INVOICE_DETAIL would accumulate cost on every edit.
 *   - INM_TYPE must remain 'OutJWINM' after edit (not changed to 'TAXINV').
 *   - No stock impact on edit (labour = service, not goods).
 *   - Locked invoice cannot be edited.
 *
 * INVOICE_MASTER / INVOICE_DETAIL columns (same table as Tax Invoice):
 *   IND_INM_CODE = FK to INVOICE_MASTER
 *   IND_INQTY    = quantity
 *   IND_RATE     = rate
 *   IND_AMT      = amount
 *   ES_DELETE    = BIT (mssql returns boolean)
 *
 * API uses companyCode (NOT companyId).
 */

import { test, expect } from '../../fixtures';
import {
  getTaxInvoiceDetails,
  getStockEntriesForDoc,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const PO_CODE = 9001;

async function createLabourInvoice(
  apiUrl: string,
  accessToken: string,
  companyId: number,
  qty = 10,
  rate = 500
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
    companyCode: companyId,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    details: [{ invoiceQuantity: qty, rate, amount: qty * rate }],
  });
  if (resp.status !== 201) {
    throw new Error(`Labour invoice create failed: ${resp.status} — ${JSON.stringify(resp.data)}`);
  }
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Labour Charge Invoice — Edit', () => {

  test('edit labour invoice → INVOICE_DETAIL replaced with new quantity', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const originalQty = 10;
    const updatedQty = 6;

    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId, originalQty);

    // Verify original detail
    const detailsBefore = await getTaxInvoiceDetails(db, invoiceCode);
    expect(detailsBefore.length, 'Must have 1 INVOICE_DETAIL row after create').toBe(1);
    expect(detailsBefore[0].IND_INQTY).toBe(originalQty);

    // Edit — change quantity
    const updateResp = await apiRequest(
      apiUrl, accessToken, 'PUT', `/LabourChargeInvoice/${invoiceCode}`, {
        invoiceCode,
        companyCode: companyId,
        invoiceDate: new Date().toISOString(),
        customerCode: CUSTOMER_CODE,
        customerPoCode: PO_CODE,
        details: [{ invoiceQuantity: updatedQty, rate: 500, amount: updatedQty * 500 }],
      }
    );
    expect(
      [200, 204],
      `PUT /LabourChargeInvoice/${invoiceCode} failed with ${updateResp.status}: ${JSON.stringify(updateResp.data)}`
    ).toContain(updateResp.status);

    // INVOICE_DETAIL: exactly 1 row with new quantity
    const detailsAfter = await getTaxInvoiceDetails(db, invoiceCode);
    expect(detailsAfter.length, 'Must have exactly 1 INVOICE_DETAIL row (orphans removed)').toBe(1);
    expect(detailsAfter[0].IND_INQTY, 'IND_INQTY must reflect updated quantity').toBe(updatedQty);
  });

  test('edit adds second detail line → 2 INVOICE_DETAIL rows, no orphans', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    // Create with 1 line
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId, 5);

    // Edit — add second line
    const updateResp = await apiRequest(
      apiUrl, accessToken, 'PUT', `/LabourChargeInvoice/${invoiceCode}`, {
        invoiceCode,
        companyCode: companyId,
        invoiceDate: new Date().toISOString(),
        customerCode: CUSTOMER_CODE,
        customerPoCode: PO_CODE,
        details: [
          { invoiceQuantity: 5, rate: 500, amount: 2500 },
          { invoiceQuantity: 3, rate: 800, amount: 2400 },
        ],
      }
    );
    expect([200, 204]).toContain(updateResp.status);

    const detailsAfter = await getTaxInvoiceDetails(db, invoiceCode);
    expect(
      detailsAfter.length,
      'Must have exactly 2 INVOICE_DETAIL rows after adding second line'
    ).toBe(2);

    const qtys = detailsAfter.map(d => d.IND_INQTY).sort((a, b) => a - b);
    expect(qtys).toEqual([3, 5]);
  });

  test('edit preserves INM_TYPE=OutJWINM — does not flip to TAXINV', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId);

    await apiRequest(apiUrl, accessToken, 'PUT', `/LabourChargeInvoice/${invoiceCode}`, {
      invoiceCode,
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 8, rate: 600, amount: 4800 }],
    });

    const row = await db.queryOne<{ INM_TYPE: string }>(
      `SELECT INM_TYPE FROM INVOICE_MASTER WHERE INM_CODE = @code`,
      { code: invoiceCode }
    );
    expect(
      row?.INM_TYPE,
      'INM_TYPE must remain OutJWINM after edit — must not flip to TAXINV'
    ).toBe('OutJWINM');
  });

  test('edit has NO stock impact — STOCK_LEDGER unchanged', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId, 10);

    // Edit
    await apiRequest(apiUrl, accessToken, 'PUT', `/LabourChargeInvoice/${invoiceCode}`, {
      invoiceCode,
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 20, rate: 500, amount: 10000 }],
    });

    // Per-document check: no stock entries created for this invoice
    const taxInvEntry = await getStockEntriesForDoc(db, invoiceCode, 'TAXINV');
    expect(taxInvEntry, 'Labour invoice edit must not create STOCK_LEDGER entries').toBe(0);
  });

  test('edit locked labour invoice → API returns 4xx', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId, 5);

    // Lock
    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    // Attempt edit
    const updateResp = await apiRequest(
      apiUrl, accessToken, 'PUT', `/LabourChargeInvoice/${invoiceCode}`, {
        invoiceCode,
        companyCode: companyId,
        invoiceDate: new Date().toISOString(),
        customerCode: CUSTOMER_CODE,
        customerPoCode: PO_CODE,
        details: [{ invoiceQuantity: 99, rate: 999, amount: 99 * 999 }],
      }
    );
    expect(
      updateResp.status,
      'Edit of locked labour invoice must return 4xx'
    ).toBeGreaterThanOrEqual(400);
    expect(updateResp.status).toBeLessThan(500);

    // Quantity must remain unchanged
    const details = await getTaxInvoiceDetails(db, invoiceCode);
    expect(details[0].IND_INQTY, 'Locked invoice quantity must not be modified').toBe(5);
  });
});
