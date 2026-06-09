/**
 * Purchase Order — Edit workflow
 *
 * Parity:
 *   - Plain MODIFY edit (PUT /CustomerPo/:id) must NOT increment CPOM_AM_COUNT.
 *     Legacy CustomerPO.aspx.cs only increments CPOM_AM_COUNT on the AMEND path
 *     (copies to CUSTPO_AM_MASTER + CUSTPO_AMD_DETAIL).  The plain MODIFY path
 *     does a straight UPDATE with no amendment tracking.
 *   - Detail lines are re-created on edit (delete old + insert new).
 *     No orphan rows must remain in CUSTPO_DETAIL.
 *   - Locked PO must block edit entirely.
 *
 * Tests:
 *   1. Create PO → edit payment terms → CPOM_PAY_TERM updated in DB, CPOM_AM_COUNT stays 0
 *   2. Edit replaces detail lines — no orphan rows
 *   3. Edit button disabled in list for locked PO
 */

import { test, expect, uniqueRef } from '../../fixtures';
import {
  getPoByCode,
  getPoByNumber,
  getPoDetails,
  isPoLocked,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE_1 = 9001;
const ITEM_CODE_2 = 9002;
const UOM_CODE = 1;

function poPayload(companyId: number, poNumber: string, overrides?: object) {
  return {
    customerCode: CUSTOMER_CODE,
    poNumber,
    poType: 1,
    poDate: new Date().toISOString(),
    creditDays: 30,
    companyId,
    grandTotal: 11800,
    projectCode: null,
    paymentTerms: 'Net 30 Original',
    details: [
      { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 10, rate: 1000, amount: 10000 },
    ],
    ...overrides,
  };
}

test.describe('Purchase Order — Edit', () => {

  test('edit PO → CPOM_PAY_TERM updated, CPOM_AM_COUNT stays 0 (plain MODIFY, not AMEND)', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-EDIT');

    // Create
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', poPayload(companyId, poNumber));
    expect(createResp.status).toBe(201);
    const poCode = (createResp.data as { poCode: number }).poCode;
    expect(poCode).toBeGreaterThan(0);

    // Verify initial amendment count = 0
    const before = await getPoByCode(db, poCode);
    expect(before).not.toBeNull();
    expect(before!.CPOM_AM_COUNT ?? 0).toBe(0);

    // Edit — change payment terms and grand total
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`, {
      poCode,
      customerCode: CUSTOMER_CODE,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 11800,
      projectCode: null,
      paymentTerms: 'Net 60 Updated',
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 10, rate: 1000, amount: 10000 },
      ],
    });
    expect([200, 204], `PUT /CustomerPo/${poCode} failed: ${updateResp.status}`).toContain(updateResp.status);

    // DB: payment terms updated
    const after = await getPoByCode(db, poCode);
    expect(after).not.toBeNull();
    expect(after!.CPOM_PAY_TERM).toContain('Net 60');

    // DB: plain MODIFY must NOT increment amendment counter — only AMEND path does
    expect(after!.CPOM_AM_COUNT ?? 0, 'CPOM_AM_COUNT must stay 0 after plain edit (only AMEND increments it)').toBe(0);
  });

  test('edit replaces detail lines — no orphan rows, correct item/qty after edit', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-DETAIL-EDIT');

    // Create with 2 detail lines
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      ...poPayload(companyId, poNumber),
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 10, rate: 1000, amount: 10000 },
        { itemCode: ITEM_CODE_2, uomCode: UOM_CODE, orderedQuantity: 5, rate: 200, amount: 1000 },
      ],
      grandTotal: 11000,
    });
    expect(createResp.status).toBe(201);
    const poCode = (createResp.data as { poCode: number }).poCode;

    // Assert 2 detail lines created
    const detailsBefore = await getPoDetails(db, poCode);
    expect(detailsBefore.length, 'Must have 2 detail lines after create').toBe(2);
    expect(detailsBefore.map(d => d.CPOD_I_CODE).sort()).toEqual([ITEM_CODE_1, ITEM_CODE_2].sort());

    // Edit — remove item 2, change item 1 quantity
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`, {
      poCode,
      customerCode: CUSTOMER_CODE,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 8000,
      projectCode: null,
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 8, rate: 1000, amount: 8000 },
        // Item 2 intentionally removed
      ],
    });
    expect([200, 204]).toContain(updateResp.status);

    // DB: only 1 detail row for item 1 with new quantity
    const detailsAfter = await getPoDetails(db, poCode);
    expect(detailsAfter.length, 'Edit must leave exactly 1 detail row (orphans removed)').toBe(1);
    expect(detailsAfter[0].CPOD_I_CODE).toBe(ITEM_CODE_1);
    expect(detailsAfter[0].CPOD_ORD_QTY).toBe(8);
  });

  // ── Phase 2: customerPoDate parity ──────────────────────────────────────────

  test('edit form — null customerPoDate from API loads as today (no blank date)', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-DATE-NULL');

    // Create PO without customerPoDate so it is NULL in DB
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo',
      poPayload(companyId, poNumber, { customerPoDate: null })
    );
    expect(createResp.status).toBe(201);
    const poCode = (createResp.data as { poCode: number }).poCode;

    // Navigate to edit form
    await page.goto(`/transactions/purchase-order/edit/${poCode}`);
    await expect(page.locator('input[placeholder="Select Customer"]').first()).toBeEnabled({ timeout: 10_000 });

    // customerPoDate field must default to today, not be blank (blank → backend 400)
    const today = new Date().toISOString().split('T')[0];
    const customerPoDateInput = page.locator('[name="customerPoDate"]');
    await expect(customerPoDateInput).toBeVisible({ timeout: 5_000 });

    const value = await customerPoDateInput.inputValue();
    expect(value, 'Null customerPoDate from API must default to today in the edit form').toBe(today);
  });

  test('edit disabled on locked PO — Edit button is disabled in list', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-LOCKED-EDIT');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', poPayload(companyId, poNumber));
    const poCode = (createResp.data as { poCode: number }).poCode;

    // Lock it via API
    await apiRequest(apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/lock?companyId=${companyId}`, null);

    await page.goto('/transactions/purchase-order');
    await page.waitForLoadState('networkidle');

    // Verify the row is visible (main scrollable container)
    const poRow = page.locator(`[role="row"]:has-text("${poNumber}")`);
    await expect(poRow).toBeVisible({ timeout: 8_000 });

    // AG Grid pinned-right container holds action buttons — use row-id to find across containers
    const editBtn = page.locator(`[row-id="${poCode}"] button[title="Edit"]`).first();
    await expect(editBtn).toBeDisabled();
  });
});
