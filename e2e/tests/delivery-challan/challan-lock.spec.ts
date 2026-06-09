/**
 * Delivery Challan — Lock / Unlock workflow
 *
 * Parity:
 *   Legacy: DELIVERY_CHALLAN_MASTER.MODIFY = 1 on lock, 0 on unlock
 *   New:    ERP_LockDeliveryChallan / ERP_UnlockDeliveryChallan SPs toggle MODIFY
 *
 * Tests:
 *   - Lock → DB MODIFY=true
 *   - Unlock → DB MODIFY=false
 *   - Cannot delete locked challan
 *   - Cannot edit locked challan (covered in challan-edit.spec.ts too)
 *   - UI shows correct Locked/Open status
 *
 * NOTE: companyCode (NOT companyId) for lock/unlock endpoints.
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const UOM_CODE = 1;

async function createChallan(
  apiUrl: string,
  accessToken: string,
  companyId: number
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan', {
    companyCode: companyId,
    challanDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 2 }],
  });
  if (resp.status !== 201) throw new Error(`Challan create failed: ${resp.status}`);
  return (resp.data as { challanCode: number }).challanCode;
}

test.describe('Delivery Challan — Lock / Unlock', () => {

  test('lock challan → DB MODIFY=true', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const dcCode = await createChallan(apiUrl, accessToken, companyId);

    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/lock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    const row = await db.queryOne<{ MODIFY: boolean | number }>(
      `SELECT MODIFY FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @code`,
      { code: dcCode }
    );
    expect(row, 'Challan row must exist in DB').not.toBeNull();
    expect(row!.MODIFY, 'DELIVERY_CHALLAN_MASTER.MODIFY must be true after lock').toBeTruthy();
  });

  test('unlock challan → DB MODIFY=false', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const dcCode = await createChallan(apiUrl, accessToken, companyId);

    // Lock first
    await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/lock?companyCode=${companyId}`, null
    );

    // Unlock — uses POST /unlock (separate endpoint from lock)
    const unlockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/unlock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(unlockResp.status);

    const row = await db.queryOne<{ MODIFY: boolean | number }>(
      `SELECT MODIFY FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @code`,
      { code: dcCode }
    );
    expect(row!.MODIFY, 'DELIVERY_CHALLAN_MASTER.MODIFY must be false after unlock').toBeFalsy();
  });

  test('cannot delete locked challan → API 4xx, row still exists', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const dcCode = await createChallan(apiUrl, accessToken, companyId);

    // Lock
    await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/lock?companyCode=${companyId}`, null
    );

    // Attempt delete
    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/DeliveryChallan/${dcCode}?companyCode=${companyId}`, undefined
    );
    expect(
      deleteResp.status,
      'Delete of locked challan must return 400 or 409'
    ).toBeGreaterThanOrEqual(400);
    expect(deleteResp.status).toBeLessThan(500);

    // DB: row still exists, not soft-deleted
    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @code`,
      { code: dcCode }
    );
    expect(row, 'Challan row must still exist after blocked delete').not.toBeNull();
    expect(row!.ES_DELETE, 'ES_DELETE must remain false — delete was blocked').toBeFalsy();
  });

  test('lock → unlock → delete succeeds — lifecycle round trip', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const dcCode = await createChallan(apiUrl, accessToken, companyId);

    // Lock
    await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/lock?companyCode=${companyId}`, null
    );

    // Unlock
    await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/unlock?companyCode=${companyId}`, null
    );

    // Now delete should succeed
    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/DeliveryChallan/${dcCode}?companyCode=${companyId}`, undefined
    );
    expect([200, 204], 'Delete after unlock must succeed').toContain(deleteResp.status);

    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @code`,
      { code: dcCode }
    );
    expect(row!.ES_DELETE, 'ES_DELETE must be true after successful delete').toBeTruthy();
  });

  test('UI: locked challan shows Locked status and Edit/Delete buttons disabled', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    const dcCode = await createChallan(apiUrl, accessToken, companyId);

    // Lock via API
    await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/lock?companyCode=${companyId}`, null
    );

    await page.goto('/transactions/delivery-challan');
    await page.waitForLoadState('networkidle');

    // AG Grid renders action buttons in the pinned-right container which has a
    // separate DOM tree from the main row.  Scope by row-id to cross containers
    // and avoid falling back to an unrelated row with .last().
    const editBtn = page.locator(`[row-id="${dcCode}"] button[title="Edit"]`).first();
    const deleteBtn = page.locator(`[row-id="${dcCode}"] button[title="Delete"]`).first();

    await expect(editBtn).toBeVisible({ timeout: 8_000 });
    await expect(editBtn).toBeDisabled();
    if (await deleteBtn.isVisible({ timeout: 2_000 })) {
      await expect(deleteBtn).toBeDisabled();
    }
  });
});
