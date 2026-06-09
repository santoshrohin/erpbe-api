/**
 * Labour Charge Invoice — Negative / Validation Tests
 *
 * Every test maps to a FluentValidation rule or business guard in the backend.
 * Tests cover:
 *   - Missing required fields (customerCode, companyCode, details)
 *   - Invalid quantity (0 and negative)
 *   - Locked invoice blocks edit and delete
 *   - Non-existent invoice → 404
 *   - Unauthorized → 401
 */

import { test, expect } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const PO_CODE = 9001;

function basePayload(companyId: number, overrides?: object) {
  return {
    companyCode: companyId,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    details: [{ invoiceQuantity: 5, rate: 500, amount: 2500 }],
    ...overrides,
  };
}

async function createLabourInvoice(apiUrl: string, accessToken: string, companyId: number): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', basePayload(companyId));
  if (resp.status !== 201) throw new Error(`Create failed: ${resp.status} — ${JSON.stringify(resp.data)}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Labour Charge Invoice — Validation (Negative Tests)', () => {

  // ── Create validation ────────────────────────────────────────────────────────

  test('POST without customerCode → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice',
      basePayload(companyId, { customerCode: 0 })
    );
    expect(resp.status, 'customerCode=0 must return 400').toBe(400);
  });

  test('POST without companyCode → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice',
      basePayload(companyId, { companyCode: 0 })
    );
    expect(resp.status, 'companyCode=0 must return 400').toBe(400);
  });

  test('POST with empty details → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice',
      basePayload(companyId, { details: [] })
    );
    expect(resp.status, 'Empty details must return 400').toBe(400);
  });

  test('POST with quantity=0 → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice',
      basePayload(companyId, {
        details: [{ invoiceQuantity: 0, rate: 500, amount: 0 }],
      })
    );
    expect(resp.status, 'invoiceQuantity=0 must return 400').toBe(400);
  });

  test('POST with negative quantity → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice',
      basePayload(companyId, {
        details: [{ invoiceQuantity: -3, rate: 500, amount: -1500 }],
      })
    );
    expect(resp.status, 'Negative invoiceQuantity must return 400').toBe(400);
  });

  // ── Lock guards ──────────────────────────────────────────────────────────────

  test('edit locked invoice → rejected (locked guard)', async ({ companyId, apiUrl, accessToken }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId);

    // Lock it
    const lockResp = await apiRequest(apiUrl, accessToken, 'POST', `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null);
    expect([200, 204]).toContain(lockResp.status);

    // Attempt update while locked — must be rejected
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/LabourChargeInvoice/${invoiceCode}`,
      basePayload(companyId, {
        invoiceCode,
        details: [{ invoiceQuantity: 2, rate: 100, amount: 200 }],
      })
    );
    expect(updateResp.status, 'Edit on locked invoice must be rejected').not.toBe(200);
    expect(updateResp.status, 'Edit on locked invoice must be rejected').not.toBe(204);
  });

  test('delete locked invoice → rejected (locked guard)', async ({ companyId, apiUrl, accessToken }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId);

    // Lock it
    await apiRequest(apiUrl, accessToken, 'POST', `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null);

    // Attempt delete while locked — must be rejected
    const deleteResp = await apiRequest(apiUrl, accessToken, 'DELETE',
      `/LabourChargeInvoice/${invoiceCode}?companyCode=${companyId}`, undefined
    );
    expect(deleteResp.status, 'Delete on locked invoice must be rejected').not.toBe(200);
    expect(deleteResp.status, 'Delete on locked invoice must be rejected').not.toBe(204);
  });

  // ── Not found ────────────────────────────────────────────────────────────────

  test('GET non-existent invoice → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'GET', `/LabourChargeInvoice/99999999?companyCode=${companyId}`, undefined);
    expect(resp.status, 'Non-existent invoice GET must return 404').toBe(404);
  });

  test('DELETE non-existent invoice → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'DELETE', `/LabourChargeInvoice/99999999?companyCode=${companyId}`, undefined);
    expect(resp.status, 'Non-existent invoice DELETE must return 404').toBe(404);
  });

  // ── Auth ─────────────────────────────────────────────────────────────────────

  test('GET without auth token → 401', async ({ companyId, apiUrl }) => {
    const resp = await apiRequest(apiUrl, '', 'GET', `/LabourChargeInvoice?companyCode=${companyId}`, undefined);
    expect(resp.status, 'Unauthenticated GET must return 401').toBe(401);
  });

  test('POST without auth token → 401', async ({ companyId, apiUrl }) => {
    const resp = await apiRequest(apiUrl, '', 'POST', '/LabourChargeInvoice', basePayload(companyId));
    expect(resp.status, 'Unauthenticated POST must return 401').toBe(401);
  });
});
