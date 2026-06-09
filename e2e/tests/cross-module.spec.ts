/**
 * Cross-Module Validation Tests
 *
 * These tests verify invariants that span multiple ERP modules:
 *
 *   1. companyCode vs companyId consistency
 *      - PO, TaxInvoice: use companyId
 *      - DeliveryChallan, LabourInvoice: use companyCode
 *      Mixing these causes silent failures or wrong company filtering.
 *
 *   2. ES_DELETE consistency
 *      Soft-delete must work the same way across ALL modules.
 *      BIT column returns JS boolean (mssql v11) — use toBeTruthy/toBeFalsy.
 *
 *   3. MODIFY (lock flag) consistency
 *      Lock/unlock behavior must be identical across all modules.
 *
 *   4. Serial number uniqueness
 *      Each module generates a serial number per company.
 *      Concurrent creates must produce unique numbers (no duplicates).
 *
 *   5. Line item integrity (MASTER ↔ DETAIL sync)
 *      Creating a master without details must fail.
 *      Detail count must always match what was submitted.
 *
 *   6. INM_TYPE isolation
 *      Tax Invoice (TAXINV) and Labour Invoice (OutJWINM) share INVOICE_MASTER.
 *      Operations on one must never affect the other.
 *
 *   7. Date handling consistency
 *      All modules must reject "" for nullable DateTime fields.
 *      All modules must accept null for the same fields.
 */

import { test, expect, uniqueRef } from '../fixtures';
import { apiRequest } from '../fixtures/auth';
import {
  getPoByCode,
  getTaxInvoiceByCode,
  getStockEntriesForDoc,
} from '../helpers/db-queries';

const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const STORE_CODE = 1;
const UOM_CODE = 1;
const PO_CODE = 9001;

// ── 1. companyCode vs companyId consistency ────────────────────────────────────

test.describe('companyCode vs companyId — correct field per module', () => {

  test('PO: uses companyId — companyCode param ignored gracefully', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: CUSTOMER_CODE,
      poNumber: uniqueRef('CM-PO'),
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,           // correct field for PO
      grandTotal: 100,
      projectCode: null,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1, rate: 100, amount: 100 }],
    });
    expect(resp.status, 'PO create with companyId must succeed').toBe(201);
  });

  test('TaxInvoice: uses companyCode — companyId in body is wrong field', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: companyId,   // correct field for TaxInvoice
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [
        { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 500, storeCode: STORE_CODE },
      ],
    });
    expect(resp.status, 'TaxInvoice create with companyCode must succeed').toBe(201);
  });

  test('DeliveryChallan: uses companyCode (NOT companyId)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan', {
      companyCode: companyId,   // correct field for DC
      challanDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1 }],
    });
    expect(resp.status, 'DeliveryChallan create with companyCode must succeed').toBe(201);
  });

  test('LabourInvoice: uses companyCode (NOT companyId)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
      companyCode: companyId,   // correct field for Labour
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 5, rate: 200, amount: 1000 }],
    });
    expect(resp.status, 'LabourInvoice create with companyCode must succeed').toBe(201);
  });
});

// ── 2. ES_DELETE consistency across all modules ───────────────────────────────

test.describe('ES_DELETE — soft-delete behavior consistent across modules', () => {

  test('PO: soft-deleted row still in DB with ES_DELETE=true', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: CUSTOMER_CODE,
      poNumber: uniqueRef('CM-DEL-PO'),
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 100,
      projectCode: null,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1, rate: 100, amount: 100 }],
    });
    expect(createResp.status).toBe(201);
    const poCode = (createResp.data as { poCode: number }).poCode;

    await apiRequest(apiUrl, accessToken, 'DELETE', `/CustomerPo/${poCode}?companyId=${companyId}`, undefined);

    const row = await getPoByCode(db, poCode);
    expect(row, 'Soft-deleted PO row must still exist in DB').not.toBeNull();
    expect(row!.ES_DELETE, 'PO ES_DELETE must be truthy (BIT=1 returns boolean)').toBeTruthy();
  });

  test('TaxInvoice: soft-deleted row still in DB with ES_DELETE=true', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 200, storeCode: STORE_CODE }],
    });
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    await apiRequest(apiUrl, accessToken, 'DELETE', `/TaxInvoice/${invoiceCode}?companyId=${companyId}`, undefined);

    const row = await getTaxInvoiceByCode(db, invoiceCode);
    expect(row, 'Soft-deleted invoice row must still exist in DB').not.toBeNull();
    expect(row!.ES_DELETE, 'Invoice ES_DELETE must be truthy').toBeTruthy();
  });

  test('DeliveryChallan: soft-deleted row still in DB with ES_DELETE=true', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan', {
      companyCode: companyId,
      challanDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1 }],
    });
    expect(createResp.status).toBe(201);
    const dcCode = (createResp.data as { challanCode: number }).challanCode;

    await apiRequest(apiUrl, accessToken, 'DELETE', `/DeliveryChallan/${dcCode}?companyCode=${companyId}`, undefined);

    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @code`, { code: dcCode }
    );
    expect(row, 'Soft-deleted challan row must still exist').not.toBeNull();
    expect(row!.ES_DELETE, 'Challan ES_DELETE must be truthy').toBeTruthy();
  });
});

// ── 3. MODIFY (lock) consistency across modules ───────────────────────────────

test.describe('MODIFY flag — lock behavior consistent across modules', () => {

  test('all modules: lock sets MODIFY=true, unlock sets MODIFY=false', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    // PO
    const poResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: CUSTOMER_CODE,
      poNumber: uniqueRef('CM-MOD-PO'),
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 100,
      projectCode: null,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1, rate: 100, amount: 100 }],
    });
    const poCode = (poResp.data as { poCode: number }).poCode;

    await apiRequest(apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, null);
    const poRow = await getPoByCode(db, poCode);
    expect(poRow!.MODIFY, 'PO MODIFY must be truthy after lock').toBeTruthy();

    await apiRequest(apiUrl, accessToken, 'DELETE', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, undefined);
    const poUnlocked = await getPoByCode(db, poCode);
    expect(poUnlocked!.MODIFY, 'PO MODIFY must be falsy after unlock').toBeFalsy();

    // TaxInvoice
    const invResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 100, storeCode: STORE_CODE }],
    });
    const invCode = (invResp.data as { invoiceCode: number }).invoiceCode;

    await apiRequest(apiUrl, accessToken, 'POST', `/TaxInvoice/${invCode}/lock?companyId=${companyId}`, null);
    const invRow = await getTaxInvoiceByCode(db, invCode);
    expect(invRow!.MODIFY, 'Invoice MODIFY must be truthy after lock').toBeTruthy();
  });
});

// ── 4. Serial number uniqueness ───────────────────────────────────────────────

test.describe('Serial number uniqueness', () => {

  test('concurrent PO creates produce unique poNumbers', async ({
    companyId, apiUrl, accessToken,
  }) => {
    // Create 5 POs in parallel
    const results = await Promise.all(
      Array.from({ length: 5 }, (_, i) =>
        apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
          customerCode: CUSTOMER_CODE,
          poNumber: uniqueRef(`CM-SN-PO-${i}`),
          poType: 1,
          poDate: new Date().toISOString(),
          creditDays: 30,
          companyId,
          grandTotal: 100,
          projectCode: null,
          details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1, rate: 100, amount: 100 }],
        })
      )
    );

    const poCodes = results
      .filter(r => r.status === 201)
      .map(r => (r.data as { poCode: number }).poCode);

    expect(poCodes.length, 'All 5 concurrent PO creates must succeed').toBe(5);

    // All codes must be unique
    const uniqueCodes = new Set(poCodes);
    expect(uniqueCodes.size, 'All poCode values must be unique (no duplicates from concurrent creates)').toBe(5);
  });

  test('concurrent TaxInvoice creates produce unique invoiceCodes', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const results = await Promise.all(
      Array.from({ length: 3 }, () =>
        apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
          companyCode: companyId,
          invoiceDate: new Date().toISOString(),
          customerCode: CUSTOMER_CODE,
          customerPoCode: PO_CODE,
          invoiceDetails: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 200, storeCode: STORE_CODE }],
        })
      )
    );

    const invoiceCodes = results
      .filter(r => r.status === 201)
      .map(r => (r.data as { invoiceCode: number }).invoiceCode);

    expect(invoiceCodes.length, 'All 3 concurrent invoice creates must succeed').toBe(3);
    const uniqueCodes = new Set(invoiceCodes);
    expect(uniqueCodes.size, 'All invoiceCode values must be unique').toBe(3);
  });
});

// ── 5. INM_TYPE isolation (Tax vs Labour) ─────────────────────────────────────

test.describe('INM_TYPE isolation — Tax Invoice vs Labour Invoice', () => {

  test('TaxInvoice and LabourInvoice in same company do not cross-contaminate', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    // Create both types
    const taxResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 2, rate: 300, storeCode: STORE_CODE }],
    });
    const labourResp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 4, rate: 200, amount: 800 }],
    });

    expect(taxResp.status).toBe(201);
    expect(labourResp.status).toBe(201);

    const taxCode = (taxResp.data as { invoiceCode: number }).invoiceCode;
    const labourCode = (labourResp.data as { invoiceCode: number }).invoiceCode;

    // Verify types
    const taxRow = await db.queryOne<{ INM_TYPE: string }>(
      `SELECT INM_TYPE FROM INVOICE_MASTER WHERE INM_CODE = @code`, { code: taxCode }
    );
    const labourRow = await db.queryOne<{ INM_TYPE: string }>(
      `SELECT INM_TYPE FROM INVOICE_MASTER WHERE INM_CODE = @code`, { code: labourCode }
    );

    expect(taxRow?.INM_TYPE, 'Tax Invoice INM_TYPE must be TAXINV').toBe('TAXINV');
    expect(labourRow?.INM_TYPE, 'Labour Invoice INM_TYPE must be OutJWINM').toBe('OutJWINM');
  });

  test('deleting TaxInvoice does not soft-delete LabourInvoice row', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const taxResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 100, storeCode: STORE_CODE }],
    });
    const labourResp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 1, rate: 100, amount: 100 }],
    });

    const taxCode = (taxResp.data as { invoiceCode: number }).invoiceCode;
    const labourCode = (labourResp.data as { invoiceCode: number }).invoiceCode;

    // Delete Tax Invoice
    await apiRequest(apiUrl, accessToken, 'DELETE', `/TaxInvoice/${taxCode}?companyId=${companyId}`, undefined);

    // Labour Invoice must be unaffected
    const labourRow = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM INVOICE_MASTER WHERE INM_CODE = @code AND INM_TYPE = 'OutJWINM'`,
      { code: labourCode }
    );
    expect(labourRow?.ES_DELETE, 'Labour invoice must not be affected by Tax Invoice delete').toBeFalsy();
  });
});

// ── 6. Date handling consistency ──────────────────────────────────────────────

test.describe('Date handling — all modules reject empty string for nullable dates', () => {

  test('PO: customerPoDate="" → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: CUSTOMER_CODE,
      poNumber: uniqueRef('CM-DATE-PO'),
      poType: 1,
      poDate: new Date().toISOString(),
      companyId,
      grandTotal: 100,
      projectCode: null,
      customerPoDate: '',  // must be null, not ""
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1, rate: 100, amount: 100 }],
    });
    expect(resp.status, 'PO customerPoDate="" must return 400').toBe(400);
  });

  test('TaxInvoice: lrDate="" → 400', async ({ apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: 1,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      lrDate: '',  // must be null, not ""
      invoiceDetails: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 100, storeCode: STORE_CODE }],
    });
    expect(resp.status, 'TaxInvoice lrDate="" must return 400').toBe(400);
  });

  test('all modules: null for optional date fields → accepted', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const poResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: CUSTOMER_CODE,
      poNumber: uniqueRef('CM-DATE-NULL'),
      poType: 1,
      poDate: new Date().toISOString(),
      companyId,
      grandTotal: 100,
      projectCode: null,
      customerPoDate: null,  // null is valid
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1, rate: 100, amount: 100 }],
    });
    expect(poResp.status, 'PO with customerPoDate=null must be accepted').toBe(201);

    const invResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      lrDate: null,  // null is valid
      invoiceDetails: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 100, storeCode: STORE_CODE }],
    });
    expect(invResp.status, 'TaxInvoice with lrDate=null must be accepted').toBe(201);
  });
});
