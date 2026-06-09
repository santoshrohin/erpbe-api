/**
 * Tax Invoice — Create workflow
 *
 * Critical parity areas:
 *   - Invoice creates INVOICE_DETAIL rows and STOCK_LEDGER entries.
 *   - lrDate: "" was causing 400 (DateTime? binding rejects empty string).
 *   - CGST stored as E_BASIC_CentralT, SGST as E_EDU_CESS_State (legacy column names).
 *   - INM_TYPE='TAXINV' distinguishes from Labour Invoices (INM_TYPE='OutJWINM').
 *
 * Test assertions (per test, not "get latest" — use ID from response for determinism):
 *   1. INVOICE_MASTER row by invoiceCode
 *   2. INVOICE_DETAIL rows: count, item, quantity, CGST
 *   3. STOCK_LEDGER: balance decreases by invoice quantity
 *   4. INM_TYPE is 'TAXINV' (no cross-contamination with Labour)
 */

import { test, expect } from '../../fixtures';
import {
  getTaxInvoiceByCode,
  getTaxInvoiceDetails,
  getStockEntriesForDoc,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const COMPANY_CODE = 1;
const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const STORE_CODE = 1;
const UOM_CODE = 1;
const PO_CODE = 9001;

function invoicePayload(overrides?: object) {
  return {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    invoiceType: 1,
    netAmount: 5000,
    grossAmount: 5900,
    taxableAmount: 5000,
    invoiceDetails: [
      {
        itemCode: ITEM_CODE,
        uomCode: UOM_CODE,
        invoiceQuantity: 5,
        rate: 1000,
        storeCode: STORE_CODE,
        cgstPercentage: 9,
        sgstPercentage: 9,
        hsnCode: '8536',
      },
    ],
    ...overrides,
  };
}

test.describe('Tax Invoice — Create', () => {

  test('creates invoice → INVOICE_MASTER + INVOICE_DETAIL + stock deducted', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceQty = 5;

    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/TaxInvoice', invoicePayload()
    );

    expect(createResp.status, `Expected 201, got ${createResp.status}: ${JSON.stringify(createResp.data)}`).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;
    expect(invoiceCode, 'INVOICE_MASTER uses negative PKs from legacy identity seed — non-zero is valid').not.toBe(0);

    // INVOICE_MASTER — look up by ID (deterministic, no "get latest" race)
    const master = await getTaxInvoiceByCode(db, invoiceCode);
    expect(master, 'INVOICE_MASTER row must exist').not.toBeNull();
    expect(master!.INM_P_CODE).toBe(CUSTOMER_CODE);
    expect(master!.INM_CM_CODE).toBe(COMPANY_CODE);
    expect(master!.INM_TYPE).toBe('TAXINV');  // must NOT be 'OutJWINM' (Labour) or null
    expect(master!.ES_DELETE).toBeFalsy();

    // INVOICE_DETAIL — verify line item was inserted with correct values
    const details = await getTaxInvoiceDetails(db, invoiceCode);
    expect(details.length, 'INVOICE_DETAIL must have 1 row').toBe(1);
    expect(details[0].IND_I_CODE).toBe(ITEM_CODE);
    expect(details[0].IND_INQTY).toBe(invoiceQty);
    expect(details[0].E_BASIC_CentralT).toBe(9);   // CGST % stored in E_BASIC_CentralT column
    expect(details[0].E_EDU_CESS_State).toBe(9);   // SGST % stored in E_EDU_CESS_State column
    expect(details[0].ES_DELETE).toBeFalsy();

    // STOCK_LEDGER — per-document assertion: this specific invoice deducted stock
    // Using per-document query avoids test pollution from concurrent runs on shared item 9001
    const stockEntry = await getStockEntriesForDoc(db, invoiceCode, 'TAXINV');
    expect(stockEntry, 'STOCK_LEDGER entry for this invoice must be negative (deduction)').toBeLessThan(0);
    expect(Math.abs(stockEntry), `Deduction must equal invoiceQty (${invoiceQty})`).toBe(invoiceQty);
  });

  test('create with 2 line items → 2 INVOICE_DETAIL rows, stock deducted for both', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const qty1 = 3, qty2 = 2;
    const ITEM_CODE_2 = 9002;

    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [
        { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: qty1, rate: 1000, storeCode: STORE_CODE, cgstPercentage: 9, sgstPercentage: 9 },
        { itemCode: ITEM_CODE_2, uomCode: UOM_CODE, invoiceQuantity: qty2, rate: 500, storeCode: STORE_CODE, cgstPercentage: 9, sgstPercentage: 9 },
      ],
    });
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    const details = await getTaxInvoiceDetails(db, invoiceCode);
    expect(details.length, 'Must have 2 INVOICE_DETAIL rows for 2 line items').toBe(2);

    const itemCodes = details.map(d => d.IND_I_CODE).sort();
    expect(itemCodes).toEqual([ITEM_CODE, ITEM_CODE_2].sort());

    // Per-document stock assertions — not susceptible to concurrent test pollution
    const stockEntry = await getStockEntriesForDoc(db, invoiceCode, 'TAXINV');
    expect(stockEntry, 'Invoice must deduct stock (negative entry)').toBeLessThan(0);
    expect(Math.abs(stockEntry), 'Total deduction must equal qty1 + qty2').toBe(qty1 + qty2);
  });

  test('create invoice with lrDate=null → 201 (null is the correct sanitized form of empty date)', async ({
    apiUrl, accessToken,
  }) => {
    // The frontend sanitizeTaxInvoiceRequest converts "" → null before sending.
    // This test verifies the API accepts null (the post-sanitization value).
    // The invoice-negative spec separately verifies the backend rejects "" directly.
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      lrDate: null,
      invoiceDetails: [
        { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 500, storeCode: STORE_CODE },
      ],
    });
    expect(
      createResp.status,
      `lrDate=null must be accepted (201). Got: ${createResp.status} — ${JSON.stringify(createResp.data)}`
    ).toBe(201);
  });

  test('invoice list shows new invoice with correct customer name', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/TaxInvoice', invoicePayload()
    );
    expect(createResp.status).toBe(201);

    await page.goto('/transactions/tax-invoice');
    await page.waitForLoadState('networkidle');

    // AG Grid renders cells as div[col-id="..."], not <td>
    const customerCell = page
      .locator('[col-id="customerName"]:has-text("E2E"), [role="gridcell"]:has-text("E2E Test Customer")')
      .first();
    await expect(customerCell).toBeVisible({ timeout: 8_000 });
  });
});
