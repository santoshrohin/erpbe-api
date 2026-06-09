/**
 * Delivery Challan — Negative / Validation Tests
 *
 * Every test maps to a FluentValidation rule or business guard in the backend.
 * Tests cover:
 *   - Missing required fields (customerCode, companyCode, details)
 *   - Invalid quantity (0 and negative)
 *   - Duplicate items in same challan
 *   - Locked challan blocks edit and delete
 *   - Non-existent challan → 404
 *   - Unauthorized → 401
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const ITEM_CODE_2 = 9002;
const UOM_CODE = 1;

function basePayload(companyId: number, overrides?: object) {
  return {
    companyCode: companyId,
    challanDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 2 }],
    ...overrides,
  };
}

async function createChallan(apiUrl: string, accessToken: string, companyId: number): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan', basePayload(companyId));
  if (resp.status !== 201) throw new Error(`Create failed: ${resp.status} — ${JSON.stringify(resp.data)}`);
  return (resp.data as { challanCode: number }).challanCode;
}

test.describe('Delivery Challan — Validation (Negative Tests)', () => {

  // ── Create validation ────────────────────────────────────────────────────────

  test('POST without customerCode → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan',
      basePayload(companyId, { customerCode: 0 })
    );
    expect(resp.status, 'customerCode=0 must return 400').toBe(400);
  });

  test('POST without companyCode → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan',
      basePayload(companyId, { companyCode: 0 })
    );
    expect(resp.status, 'companyCode=0 must return 400').toBe(400);
  });

  test('POST with empty details → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan',
      basePayload(companyId, { details: [] })
    );
    expect(resp.status, 'Empty details must return 400').toBe(400);
  });

  test('POST with quantity=0 → 400 (must be greater than zero)', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan',
      basePayload(companyId, {
        details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 0 }],
      })
    );
    expect(resp.status, 'orderedQuantity=0 must return 400').toBe(400);
  });

  test('POST with negative quantity → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan',
      basePayload(companyId, {
        details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: -5 }],
      })
    );
    expect(resp.status, 'Negative orderedQuantity must return 400').toBe(400);
  });

  test('POST with duplicate item codes → 400', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan',
      basePayload(companyId, {
        details: [
          { itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 2 },
          { itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 3 },
        ],
      })
    );
    expect(resp.status, 'Duplicate item codes must return 400').toBe(400);
  });

  // ── Lock guards ──────────────────────────────────────────────────────────────

  test('edit locked challan → 400/422/500 (locked guard)', async ({ companyId, apiUrl, accessToken }) => {
    const challanCode = await createChallan(apiUrl, accessToken, companyId);

    // Lock it
    const lockResp = await apiRequest(apiUrl, accessToken, 'POST', `/DeliveryChallan/${challanCode}/lock?companyCode=${companyId}`, null);
    expect([200, 204]).toContain(lockResp.status);

    // Attempt edit while locked — must be rejected
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/DeliveryChallan/${challanCode}`,
      basePayload(companyId, {
        challanCode,
        details: [{ itemCode: ITEM_CODE_2, uomCode: UOM_CODE, orderedQuantity: 1 }],
      })
    );
    expect(updateResp.status, 'Edit on locked challan must be rejected').not.toBe(200);
    expect(updateResp.status, 'Edit on locked challan must be rejected').not.toBe(204);
  });

  test('delete locked challan → 400/422/500 (locked guard)', async ({ companyId, apiUrl, accessToken }) => {
    const challanCode = await createChallan(apiUrl, accessToken, companyId);

    // Lock it
    await apiRequest(apiUrl, accessToken, 'POST', `/DeliveryChallan/${challanCode}/lock?companyCode=${companyId}`, null);

    // Attempt delete while locked — must be rejected
    const deleteResp = await apiRequest(apiUrl, accessToken, 'DELETE',
      `/DeliveryChallan/${challanCode}?companyCode=${companyId}`, undefined
    );
    expect(deleteResp.status, 'Delete on locked challan must be rejected').not.toBe(200);
    expect(deleteResp.status, 'Delete on locked challan must be rejected').not.toBe(204);
  });

  // ── Not found ────────────────────────────────────────────────────────────────

  test('GET non-existent challan → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'GET', `/DeliveryChallan/99999999?companyCode=${companyId}`, undefined);
    expect(resp.status, 'Non-existent challan GET must return 404').toBe(404);
  });

  test('DELETE non-existent challan → 404', async ({ companyId, apiUrl, accessToken }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'DELETE', `/DeliveryChallan/99999999?companyCode=${companyId}`, undefined);
    expect(resp.status, 'Non-existent challan DELETE must return 404').toBe(404);
  });

  // ── Auth ─────────────────────────────────────────────────────────────────────

  test('GET without auth token → 401', async ({ companyId, apiUrl }) => {
    const resp = await apiRequest(apiUrl, '', 'GET', `/DeliveryChallan?companyCode=${companyId}`, undefined);
    expect(resp.status, 'Unauthenticated GET must return 401').toBe(401);
  });

  test('POST without auth token → 401', async ({ companyId, apiUrl }) => {
    const resp = await apiRequest(apiUrl, '', 'POST', '/DeliveryChallan', basePayload(companyId));
    expect(resp.status, 'Unauthenticated POST must return 401').toBe(401);
  });
});
