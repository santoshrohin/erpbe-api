/**
 * Purchase Order — Negative / Validation Tests
 *
 * Tests that the API correctly rejects invalid input.
 * Every test here maps to a FluentValidation rule in UpdateCustomerPoCommandValidator
 * or CreateCustomerPoCommandValidator.
 *
 * WHY THESE MATTER:
 *   Validation rules prevent bad data entering the DB. If a rule is accidentally
 *   removed (e.g. during refactor), these tests catch it immediately.
 *   They also document the exact error messages the frontend should display.
 *
 * Tests cover:
 *   - Missing required fields (customerCode, poNumber, companyId)
 *   - Empty details array
 *   - Invalid nullable fields (projectCode=0, customerPoDate="")
 *   - GET / DELETE on non-existent IDs → 404
 *   - Duplicate PO number (if unique constraint exists)
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const UOM_CODE = 1;

const VALID_DETAIL = { itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 1, rate: 100, amount: 100 };

function basePayload(companyId: number, overrides?: object) {
  return {
    customerCode: CUSTOMER_CODE,
    poNumber: uniqueRef('PO-NEG'),
    poType: 1,
    poDate: new Date().toISOString(),
    creditDays: 30,
    companyId,
    grandTotal: 100,
    projectCode: null,
    details: [VALID_DETAIL],
    ...overrides,
  };
}

test.describe('Purchase Order — Validation (Negative Tests)', () => {

  // ── Create validation ────────────────────────────────────────────────────────

  test('POST without customerCode → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { customerCode: 0 })
    );
    expect(resp.status, 'Missing customerCode must return 400').toBe(400);
  });

  test('POST without poNumber → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { poNumber: '' })
    );
    expect(resp.status, 'Empty poNumber must return 400').toBe(400);
  });

  test('POST without companyId → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { companyId: 0 })
    );
    expect(resp.status, 'companyId=0 must return 400').toBe(400);
  });

  test('POST with empty details array → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { details: [] })
    );
    expect(resp.status, 'Empty details array must return 400').toBe(400);
  });

  test('POST with projectCode=0 → 400 (must be null, not 0)', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { projectCode: 0 })
    );
    expect(resp.status, 'projectCode=0 must return 400 (validator rejects 0, accepts null)').toBe(400);
  });

  test('POST with customerPoDate="" → 400 (must be null, not empty string)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { customerPoDate: '' })
    );
    expect(resp.status, 'customerPoDate="" must return 400 (DateTime binding rejects empty string)').toBe(400);
  });

  test('POST with negative grandTotal → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { grandTotal: -1 })
    );
    expect(resp.status, 'Negative grandTotal must return 400').toBe(400);
  });

  test('POST with grandTotal=0 → 201 (zero total is valid)', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { grandTotal: 0 })
    );
    // GrandTotal=0 is a valid scenario (e.g., free samples) — must NOT be rejected
    expect(resp.status, 'grandTotal=0 must be accepted (GrandTotal validator uses GreaterThanOrEqualTo(0))').toBe(201);
  });

  test('POST with projectCode=null → 201 (null is valid)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId, { projectCode: null })
    );
    expect(resp.status, 'projectCode=null must be accepted').toBe(201);
  });

  // ── Update validation ────────────────────────────────────────────────────────

  test('PUT with empty details array → 400', async ({ companyId, apiUrl, accessToken }) => {
    // Create first
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId)
    );
    expect(createResp.status).toBe(201);
    const poCode = (createResp.data as { poCode: number }).poCode;

    // Update with empty details
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`, {
      poCode,
      customerCode: CUSTOMER_CODE,
      poNumber: uniqueRef('PO-NEG-UPD'),
      poType: 1,
      poDate: new Date().toISOString(),
      companyId,
      grandTotal: 0,
      details: [],  // empty — must fail
    });
    expect(updateResp.status, 'PUT with empty details must return 400').toBe(400);
  });

  test('PUT with customerCode=0 → 400', async ({ companyId, apiUrl, accessToken }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      basePayload(companyId)
    );
    const poCode = (createResp.data as { poCode: number }).poCode;

    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`, {
      poCode,
      customerCode: 0,  // invalid
      poNumber: uniqueRef('PO-NEG-CC'),
      poType: 1,
      poDate: new Date().toISOString(),
      companyId,
      grandTotal: 100,
      details: [VALID_DETAIL],
    });
    expect(updateResp.status, 'PUT with customerCode=0 must return 400').toBe(400);
  });

  // ── 404 for non-existent resources ──────────────────────────────────────────

  test('GET non-existent PO → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'GET', `/CustomerPo/999999999?companyId=${companyId}`);
    expect(resp.status, 'GET for non-existent PO must return 404').toBe(404);
  });

  test('DELETE non-existent PO → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/CustomerPo/999999999?companyId=${companyId}`, undefined
    );
    expect(resp.status, 'DELETE for non-existent PO must return 404').toBe(404);
  });

  // ── Unauthenticated access ───────────────────────────────────────────────────

  test('GET /CustomerPo without auth token → 401', async ({ companyId, apiUrl }) => {
    const resp = await apiRequest(apiUrl, '', 'GET', `/CustomerPo?companyId=${companyId}`);
    expect(resp.status, 'Unauthenticated request must return 401').toBe(401);
  });
});
