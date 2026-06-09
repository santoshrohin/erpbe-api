/**
 * Tax Invoice — Edit (Update) workflow
 *
 * Parity gaps this test proves:
 *   1. INVOICE_DETAIL rows are replaced on edit (old rows deleted, new rows inserted).
 *      Legacy behavior: ERP_DeleteInvoiceDetails (hard-delete) + ERP_CreateTaxInvoiceDetail.
 *   2. STOCK_LEDGER is correctly adjusted: old deduction removed, new deduction inserted.
 *      If this fails → stock is double-deducted on every edit (critical inventory bug).
 *   3. Locked invoice cannot be edited (API must return 4xx).
 *   4. No orphan INVOICE_DETAIL rows remain after edit.
 *
 * Stock reversal mechanism (ERP_ManageTaxInvoiceStock):
 *   - 'INSERT' operation: adds negative STL_DOC_QTY row (stock OUT)
 *   - 'DELETE' operation: removes rows WHERE STL_DOC_NO=invoiceCode AND STL_DOC_TYPE='TAXINV'
 *   - On update: old stock must be reversed BEFORE new detail is inserted.
 *     If the handler only calls INSERT (not DELETE then INSERT), stock is double-counted.
 */

import { test, expect } from '../../fixtures';
import {
  getTaxInvoiceByCode,
  getTaxInvoiceDetails,
  getStockEntriesForDoc,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const COMPANY_CODE = 1;
const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const STORE_CODE = 1;
const UOM_CODE = 1;
const PO_CODE = 9001;

async function createInvoice(
  apiUrl: string,
  accessToken: string,
  qty: number,
  itemCode = ITEM_CODE
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    invoiceDetails: [
      { itemCode, uomCode: UOM_CODE, invoiceQuantity: qty, rate: 1000, storeCode: STORE_CODE, cgstPercentage: 9, sgstPercentage: 9 },
    ],
  });
  if (resp.status !== 201) throw new Error(`Invoice create failed: ${resp.status} — ${JSON.stringify(resp.data)}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Tax Invoice — Edit', () => {

  test('edit invoice → INVOICE_DETAIL replaced with new quantity', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const originalQty = 5;
    const updatedQty = 3;

    const invoiceCode = await createInvoice(apiUrl, accessToken, originalQty);

    // Verify original detail
    const detailsBefore = await getTaxInvoiceDetails(db, invoiceCode);
    expect(detailsBefore.length).toBe(1);
    expect(detailsBefore[0].IND_INQTY).toBe(originalQty);

    // Edit — change quantity
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/TaxInvoice/${invoiceCode}`, {
      invoiceCode,
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [
        { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: updatedQty, rate: 1000, storeCode: STORE_CODE, cgstPercentage: 9, sgstPercentage: 9 },
      ],
    });
    expect(
      [200, 204],
      `PUT /TaxInvoice/${invoiceCode} failed with ${updateResp.status}: ${JSON.stringify(updateResp.data)}`
    ).toContain(updateResp.status);

    // INVOICE_DETAIL: exactly 1 row with new quantity (no orphans)
    const detailsAfter = await getTaxInvoiceDetails(db, invoiceCode);
    expect(detailsAfter.length, 'Must have exactly 1 INVOICE_DETAIL row (old row deleted, new inserted)').toBe(1);
    expect(
      detailsAfter[0].IND_INQTY,
      'Quantity must be updated to new value'
    ).toBe(updatedQty);
  });

  test('edit invoice → stock ledger correctly adjusted (no double-deduction)', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const originalQty = 5;
    const updatedQty = 3;

    const invoiceCode = await createInvoice(apiUrl, accessToken, originalQty);

    // Verify original stock deduction
    const stockAfterCreate = await getStockEntriesForDoc(db, invoiceCode, 'TAXINV');
    expect(
      stockAfterCreate,
      `After create: STOCK_LEDGER must show -${originalQty} for this invoice`
    ).toBe(-originalQty);

    // Edit — reduce quantity
    await apiRequest(apiUrl, accessToken, 'PUT', `/TaxInvoice/${invoiceCode}`, {
      invoiceCode,
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [
        { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: updatedQty, rate: 1000, storeCode: STORE_CODE, cgstPercentage: 9, sgstPercentage: 9 },
      ],
    });

    // STOCK_LEDGER: must reflect ONLY the updated quantity, not original+updated
    const stockAfterEdit = await getStockEntriesForDoc(db, invoiceCode, 'TAXINV');
    expect(
      stockAfterEdit,
      `After edit: STOCK_LEDGER must show -${updatedQty} (old entry reversed). ` +
      `If it shows -${originalQty + updatedQty}, the update handler has a double-deduction bug.`
    ).toBe(-updatedQty);
  });

  test('edit cannot change locked invoice — API returns 4xx', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken, 5);

    // Lock it
    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/TaxInvoice/${invoiceCode}/lock?companyId=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    // Try to edit locked invoice
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/TaxInvoice/${invoiceCode}`, {
      invoiceCode,
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [
        { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 99, rate: 1000, storeCode: STORE_CODE },
      ],
    });
    expect(
      updateResp.status,
      'Edit of locked invoice must return 4xx'
    ).toBeGreaterThanOrEqual(400);
    expect(updateResp.status).toBeLessThan(500);

    // Verify quantity was NOT changed
    const details = await getTaxInvoiceDetails(db, invoiceCode);
    expect(details[0].IND_INQTY, 'Locked invoice quantity must not be modified').toBe(5);
  });

  test('edit with 2 items → replaces all details, no orphan rows', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    // Create with 1 item
    const invoiceCode = await createInvoice(apiUrl, accessToken, 4);
    const detailsBefore = await getTaxInvoiceDetails(db, invoiceCode);
    expect(detailsBefore.length).toBe(1);

    // Edit — add second item
    const updateResp = await apiRequest(apiUrl, accessToken, 'PUT', `/TaxInvoice/${invoiceCode}`, {
      invoiceCode,
      companyCode: COMPANY_CODE,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      invoiceDetails: [
        { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 4, rate: 1000, storeCode: STORE_CODE, cgstPercentage: 9, sgstPercentage: 9 },
        { itemCode: 9002, uomCode: UOM_CODE, invoiceQuantity: 2, rate: 500, storeCode: STORE_CODE, cgstPercentage: 9, sgstPercentage: 9 },
      ],
    });
    expect([200, 204]).toContain(updateResp.status);

    const detailsAfter = await getTaxInvoiceDetails(db, invoiceCode);
    expect(detailsAfter.length, 'Must have exactly 2 INVOICE_DETAIL rows after adding second item').toBe(2);

    const itemCodes = detailsAfter.map(d => d.IND_I_CODE).sort();
    expect(itemCodes).toEqual([9001, 9002].sort());
  });
});
