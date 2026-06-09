/**
 * Purchase Order — Delete workflow
 *
 * Parity:
 *   Legacy: ES_DELETE = 1 (soft delete), record preserved
 *   New:    ERP_DeleteCustomerPo SP sets ES_DELETE = 1
 *
 * Tests:
 *   - Delete unlocked PO → grid removes it → DB ES_DELETE=1
 *   - Delete locked PO → blocked (covered in po-lock.spec.ts)
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { getPoByNumber, isPoDeleted } from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

test.describe('Purchase Order — Delete', () => {

  test('delete unlocked PO → removed from grid + ES_DELETE=1 in DB', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-DEL');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: 9001,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 5000,
      projectCode: null,
      details: [{ itemCode: 9001, uomCode: 1, orderedQuantity: 5, rate: 1000, amount: 5000 }],
    });
    expect(createResp.status).toBe(201);
    const poId = (createResp.data as { poCode: number }).poCode;  // CustomerPoMasterDto → JSON: poCode

    await page.goto('/transactions/purchase-order');
    await page.waitForLoadState('networkidle');

    // Verify the row is visible (main scrollable container has the text)
    const poRow = page.locator(`[role="row"]:has-text("${poNumber}")`);
    await expect(poRow).toBeVisible({ timeout: 8_000 });

    // AG Grid pinned columns are in a separate DOM container — use row-id to find the button
    // across both the main container and the pinned-right container.
    const deleteBtn = page.locator(`[row-id="${poId}"] button[title="Delete"]`).first();
    await deleteBtn.click();

    // Confirm deletion dialog
    const confirmBtn = page.locator('button:has-text("Delete"), button:has-text("Confirm")').last();
    await expect(confirmBtn).toBeVisible({ timeout: 3_000 });
    await confirmBtn.click();

    // Row disappears from grid
    await expect(poRow).not.toBeVisible({ timeout: 8_000 });

    // DB: soft-deleted (ES_DELETE=1), row still exists
    const softDeleted = await isPoDeleted(db, poId);
    expect(softDeleted, 'PO must be soft-deleted (ES_DELETE=1), not hard-deleted').toBe(true);
  });

  test('delete via API returns 204 → ES_DELETE=1 in DB', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-API-DEL');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: 9001,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 2500,
      projectCode: null,
      details: [{ itemCode: 9001, uomCode: 1, orderedQuantity: 2, rate: 1250, amount: 2500 }],
    });
    const poId = (createResp.data as { poCode: number }).poCode;  // CustomerPoMasterDto → JSON: poCode

    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/CustomerPo/${poId}?companyId=${companyId}`, undefined
    );
    expect([200, 204]).toContain(deleteResp.status);

    const softDeleted = await isPoDeleted(db, poId);
    expect(softDeleted).toBe(true);
  });
});
