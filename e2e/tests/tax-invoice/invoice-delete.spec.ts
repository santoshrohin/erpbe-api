/**
 * Tax Invoice — Delete workflow
 *
 * Parity: Legacy soft-deletes (ES_DELETE=1), DB row persists.
 * Locked invoice cannot be deleted.
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

const COMPANY_CODE = 1;
const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const UOM_CODE = 1;
const PO_CODE = 9001;
const STORE_CODE = 1;

async function createInvoice(apiUrl: string, accessToken: string): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    invoiceDetails: [
      { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 300, storeCode: STORE_CODE },
    ],
  });
  if (resp.status !== 201) throw new Error(`Invoice create failed: ${resp.status}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Tax Invoice — Delete', () => {

  test('delete unlocked invoice → API 200/204 + row soft-deleted in DB', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/TaxInvoice/${invoiceCode}?companyId=${companyId}`, undefined
    );
    expect([200, 204]).toContain(deleteResp.status);

    // Verify soft-delete (record still in DB, ES_DELETE=1)
    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM INVOICE_MASTER WHERE INM_CODE = @code`,
      { code: invoiceCode }
    );
    expect(row, 'Row must still exist after soft-delete').not.toBeNull();
    expect(row!.ES_DELETE).toBeTruthy();  // mssql returns BIT as boolean
  });

  test('delete locked invoice → API returns 400/409', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    // Lock
    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/TaxInvoice/${invoiceCode}/lock?companyId=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    // Delete locked — must fail
    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/TaxInvoice/${invoiceCode}?companyId=${companyId}`, undefined
    );
    expect(
      deleteResp.status,
      'Deleting a locked invoice must return 400 or 409'
    ).toBeGreaterThanOrEqual(400);
    expect(deleteResp.status).toBeLessThan(500);
  });
});
