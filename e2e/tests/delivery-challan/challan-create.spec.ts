/**
 * Delivery Challan — Create / Lock / Print / Delete
 *
 * Parity:
 *   - Challan writes to DELIVERY_CHALLAN_MASTER + DELIVERY_CHALLAN_DETAIL.
 *   - ERP_CreateDeliveryChallanDetail also inserts a negative STL_DOC_QTY row into
 *     STOCK_LEDGER (STL_DOC_TYPE='DCOUT') — so DC DOES deduct stock.
 *   - ERP_DeleteDeliveryChallan removes STOCK_LEDGER DCOUT rows — stock is restored on delete.
 *   - MODIFY column controls lock state (BIT → JS boolean via mssql v11).
 *   - ES_DELETE for soft delete (BIT → boolean).
 *
 * API query param name: companyCode (NOT companyId — different from TaxInvoice!)
 * Response body field:  challanCode  (NOT id — DeliveryChallanMasterDto.ChallanCode)
 * DB primary key col:   DCM_CODE     (NOT DC_CODE — was a named wrong)
 * DB company col:       DCM_CM_CODE  (NOT DC_CM_COMP_ID)
 */

import { test, expect } from '../../fixtures';
import {
  getLatestChallan,
  getChallanDetails,
  getStockEntriesForDoc,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const UOM_CODE = 1;

function challanPayload(companyId: number, overrides?: object) {
  return {
    companyCode: companyId,  // controller uses companyCode, not companyId
    challanDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    details: [
      {
        itemCode: ITEM_CODE,
        uomCode: UOM_CODE,
        orderedQuantity: 3,  // correct field in CreateDeliveryChallanDetailCommand
      },
    ],
    ...overrides,
  };
}

test.describe('Delivery Challan — Create', () => {

  test('creates challan → DCM_MASTER row + DETAIL row + stock deducted', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const challanQty = 3;

    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/DeliveryChallan', challanPayload(companyId, {
        details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: challanQty }],
      })
    );

    expect(createResp.status, `Expected 201, got ${createResp.status}: ${JSON.stringify(createResp.data)}`).toBe(201);
    const dcCode = (createResp.data as { challanCode: number }).challanCode;
    expect(dcCode, 'DELIVERY_CHALLAN_MASTER uses negative PKs from legacy identity seed — non-zero is valid').not.toBe(0);

    // DELIVERY_CHALLAN_MASTER
    const challan = await getLatestChallan(db, companyId);
    expect(challan, 'DELIVERY_CHALLAN_MASTER row must exist').not.toBeNull();
    expect(challan!.DCM_CM_CODE).toBe(companyId);
    expect(challan!.ES_DELETE).toBeFalsy();

    // DELIVERY_CHALLAN_DETAIL — line items must be persisted
    const details = await getChallanDetails(db, dcCode);
    expect(details.length, 'DELIVERY_CHALLAN_DETAIL must have 1 row').toBe(1);
    expect(details[0].DCD_I_CODE).toBe(ITEM_CODE);
    expect(details[0].DCD_ORD_QTY).toBe(challanQty);
    expect(details[0].ES_DELETE).toBeFalsy();

    // STOCK_LEDGER — ERP_CreateDeliveryChallanDetail inserts a DCOUT row
    const challanStock = await getStockEntriesForDoc(db, dcCode, 'DCOUT');
    expect(
      challanStock,
      `STOCK_LEDGER must show -${challanQty} for challan ${dcCode} (DCOUT entry)`
    ).toBe(-challanQty);

  });

  test('lock challan → MODIFY=true in DB', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/DeliveryChallan', challanPayload(companyId)
    );
    expect(createResp.status).toBe(201);
    const dcCode = (createResp.data as { challanCode: number }).challanCode;

    const lockResp = await apiRequest(
      apiUrl, accessToken, 'POST', `/DeliveryChallan/${dcCode}/lock?companyCode=${companyId}`, null
    );
    expect([200, 204]).toContain(lockResp.status);

    const row = await db.queryOne<{ MODIFY: boolean | number }>(
      `SELECT MODIFY FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @code`, { code: dcCode }
    );
    expect(row?.MODIFY).toBeTruthy();
  });

  test('print challan → PDF blob returned', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/DeliveryChallan', challanPayload(companyId)
    );
    expect(createResp.status).toBe(201);
    const dcCode = (createResp.data as { challanCode: number }).challanCode;

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET', `/DeliveryChallan/${dcCode}/print?companyCode=${companyId}`
    );
    expect(printResp.status).toBe(200);
    const buffer = printResp.data as ArrayBuffer;
    expect(buffer.byteLength).toBeGreaterThan(1024);
    const magic = String.fromCharCode(...new Uint8Array(buffer.slice(0, 4)));
    expect(magic).toBe('%PDF');
  });

  test('soft delete challan → ES_DELETE=true + stock restored in STOCK_LEDGER', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const challanQty = 2;

    const createResp = await apiRequest(
      apiUrl, accessToken, 'POST', '/DeliveryChallan', challanPayload(companyId, {
        details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: challanQty }],
      })
    );
    expect(createResp.status).toBe(201);
    const dcCode = (createResp.data as { challanCode: number }).challanCode;

    // Confirm stock was deducted (per-document: avoids race with other parallel tests)
    const stockMid = await getStockEntriesForDoc(db, dcCode, 'DCOUT');
    expect(stockMid, 'Stock must be deducted before delete').toBeLessThan(0);

    // Delete
    const delResp = await apiRequest(
      apiUrl, accessToken, 'DELETE', `/DeliveryChallan/${dcCode}?companyCode=${companyId}`, undefined
    );
    expect([200, 204]).toContain(delResp.status);

    // DELIVERY_CHALLAN_MASTER: soft-deleted
    const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
      `SELECT ES_DELETE FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @code`, { code: dcCode }
    );
    expect(row?.ES_DELETE).toBeTruthy();

    // STOCK_LEDGER: ERP_DeleteDeliveryChallan removes DCOUT rows → stock restored
    const challanStock = await getStockEntriesForDoc(db, dcCode, 'DCOUT');
    expect(
      challanStock,
      'STOCK_LEDGER DCOUT entries must be removed after challan delete (stock reversal)'
    ).toBe(0);
  });
});
