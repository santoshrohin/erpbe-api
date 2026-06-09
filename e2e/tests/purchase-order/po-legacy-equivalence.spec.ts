/**
 * Customer Purchase Order — Legacy Equivalence Verification
 *
 * Proves that the new React + .NET stack behaves exactly like the legacy
 * ASPX application (CustomerPO.aspx.cs + ViewCustomerPO.aspx.cs).
 *
 * Each test maps to a specific legacy code path identified during Phase 2.
 */

import { test, expect, uniqueRef } from '../../fixtures';
import {
  getPoByCode,
  getPoByNumber,
  getPoDetails,
  isPoLocked,
  isPoDeleted,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE_1   = 9001;
const ITEM_CODE_2   = 9002;
const UOM_CODE      = 1;

function basePoPayload(companyId: number, poNumber: string, overrides?: object) {
  const now = new Date().toISOString();
  return {
    customerCode:  CUSTOMER_CODE,
    poNumber,
    poType:        1,
    poDate:        now,
    customerPoDate: now,
    creditDays:    30,
    companyId,
    grandTotal:    10000,
    details: [
      { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 10, rate: 1000, amount: 10000 },
    ],
    ...overrides,
  };
}

// ─── 1. CREATE ────────────────────────────────────────────────────────────────

test.describe('Create PO', () => {

  test('create → DB state correct: MODIFY=0, ES_DELETE=0, AM_COUNT=0', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-CREATE');

    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    expect(resp.status).toBe(201);
    const poCode = (resp.data as { poCode: number }).poCode;
    expect(poCode).toBeGreaterThan(0);

    const row = await getPoByCode(db, poCode);
    expect(row).not.toBeNull();
    expect(row!.CPOM_PONO).toBe(poNumber);
    expect(row!.ES_DELETE).toBeFalsy();
    expect(row!.MODIFY).toBeFalsy();
    expect(row!.CPOM_AM_COUNT ?? 0).toBe(0);

    const details = await getPoDetails(db, poCode);
    expect(details).toHaveLength(1);
    expect(details[0].CPOD_I_CODE).toBe(ITEM_CODE_1);
    expect(details[0].CPOD_ORD_QTY).toBe(10);
    expect(details[0].CPOD_RATE).toBe(1000);
  });

  test('create with 2 line items → both detail rows in DB', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-CREATE-2');

    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber, {
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 5,  rate: 1000, amount: 5000 },
        { itemCode: ITEM_CODE_2, uomCode: UOM_CODE, orderedQuantity: 10, rate: 200,  amount: 2000 },
      ],
      grandTotal: 7000,
    }));
    expect(resp.status).toBe(201);
    const poCode = (resp.data as { poCode: number }).poCode;

    const details = await getPoDetails(db, poCode);
    expect(details).toHaveLength(2);
    const itemCodes = details.map(d => d.CPOD_I_CODE).sort();
    expect(itemCodes).toEqual([ITEM_CODE_1, ITEM_CODE_2].sort());
  });
});

// ─── 2. EDIT ─────────────────────────────────────────────────────────────────

test.describe('Edit PO', () => {

  test('edit → fields updated in DB, CPOM_AM_COUNT stays 0 (MODIFY path, not AMEND)', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-EDIT');

    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    expect(createResp.status).toBe(201);
    const poCode = (createResp.data as { poCode: number }).poCode;

    const before = await getPoByCode(db, poCode);
    expect(before!.CPOM_AM_COUNT ?? 0).toBe(0);

    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`, {
      ...basePoPayload(companyId, poNumber),
      poCode,
      paymentTerms: 'Net 60 Updated',
      grandTotal:   8000,
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 8, rate: 1000, amount: 8000 },
      ],
    });
    expect([200, 204]).toContain(updateResp.status);

    const after = await getPoByCode(db, poCode);
    expect(after!.CPOM_PAY_TERM).toContain('Net 60');
    // Legacy MODIFY path: no amendment tracking
    expect(after!.CPOM_AM_COUNT ?? 0, 'MODIFY must NOT increment CPOM_AM_COUNT').toBe(0);
  });

  test('edit replaces detail lines — no orphan rows remain', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-EDIT-DETAIL');

    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber, {
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 10, rate: 1000, amount: 10000 },
        { itemCode: ITEM_CODE_2, uomCode: UOM_CODE, orderedQuantity: 5,  rate: 200,  amount: 1000  },
      ],
      grandTotal: 11000,
    }));
    const poCode = (createResp.data as { poCode: number }).poCode;
    expect((await getPoDetails(db, poCode))).toHaveLength(2);

    // Remove item 2, change item 1 qty
    await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`, {
      ...basePoPayload(companyId, poNumber),
      poCode,
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 7, rate: 1000, amount: 7000 },
      ],
      grandTotal: 7000,
    });

    const detailsAfter = await getPoDetails(db, poCode);
    expect(detailsAfter, 'No orphan rows — only item 1 should remain').toHaveLength(1);
    expect(detailsAfter[0].CPOD_I_CODE).toBe(ITEM_CODE_1);
    expect(detailsAfter[0].CPOD_ORD_QTY).toBe(7);
  });
});

// ─── 3. DELETE ────────────────────────────────────────────────────────────────

test.describe('Delete PO', () => {

  test('delete → soft-delete (ES_DELETE=1), not physical row removal', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-DEL');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    const poCode = (createResp.data as { poCode: number }).poCode;

    const delResp = await apiRequest(apiUrl, accessToken, 'DELETE', `/CustomerPo/${poCode}?companyId=${companyId}`, null);
    expect([200, 204]).toContain(delResp.status);

    expect(await isPoDeleted(db, poCode)).toBeTruthy();
    // Row still physically exists
    const row = await getPoByCode(db, poCode);
    expect(row).not.toBeNull();
  });
});

// ─── 4. LOCK ─────────────────────────────────────────────────────────────────

test.describe('Lock behaviour', () => {

  test('lock sets MODIFY=1 in DB, unlock sets MODIFY=0', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-LOCK');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    const poCode = (createResp.data as { poCode: number }).poCode;

    expect(await isPoLocked(db, poCode)).toBeFalsy();

    await apiRequest(apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, null);
    expect(await isPoLocked(db, poCode), 'MODIFY must be 1 after lock').toBeTruthy();

    await apiRequest(apiUrl, accessToken, 'DELETE', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, null);
    expect(await isPoLocked(db, poCode), 'MODIFY must be 0 after unlock').toBeFalsy();
  });

  test('second lock attempt on already-locked PO returns 409 (legacy ModifyLog block)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-LOCK-409');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    const poCode = (createResp.data as { poCode: number }).poCode;

    await apiRequest(apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, null);
    const secondLock = await apiRequest(apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, null);
    expect(secondLock.status).toBe(409);

    // Cleanup
    await apiRequest(apiUrl, accessToken, 'DELETE', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, null);
  });
});

// ─── 5. VALIDATION ────────────────────────────────────────────────────────────

test.describe('Validation — legacy business rules', () => {

  test('PO Date earlier than Customer PO Date → 400 (legacy line 230: PoDate < CustPoDate)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-PODATE');
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      ...basePoPayload(companyId, poNumber),
      poDate:        '2024-01-10T00:00:00.000Z',
      customerPoDate: '2024-01-15T00:00:00.000Z',  // PO date is BEFORE customer PO date
    });
    expect(resp.status).toBe(400);
  });

  test('duplicate PO number → rejected (legacy uniqueness check)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-DUP');

    // First create succeeds
    const first = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    expect(first.status).toBe(201);

    // Second create with same number must fail
    const second = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    expect(second.status, 'Duplicate PO number must be rejected').toBeGreaterThanOrEqual(400);
  });

  test('verbal order allows duplicate PO number (legacy chkIsVerbal bypasses uniqueness check)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-VERBAL-DUP');

    const first = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber, { isVerbalOrder: true, poNumber: 'VERBAL' }));
    expect(first.status).toBe(201);

    // Second verbal order with same 'VERBAL' number must also succeed
    const second = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber, { isVerbalOrder: true, poNumber: 'VERBAL' }));
    expect(second.status, 'Verbal orders must bypass PO number uniqueness').toBe(201);
  });

  test('empty details list → 400 (at least one line item required)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      ...basePoPayload(companyId, uniqueRef('EQ-NOITEMS')),
      details: [],
    });
    expect(resp.status).toBe(400);
  });

  test('update PO with duplicate number (excluding self) → rejected', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const po1Number = uniqueRef('EQ-UPD-DUP1');
    const po2Number = uniqueRef('EQ-UPD-DUP2');

    const resp1 = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, po1Number));
    const resp2 = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, po2Number));
    const poCode2 = (resp2.data as { poCode: number }).poCode;

    // Try to update PO2 to use PO1's number
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode2}`, {
      ...basePoPayload(companyId, po1Number),  // <-- duplicate number
      poCode: poCode2,
    });
    expect(updateResp.status, 'Update with duplicate number must be rejected').toBeGreaterThanOrEqual(400);
  });

  test('update self with same PO number → allowed (exclusion logic works)', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-UPD-SELF');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    const poCode = (createResp.data as { poCode: number }).poCode;

    // Update using same PO number — must succeed (self-exclusion)
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`, {
      ...basePoPayload(companyId, poNumber),
      poCode,
      paymentTerms: 'Net 45',
    });
    expect(updateResp.status, 'Updating own PO number must be allowed').toBeGreaterThanOrEqual(200);
    expect(updateResp.status).toBeLessThan(300);
  });
});

// ─── 6. UI — VERBAL ORDER ─────────────────────────────────────────────────────

test.describe('UI — Verbal Order', () => {

  test('checking Verbal Order sets PO Number to VERBAL and disables the field', async ({
    page,
  }) => {
    await page.goto('/transactions/purchase-order/create');
    await page.waitForLoadState('networkidle');

    const poNumberInput = page.locator('[name="poNumber"]');
    await expect(poNumberInput).toBeEnabled();

    // Check the verbal order checkbox
    await page.locator('[name="isVerbalOrder"]').check();

    // PO Number field must auto-set to 'VERBAL' and be disabled
    await expect(poNumberInput).toHaveValue('VERBAL');
    await expect(poNumberInput).toBeDisabled();
  });
});

// ─── 7. UI — LOCK ACQUIRED AND RELEASED ──────────────────────────────────────

test.describe('UI — Lock lifecycle', () => {

  test('opening edit page acquires lock (MODIFY=1); cancel releases it (MODIFY=0)', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-UI-LOCK');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    const poCode = (createResp.data as { poCode: number }).poCode;

    // Navigate to edit — lock should be acquired
    await page.goto(`/transactions/purchase-order/edit/${poCode}`);
    await page.waitForLoadState('networkidle');
    // Wait for form to load (customer field visible)
    await expect(page.locator('input[placeholder="Select Customer"]').first()).toBeVisible({ timeout: 10_000 });

    expect(await isPoLocked(db, poCode), 'MODIFY must be 1 after opening edit page').toBeTruthy();

    // Click Cancel — lock should be released
    await page.locator('button[type="button"]:has-text("Cancel")').click();
    await page.waitForURL('**/purchase-order');

    expect(await isPoLocked(db, poCode), 'MODIFY must be 0 after cancel').toBeFalsy();
  });

  test('navigating away from edit page (back button) releases lock', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('EQ-UI-NAVAWAY');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
    const poCode = (createResp.data as { poCode: number }).poCode;

    await page.goto(`/transactions/purchase-order/edit/${poCode}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('input[placeholder="Select Customer"]').first()).toBeVisible({ timeout: 10_000 });
    expect(await isPoLocked(db, poCode)).toBeTruthy();

    // Navigate away via the Back button (SPA navigation — keeps the document alive
    // so the cleanup unlock fetch can complete; page.goBack() causes a full reload
    // which cancels in-flight requests before the server processes them).
    await page.locator('button:has-text("Back to Purchase Orders")').click();
    await page.waitForURL('**/purchase-order', { timeout: 8_000 });
    await page.waitForTimeout(1500);

    expect(await isPoLocked(db, poCode), 'MODIFY must be 0 after navigating away').toBeFalsy();
  });
});
