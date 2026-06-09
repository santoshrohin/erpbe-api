/**
 * Tax Invoice — Supplementary Invoice
 *
 * Parity:
 *   Legacy: INVOICE_MASTER.INM_SUPPLEMENTORY BIT marks supplementary invoices.
 *   New:    Same flag + INM_PARENT_CODE INT stores explicit parent reference.
 *
 * Business rules (legacy parity):
 *   - Supplementary invoice is a separate INVOICE_MASTER row with INM_SUPPLEMENTORY=1
 *   - INM_PARENT_CODE stores the original invoice's INM_CODE
 *   - Stock IS deducted (supplementary invoices are real shipment documents)
 *   - Deleting a supplementary does NOT affect the original
 *   - Locking/unlocking is independent per invoice
 *
 * These tests have NO skip guards. All 4 must pass against the live API.
 * If any test fails, it indicates a real implementation gap.
 */

import { test, expect } from '../../fixtures';
import {
  getTaxInvoiceByCode,
  getTaxInvoiceDetails,
  getStockEntriesForDoc,
  isTaxInvoiceLocked,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const COMPANY_CODE = 1;
const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const STORE_CODE = 1;
const UOM_CODE = 1;
const PO_CODE = 9001;

async function createOriginalInvoice(
  apiUrl: string,
  accessToken: string,
  qty = 5
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    invoiceDetails: [
      {
        itemCode: ITEM_CODE,
        uomCode: UOM_CODE,
        invoiceQuantity: qty,
        rate: 1000,
        storeCode: STORE_CODE,
        cgstPercentage: 9,
        sgstPercentage: 9,
      },
    ],
  });
  if (resp.status !== 201) throw new Error(`Original invoice create failed: ${resp.status} — ${JSON.stringify(resp.data)}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

async function createSupplementaryInvoice(
  apiUrl: string,
  accessToken: string,
  parentInvoiceCode: number,
  qty = 1
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    isSupplementary: true,
    parentInvoiceCode,
    invoiceDetails: [
      {
        itemCode: ITEM_CODE,
        uomCode: UOM_CODE,
        invoiceQuantity: qty,
        rate: 500,
        storeCode: STORE_CODE,
        cgstPercentage: 9,
        sgstPercentage: 9,
      },
    ],
  });
  if (resp.status !== 201) throw new Error(`Supplementary invoice create failed: ${resp.status} — ${JSON.stringify(resp.data)}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Tax Invoice — Supplementary Invoice', () => {

  test('create supplementary invoice → INM_SUPPLEMENTORY=1 + INM_PARENT_CODE=originalCode in DB', async ({
    db, apiUrl, accessToken,
  }) => {
    const originalCode = await createOriginalInvoice(apiUrl, accessToken, 5);
    const suppCode = await createSupplementaryInvoice(apiUrl, accessToken, originalCode, 1);

    // Supplementary row must exist and not be deleted
    const suppRow = await db.queryOne<{
      INM_SUPPLEMENTORY: boolean | number;
      INM_PARENT_CODE: number | null;
      INM_TYPE: string | null;
      ES_DELETE: boolean | number;
    }>(
      `SELECT INM_SUPPLEMENTORY, INM_PARENT_CODE, INM_TYPE, ES_DELETE
       FROM INVOICE_MASTER WHERE INM_CODE = @suppCode`,
      { suppCode }
    );

    expect(suppRow, 'Supplementary INVOICE_MASTER row must exist').not.toBeNull();
    expect(suppRow!.ES_DELETE, 'Supplementary invoice must not be deleted').toBeFalsy();
    expect(suppRow!.INM_SUPPLEMENTORY, 'INM_SUPPLEMENTORY must be 1').toBeTruthy();
    expect(suppRow!.INM_PARENT_CODE, 'INM_PARENT_CODE must equal original invoice code').toBe(originalCode);
    expect(suppRow!.INM_TYPE, 'INM_TYPE must be TAXINV').toBe('TAXINV');
  });

  test('supplementary invoice deducts stock independently of original', async ({
    db, apiUrl, accessToken,
  }) => {
    test.setTimeout(60_000);
    const originalCode = await createOriginalInvoice(apiUrl, accessToken, 3);

    // Verify original deducted stock
    const originalStock = await getStockEntriesForDoc(db, originalCode, 'TAXINV');
    expect(originalStock, 'Original invoice must deduct stock').toBeLessThan(0);

    // Create supplementary — deducts additional stock
    const suppCode = await createSupplementaryInvoice(apiUrl, accessToken, originalCode, 2);

    const suppStock = await getStockEntriesForDoc(db, suppCode, 'TAXINV');
    expect(suppStock, 'Supplementary invoice must deduct stock (not restore original)').toBeLessThan(0);

    // Original stock entry unchanged
    const originalStockAfter = await getStockEntriesForDoc(db, originalCode, 'TAXINV');
    expect(originalStockAfter, 'Original stock entry must be unchanged after supplementary created').toBe(originalStock);
  });

  test('original invoice unaffected after supplementary created — details unchanged', async ({
    db, apiUrl, accessToken,
  }) => {
    const originalCode = await createOriginalInvoice(apiUrl, accessToken, 4);

    // Snapshot original details before supplementary
    const originalDetailsBefore = await getTaxInvoiceDetails(db, originalCode);
    expect(originalDetailsBefore.length, 'Original invoice must have 1 detail row').toBe(1);
    const originalQty = originalDetailsBefore[0].IND_INQTY;

    // Create supplementary
    await createSupplementaryInvoice(apiUrl, accessToken, originalCode, 1);

    // Original must be unchanged
    const originalRow = await getTaxInvoiceByCode(db, originalCode);
    expect(originalRow, 'Original invoice must still exist').not.toBeNull();
    expect(originalRow!.ES_DELETE, 'Original invoice must not be soft-deleted').toBeFalsy();

    const originalDetailsAfter = await getTaxInvoiceDetails(db, originalCode);
    expect(originalDetailsAfter.length, 'Original invoice detail count must be unchanged').toBe(1);
    expect(originalDetailsAfter[0].IND_INQTY, 'Original invoice qty must remain unchanged').toBe(originalQty);
  });

  test('delete supplementary does not affect original — original survives', async ({
    db, apiUrl, accessToken,
  }) => {
    const originalCode = await createOriginalInvoice(apiUrl, accessToken, 2);
    const suppCode = await createSupplementaryInvoice(apiUrl, accessToken, originalCode, 1);

    // Delete supplementary
    const deleteResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/TaxInvoice/${suppCode}?companyId=${COMPANY_CODE}`, undefined
    );
    expect([200, 204], 'Supplementary delete must succeed').toContain(deleteResp.status);

    // Supplementary must be soft-deleted
    const suppRow = await getTaxInvoiceByCode(db, suppCode);
    expect(suppRow, 'Supplementary row must still exist in DB (soft delete)').not.toBeNull();
    expect(suppRow!.ES_DELETE, 'Supplementary invoice must be soft-deleted').toBeTruthy();

    // Original must be completely unaffected
    const originalRow = await getTaxInvoiceByCode(db, originalCode);
    expect(originalRow, 'Original invoice must still exist after supplementary deleted').not.toBeNull();
    expect(originalRow!.ES_DELETE, 'Original invoice must NOT be soft-deleted').toBeFalsy();
  });
});
