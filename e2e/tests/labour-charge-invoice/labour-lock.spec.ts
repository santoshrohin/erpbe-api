/**
 * Labour Charge Invoice — Lock / Unlock workflow
 *
 * Parity:
 *   Legacy: INVOICE_MASTER.MODIFY = 1 on lock, 0 on unlock
 *   New:    ERP_LockInvoice / ERP_UnlockInvoice SPs (shared with Tax Invoice)
 *           filtered by INM_TYPE='OutJWINM'
 *
 * Tests:
 *   - Lock → MODIFY=true in DB
 *   - Unlock → MODIFY=false in DB
 *   - Cannot delete locked invoice
 *   - Cannot edit locked invoice (also covered in labour-edit.spec.ts)
 *   - Full lifecycle: lock → unlock → delete succeeds
 *
 * API uses companyCode (NOT companyId).
 */

import { test, expect } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const PO_CODE = 9001;

async function createLabourInvoice(
  apiUrl: string,
  accessToken: string,
  companyId: number
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
    companyCode: companyId,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    details: [{ invoiceQuantity: 5, rate: 400, amount: 2000 }],
  });
  if (resp.status !== 201) throw new Error(`Labour invoice create failed: ${resp.status}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Labour Charge Invoice — Lock / Unlock', () => {

  test('lock invoice → INVOICE_MASTER.MODIFY=true', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId);

    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST',
      `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    const row = await db.queryOne<{ MODIFY: boolean | number }>(
      `SELECT MODIFY FROM INVOICE_MASTER WHERE INM_CODE = @code AND INM_TYPE = 'OutJWINM'`,
      { code: invoiceCode }
    );
    expect(row, 'INVOICE_MASTER row must exist').not.toBeNull();
    expect(row!.MODIFY, 'MODIFY must be true after lock').toBeTruthy();
  });

  test('unlock invoice → INVOICE_MASTER.MODIFY=false', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId);

    // Lock first
    await apiRequest(
      apiUrl, accessToken, 'POST',
      `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null
    );

    // Unlock
    const unlockResp = await apiRequest(
      apiUrl, accessToken, 'POST',
      `/LabourChargeInvoice/${invoiceCode}/unlock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(unlockResp.status);

    const row = await db.queryOne<{ MODIFY: boolean | number }>(
      `SELECT MODIFY FROM INVOICE_MASTER WHERE INM_CODE = @code AND INM_TYPE = 'OutJWINM'`,
      { code: invoiceCode }
    );
    expect(row!.MODIFY, 'MODIFY must be false after unlock').toBeFalsy();
  });

  test('cannot delete locked invoice → API 4xx, ES_DELETE remains false', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId);

    // Lock
    await apiRequest(
      apiUrl, accessToken, 'POST',
      `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null
    );

    // Delete blocked
    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE',
      `/LabourChargeInvoice/${invoiceCode}?companyCode=${companyId}`, undefined
    );
    expect(deleteResp.status, 'Delete of locked invoice must return 400 or 409').toBeGreaterThanOrEqual(400);
    expect(deleteResp.status).toBeLessThan(500);

    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM INVOICE_MASTER WHERE INM_CODE = @code AND INM_TYPE = 'OutJWINM'`,
      { code: invoiceCode }
    );
    expect(row!.ES_DELETE, 'ES_DELETE must remain false — delete was blocked').toBeFalsy();
  });

  test('full lifecycle: lock → unlock → delete succeeds', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createLabourInvoice(apiUrl, accessToken, companyId);

    await apiRequest(
      apiUrl, accessToken, 'POST',
      `/LabourChargeInvoice/${invoiceCode}/lock?companyCode=${companyId}`, null
    );
    await apiRequest(
      apiUrl, accessToken, 'POST',
      `/LabourChargeInvoice/${invoiceCode}/unlock?companyCode=${companyId}`, null
    );

    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE',
      `/LabourChargeInvoice/${invoiceCode}?companyCode=${companyId}`, undefined
    );
    expect([200, 204], 'Delete after unlock must succeed').toContain(deleteResp.status);

    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM INVOICE_MASTER WHERE INM_CODE = @code`,
      { code: invoiceCode }
    );
    expect(row!.ES_DELETE, 'ES_DELETE must be true after successful delete').toBeTruthy();
  });

  test('lock does not affect Tax Invoice rows — INM_TYPE isolation', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    // Create a Labour invoice and lock it
    const labourCode = await createLabourInvoice(apiUrl, accessToken, companyId);
    await apiRequest(
      apiUrl, accessToken, 'POST',
      `/LabourChargeInvoice/${labourCode}/lock?companyCode=${companyId}`, null
    );

    // Query: only rows with INM_TYPE='OutJWINM' should be locked — verify it didn't touch TAXINV rows
    const wronglyLocked = await db.queryOne<{ cnt: number }>(
      `SELECT COUNT(*) AS cnt
       FROM INVOICE_MASTER
       WHERE INM_CODE = @code AND INM_TYPE = 'TAXINV' AND MODIFY = 1`,
      { code: labourCode }
    );
    expect(
      wronglyLocked?.cnt ?? 0,
      'Locking a Labour invoice must not affect Tax Invoice rows (INM_TYPE isolation)'
    ).toBe(0);
  });
});
