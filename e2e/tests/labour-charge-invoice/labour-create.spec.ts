/**
 * Labour Charge Invoice — Create / Lock / Print / Delete
 *
 * Parity:
 *   - Labour invoices are stored in INVOICE_MASTER with INM_TYPE='OutJWINM'.
 *     There is NO separate LABOUR_CHARGE_INVOICE_MASTER table.
 *   - Line items go into INVOICE_DETAIL (same as Tax Invoice, same table).
 *   - Labour invoices do NOT write to STOCK_LEDGER (service charges, not goods).
 *   - MODIFY column controls lock (BIT → JS boolean via mssql v11).
 *   - ES_DELETE for soft delete (BIT → boolean).
 *
 * API query param name: companyCode (NOT companyId — different from TaxInvoice!)
 * Response body field:  invoiceCode  (NOT id — LabourChargeInvoiceMasterDto.InvoiceCode)
 * Detail quantity field: invoiceQuantity (NOT quantity)
 *
 * Cross-module isolation: INM_TYPE='OutJWINM' must be asserted explicitly to prove
 * the row was not mis-created as a Tax Invoice (INM_TYPE='TAXINV').
 */

import { test, expect } from '../../fixtures';
import {
  getLatestLabourInvoice,
  getTaxInvoiceDetails,
  getStockEntriesForDoc,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const PO_CODE = 9001;

function labourPayload(companyId: number, overrides?: object) {
  return {
    companyCode: companyId,   // controller uses companyCode, not companyId
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    details: [
      {
        invoiceQuantity: 10,  // correct field in CreateLabourChargeInvoiceDetailRequest
        rate: 500,
        amount: 5000,
      },
    ],
    ...overrides,
  };
}

test.describe('Labour Charge Invoice — UI smoke', () => {

  test('list page loads and shows invoices', async ({ page, companyId, apiUrl, accessToken }) => {
    // Ensure at least one invoice exists
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 1, rate: 100, amount: 100 }],
    });
    expect(createResp.status).toBe(201);

    await page.goto('/transactions/labour-charge-invoice');
    await page.waitForLoadState('networkidle');

    // Grid must be visible
    const grid = page.locator('[class*="ag-root"], .ag-root-wrapper, [role="grid"]').first();
    await expect(grid).toBeVisible({ timeout: 8_000 });
  });

  test('create form reachable — page loads with expected form elements', async ({ page }) => {
    await page.goto('/transactions/labour-charge-invoice/create');

    // Form page must load with a Save/Submit button visible
    const saveBtn = page.locator('button[type="submit"]:has-text("Save"), button:has-text("Save")').first();
    await expect(saveBtn, 'Save button must be visible on create form').toBeVisible({ timeout: 8_000 });

    // Customer selection must be rendered (native select or searchable input)
    const customerSel = page.locator('select:has(option:has-text("Select Customer")), input[placeholder*="Customer"]').first();
    await expect(customerSel, 'Customer select must be visible').toBeVisible({ timeout: 5_000 });
  });
});

test.describe('Labour Charge Invoice — CRUD', () => {

  test('create → INVOICE_MASTER (INM_TYPE=OutJWINM) + INVOICE_DETAIL rows exist', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/LabourChargeInvoice', labourPayload(companyId)
    );

    expect(createResp.status, `Expected 201, got ${createResp.status}: ${JSON.stringify(createResp.data)}`).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;
    expect(invoiceCode, 'INVOICE_MASTER uses negative PKs from legacy identity seed — non-zero is valid').not.toBe(0);

    // INVOICE_MASTER — verify INM_TYPE is correct (cross-module isolation)
    const master = await getLatestLabourInvoice(db, companyId);
    expect(master, 'INVOICE_MASTER row must exist for Labour Invoice').not.toBeNull();
    expect(master!.INM_P_CODE).toBe(CUSTOMER_CODE);
    expect(master!.INM_TYPE).toBe('OutJWINM');  // must NOT be 'TAXINV' or null
    expect(master!.ES_DELETE).toBeFalsy();

    // Directly verify INM_TYPE on the specific row (deterministic)
    const typeRow = await db.queryOne<{ INM_TYPE: string }>(
      `SELECT INM_TYPE FROM INVOICE_MASTER WHERE INM_CODE = @code`,
      { code: invoiceCode }
    );
    expect(
      typeRow?.INM_TYPE,
      'INM_TYPE must be OutJWINM — proves Labour Invoice is stored separately from Tax Invoices'
    ).toBe('OutJWINM');

    // INVOICE_DETAIL — line items must be persisted (uses getTaxInvoiceDetails — same table)
    const details = await getTaxInvoiceDetails(db, invoiceCode);
    expect(details.length, 'INVOICE_DETAIL must have 1 row for labour invoice').toBe(1);
    expect(details[0].IND_INQTY).toBe(10);
    expect(details[0].ES_DELETE).toBeFalsy();
  });

  test('lock labour invoice → MODIFY=true in INVOICE_MASTER', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/LabourChargeInvoice', labourPayload(companyId, {
        details: [{ invoiceQuantity: 1, rate: 200, amount: 200 }],
      })
    );
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    const row = await db.queryOne<{ MODIFY: boolean | number }>(
      `SELECT MODIFY FROM INVOICE_MASTER WHERE INM_CODE = @code AND INM_TYPE = 'OutJWINM'`,
      { code: invoiceCode }
    );
    expect(row?.MODIFY).toBeTruthy();
  });

  test('print labour invoice → PDF blob returned', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/LabourChargeInvoice', labourPayload(companyId, {
        details: [{ invoiceQuantity: 2, rate: 500, amount: 1000 }],
      })
    );
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET', `/LabourChargeInvoice/${invoiceCode}/print?companyCode=${companyId}`
    );
    expect(printResp.status).toBe(200);
    const buffer = printResp.data as ArrayBuffer;
    expect(buffer.byteLength).toBeGreaterThan(1024);
    const magic = String.fromCharCode(...new Uint8Array(buffer.slice(0, 4)));
    expect(magic).toBe('%PDF');
  });

  test('soft delete → ES_DELETE=true in INVOICE_MASTER', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/LabourChargeInvoice', labourPayload(companyId, {
        details: [{ invoiceQuantity: 1, rate: 100, amount: 100 }],
      })
    );
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    const delResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/LabourChargeInvoice/${invoiceCode}?companyCode=${companyId}`, undefined
    );
    expect([200, 204]).toContain(delResp.status);

    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM INVOICE_MASTER WHERE INM_CODE = @code AND INM_TYPE = 'OutJWINM'`,
      { code: invoiceCode }
    );
    expect(row?.ES_DELETE).toBeTruthy();
  });

  test('labour invoice has NO stock impact — STOCK_LEDGER unchanged', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/LabourChargeInvoice', labourPayload(companyId, {
        details: [{ invoiceQuantity: 10, rate: 500, amount: 5000 }],
      })
    );
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    // Per-document stock check: labour invoices must create zero STOCK_LEDGER entries
    const stockEntry = await getStockEntriesForDoc(db, invoiceCode, 'TAXINV');
    expect(stockEntry, 'Labour Invoice must not create any STOCK_LEDGER entries with TAXINV type').toBe(0);

    const dcEntry = await getStockEntriesForDoc(db, invoiceCode, 'DCOUT');
    expect(dcEntry, 'Labour Invoice must not create any STOCK_LEDGER entries with DCOUT type').toBe(0);
  });
});

