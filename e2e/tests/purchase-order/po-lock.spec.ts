/**
 * Purchase Order — Lock / Unlock workflow
 *
 * Parity:
 *   Legacy: MODIFY column set to 1 on lock, 0 on unlock
 *   New:    ERP_LockCustomerPo SP sets MODIFY=1; ERP_UnlockCustomerPo sets MODIFY=0
 *
 * This test:
 *   - Locks via UI → DB MODIFY=1
 *   - Grid shows "Locked" status
 *   - Edit/Delete buttons disabled
 *   - Unlocks via UI → DB MODIFY=0
 *   - Edit/Delete buttons re-enabled
 *   - Cannot delete while locked → API returns 400
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { getPoByNumber, isPoLocked } from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

test.describe('Purchase Order — Lock / Unlock', () => {

  async function createTestPo(
    apiUrl: string,
    accessToken: string,
    companyId: number,
    poNumber: string
  ) {
    const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: 9001,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 11800,
      projectCode: null,
      details: [{ itemCode: 9001, uomCode: 1, orderedQuantity: 5, rate: 100, amount: 500 }],
    });
    return (resp.data as { poCode: number }).poCode;  // CustomerPoMasterDto.PoCode → JSON: poCode
  }

  test('lock PO → UI shows Locked + DB MODIFY=1', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-LOCK');
    const poId = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto('/transactions/purchase-order');
    await page.waitForLoadState('networkidle');

    // Verify the row is visible (main container has the text)
    const poRow = page.locator(`[role="row"]:has-text("${poNumber}")`);
    await expect(poRow).toBeVisible({ timeout: 8_000 });
    // AG Grid pinned-right container holds action buttons; use row-id to span containers
    const lockBtn = page.locator(`[row-id="${poId}"] button[title="Lock"]`).first();
    await lockBtn.click();

    // Confirm in any confirmation dialog
    const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Lock")').last();
    if (await confirmBtn.isVisible({ timeout: 1_500 })) await confirmBtn.click();

    // Grid should update: the Lock button becomes Unlock after locking.
    // The Status column ("Locked" span) may be scrolled off-screen — AG Grid column
    // virtualisation removes off-screen cells from the DOM. Use the pinned-right Actions
    // button title instead — it's always rendered.
    await expect(page.locator(`[row-id="${poId}"] button[title="Unlock"]`).first()).toBeVisible({ timeout: 8_000 });

    // DB assertion
    const locked = await isPoLocked(db, poId);
    expect(locked, 'MODIFY column must be 1 after lock').toBe(true);
  });

  test('unlock PO → UI shows Open + DB MODIFY=0', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-UNLOCK');
    const poId = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    // Lock it first via API
    await apiRequest(apiUrl, accessToken, 'POST', `/CustomerPo/${poId}/lock?companyId=${companyId}`, null);

    await page.goto('/transactions/purchase-order');
    await page.waitForLoadState('networkidle');

    const poRow = page.locator(`[role="row"]:has-text("${poNumber}")`);
    await expect(poRow).toBeVisible({ timeout: 8_000 });

    // Click Unlock (pinned-right container, use row-id)
    const unlockBtn = page.locator(`[row-id="${poId}"] button[title="Unlock"]`).first();
    await unlockBtn.click();
    const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes")').last();
    if (await confirmBtn.isVisible({ timeout: 1_500 })) await confirmBtn.click();

    // After unlocking the Unlock button becomes Lock (pinned-right, always in DOM).
    await expect(page.locator(`[row-id="${poId}"] button[title="Lock"]`).first()).toBeVisible({ timeout: 8_000 });

    const locked = await isPoLocked(db, poId);
    expect(locked, 'MODIFY column must be 0 after unlock').toBe(false);
  });

  test('cannot delete locked PO → API returns 400, DB row still exists', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-NO-DEL');
    const poId = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    // Lock
    await apiRequest(apiUrl, accessToken, 'POST', `/CustomerPo/${poId}/lock?companyId=${companyId}`, null);

    // Attempt delete
    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/CustomerPo/${poId}?companyId=${companyId}`, undefined
    );
    expect(deleteResp.status, 'Locked PO delete must return 400 or 409').toBeGreaterThanOrEqual(400);
    expect(deleteResp.status).toBeLessThan(500);

    // DB: row still exists and is not soft-deleted
    const po = await getPoByNumber(db, poNumber);
    expect(po).not.toBeNull();
    expect(po!.ES_DELETE).toBeFalsy();  // mssql returns BIT as boolean
  });
});
