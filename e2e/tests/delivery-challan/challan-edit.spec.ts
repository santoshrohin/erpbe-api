/**
 * Delivery Challan — Edit workflow
 *
 * Parity gaps covered:
 *   - Detail lines are replaced on edit (ERP_DeleteDeliveryChallanDetail + re-insert).
 *     No orphan rows must remain in DELIVERY_CHALLAN_DETAIL after edit.
 *   - STOCK_LEDGER must be adjusted: old DCOUT entry removed, new one inserted.
 *     Double-deduction bug would appear here if DELETE step is missing from SP.
 *   - Locked challan cannot be edited.
 *
 * Stock mechanism:
 *   Create → adds DCOUT row (negative qty)
 *   Edit   → removes old DCOUT rows for this DCM_CODE, inserts new DCOUT row
 *   If SP only inserts (no delete): stock is double-deducted on every edit.
 *
 * Column names (CRITICAL):
 *   DELIVERY_CHALLAN_DETAIL: DCD_DCM_CODE, DCD_I_CODE, DCD_ORD_QTY, DCD_UM_CODE
 *   STOCK_LEDGER:            STL_DOC_NO, STL_DOC_TYPE='DCOUT', STL_DOC_QTY
 */

import { test, expect } from '../../fixtures';
import {
  getChallanDetails,
  getStockEntriesForDoc,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const ITEM_CODE_2 = 9002;
const UOM_CODE = 1;

async function createChallan(
  apiUrl: string,
  accessToken: string,
  companyId: number,
  qty = 3,
  itemCode = ITEM_CODE
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan', {
    companyCode: companyId,
    challanDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    details: [{ itemCode, uomCode: UOM_CODE, orderedQuantity: qty }],
  });
  if (resp.status !== 201) {
    throw new Error(`Challan create failed: ${resp.status} — ${JSON.stringify(resp.data)}`);
  }
  return (resp.data as { challanCode: number }).challanCode;
}

test.describe('Delivery Challan — Edit', () => {

  test('edit challan → DELIVERY_CHALLAN_DETAIL replaced with new quantity', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const originalQty = 3;
    const updatedQty = 7;

    const dcCode = await createChallan(apiUrl, accessToken, companyId, originalQty);

    // Verify original detail
    const detailsBefore = await getChallanDetails(db, dcCode);
    expect(detailsBefore.length, 'Must have 1 detail row after create').toBe(1);
    expect(detailsBefore[0].DCD_ORD_QTY).toBe(originalQty);

    // Edit — change quantity
    const updateResp = await apiRequest(
      apiUrl, accessToken, 'PUT', `/DeliveryChallan/${dcCode}`, {
        challanCode: dcCode,
        companyCode: companyId,
        challanDate: new Date().toISOString(),
        customerCode: CUSTOMER_CODE,
        details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: updatedQty }],
      }
    );
    expect(
      [200, 204],
      `PUT /DeliveryChallan/${dcCode} failed with ${updateResp.status}: ${JSON.stringify(updateResp.data)}`
    ).toContain(updateResp.status);

    // DELIVERY_CHALLAN_DETAIL: 1 row with new quantity, no orphans
    const detailsAfter = await getChallanDetails(db, dcCode);
    expect(detailsAfter.length, 'Must have exactly 1 detail row after edit (orphans removed)').toBe(1);
    expect(detailsAfter[0].DCD_ORD_QTY, 'DCD_ORD_QTY must reflect updated quantity').toBe(updatedQty);
  });

  test('edit challan → stock ledger correctly adjusted (no double-deduction)', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const originalQty = 4;
    const updatedQty = 2;

    const dcCode = await createChallan(apiUrl, accessToken, companyId, originalQty);

    // After create: DCOUT entry must be -originalQty
    const stockAfterCreate = await getStockEntriesForDoc(db, dcCode, 'DCOUT');
    expect(stockAfterCreate, `STOCK_LEDGER must show -${originalQty} after create`).toBe(-originalQty);

    // Edit — reduce quantity
    await apiRequest(apiUrl, accessToken, 'PUT', `/DeliveryChallan/${dcCode}`, {
      challanCode: dcCode,
      companyCode: companyId,
      challanDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: updatedQty }],
    });

    // After edit: DCOUT entry must show -updatedQty ONLY (old entry reversed)
    const stockAfterEdit = await getStockEntriesForDoc(db, dcCode, 'DCOUT');
    expect(
      stockAfterEdit,
      `STOCK_LEDGER must show -${updatedQty} after edit. ` +
      `If it shows -${originalQty + updatedQty}, the update SP has a double-deduction bug.`
    ).toBe(-updatedQty);
  });

  test('edit replaces multi-item detail — correct items and quantities after edit', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    // Create with 1 item
    const dcCode = await createChallan(apiUrl, accessToken, companyId, 5, ITEM_CODE);

    const detailsBefore = await getChallanDetails(db, dcCode);
    expect(detailsBefore.length).toBe(1);

    // Edit — swap to 2 items
    const updateResp = await apiRequest(
      apiUrl, accessToken, 'PUT', `/DeliveryChallan/${dcCode}`, {
        challanCode: dcCode,
        companyCode: companyId,
        challanDate: new Date().toISOString(),
        customerCode: CUSTOMER_CODE,
        details: [
          { itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 3 },
          { itemCode: ITEM_CODE_2, uomCode: UOM_CODE, orderedQuantity: 2 },
        ],
      }
    );
    expect([200, 204]).toContain(updateResp.status);

    const detailsAfter = await getChallanDetails(db, dcCode);
    expect(detailsAfter.length, 'Must have 2 detail rows after edit with 2 items').toBe(2);

    const itemCodes = detailsAfter.map(d => d.DCD_I_CODE).sort();
    expect(itemCodes).toEqual([ITEM_CODE, ITEM_CODE_2].sort());

    const item1 = detailsAfter.find(d => d.DCD_I_CODE === ITEM_CODE)!;
    expect(item1.DCD_ORD_QTY).toBe(3);
    const item2 = detailsAfter.find(d => d.DCD_I_CODE === ITEM_CODE_2)!;
    expect(item2.DCD_ORD_QTY).toBe(2);
  });

  test('edit locked challan → API returns 4xx, detail unchanged', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const originalQty = 3;
    const dcCode = await createChallan(apiUrl, accessToken, companyId, originalQty);

    // Lock it
    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/lock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    // Attempt edit on locked challan
    const updateResp = await apiRequest(
      apiUrl, accessToken, 'PUT', `/DeliveryChallan/${dcCode}`, {
        challanCode: dcCode,
        companyCode: companyId,
        challanDate: new Date().toISOString(),
        customerCode: CUSTOMER_CODE,
        details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 99 }],
      }
    );
    expect(
      updateResp.status,
      'Edit of locked challan must return 4xx'
    ).toBeGreaterThanOrEqual(400);
    expect(updateResp.status).toBeLessThan(500);

    // DB: quantity must remain unchanged
    const details = await getChallanDetails(db, dcCode);
    expect(details[0].DCD_ORD_QTY, 'Locked challan detail must not be modified').toBe(originalQty);
  });
});
