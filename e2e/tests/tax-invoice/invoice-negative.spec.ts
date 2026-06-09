/**
 * Tax Invoice — Negative / Validation Tests
 *
 * Tests that the API correctly rejects invalid input for Tax Invoice.
 * Every test maps to a FluentValidation rule or controller guard.
 *
 * Critical historical bugs caught here:
 *   - lrDate="" caused 400 (DateTime? binding rejects empty string) — FIXED
 *   - companyCode required (not companyId) — different from PO module
 *   - invoiceDetails must be non-empty
 *   - invoiceQuantity must be > 0
 *
 * Tests:
 *   - Missing required fields → 400 with error body
 *   - Empty invoiceDetails → 400
 *   - lrDate="" → 400 (raw, no sanitization — proves backend rejects it)
 *   - companyCode=0 → 400
 *   - GET/DELETE non-existent → 404
 *   - Unauthenticated → 401
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

const COMPANY_CODE = 1;
const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const STORE_CODE = 1;
const UOM_CODE = 1;
const PO_CODE = 9001;

const VALID_DETAIL = {
  itemCode: ITEM_CODE,
  uomCode: UOM_CODE,
  invoiceQuantity: 1,
  rate: 500,
  storeCode: STORE_CODE,
  cgstPercentage: 9,
  sgstPercentage: 9,
};

function basePayload(overrides?: object) {
  return {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    invoiceDetails: [VALID_DETAIL],
    ...overrides,
  };
}

async function createInvoice(apiUrl: string, accessToken: string): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', basePayload());
  if (resp.status !== 201) throw new Error(`Invoice create failed: ${resp.status}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Tax Invoice — Validation (Negative Tests)', () => {

  // ── Create validation ────────────────────────────────────────────────────────

  test('POST without customerCode → 400', async ({ apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice',
      basePayload({ customerCode: 0 })
    );
    expect(resp.status, 'customerCode=0 must return 400').toBe(400);
  });

  test('POST with empty invoiceDetails → 400', async ({ apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice',
      basePayload({ invoiceDetails: [] })
    );
    expect(resp.status, 'Empty invoiceDetails must return 400').toBe(400);
  });

  test('POST with companyCode=0 → 400', async ({ apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice',
      basePayload({ companyCode: 0 })
    );
    expect(resp.status, 'companyCode=0 must return 400').toBe(400);
  });

  test('POST without invoiceDate → 400', async ({ apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice',
      basePayload({ invoiceDate: null })
    );
    expect(resp.status, 'Missing invoiceDate must return 400').toBe(400);
  });

  test('POST with lrDate="" → 400 (backend rejects empty string for nullable DateTime)', async ({
    apiUrl, accessToken,
  }) => {
    // This test proves the BACKEND rejects "" for lrDate directly.
    // The frontend sanitize function converts "" → null before sending.
    // This test sends "" raw (bypassing frontend sanitization) to prove the
    // backend-side validation is also in place as a defense-in-depth measure.
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice',
      basePayload({ lrDate: '' })
    );
    expect(
      resp.status,
      'lrDate="" must return 400 — backend DateTime binding rejects empty string'
    ).toBe(400);
  });

  test('POST with invoiceQuantity=0 in detail → 400', async ({ apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice',
      basePayload({
        invoiceDetails: [{ ...VALID_DETAIL, invoiceQuantity: 0 }],
      })
    );
    expect(resp.status, 'invoiceQuantity=0 must return 400').toBe(400);
  });

  test('POST with negative invoiceQuantity → 400', async ({ apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice',
      basePayload({
        invoiceDetails: [{ ...VALID_DETAIL, invoiceQuantity: -5 }],
      })
    );
    expect(resp.status, 'Negative invoiceQuantity must return 400').toBe(400);
  });

  // ── Update validation ────────────────────────────────────────────────────────

  test('PUT with empty invoiceDetails → 400', async ({ companyId, apiUrl, accessToken }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/TaxInvoice/${invoiceCode}`, {
      invoiceCode,
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [],
    });
    expect(updateResp.status, 'PUT with empty invoiceDetails must return 400').toBe(400);
  });

  test('PUT with invoiceCode mismatch (body vs URL) → 400', async ({ apiUrl, accessToken }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    // Send body with wrong invoiceCode
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/TaxInvoice/${invoiceCode}`, {
      invoiceCode: 99999999,  // mismatch with URL id
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      invoiceDetails: [VALID_DETAIL],
    });
    // Controller should normalize (like PO controller) OR return 400
    expect(
      [200, 204, 400],
      'Invoice code mismatch must either be normalized (200) or rejected (400) — never 500'
    ).toContain(updateResp.status);
    expect(updateResp.status).not.toBe(500);
  });

  // ── 404 for non-existent resources ──────────────────────────────────────────

  test('GET non-existent invoice → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'GET', `/TaxInvoice/999999999?companyId=${companyId}`);
    expect(resp.status, 'GET non-existent invoice must return 404').toBe(404);
  });

  test('DELETE non-existent invoice → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/TaxInvoice/999999999?companyId=${companyId}`, undefined
    );
    expect(resp.status, 'DELETE non-existent invoice must return 404').toBe(404);
  });

  // ── Unauthenticated ──────────────────────────────────────────────────────────

  test('GET /TaxInvoice without auth → 401', async ({ companyId, apiUrl }) => {
    const resp = await apiRequest(apiUrl, '', 'GET', `/TaxInvoice?companyId=${companyId}`);
    expect(resp.status, 'Unauthenticated request must return 401').toBe(401);
  });

  test('POST /TaxInvoice without auth → 401', async ({ apiUrl }) => {
    const resp = await apiRequest(apiUrl, '', 'POST', '/TaxInvoice', basePayload());
    expect(resp.status, 'Unauthenticated POST must return 401').toBe(401);
  });
});
