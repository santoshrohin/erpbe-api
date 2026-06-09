/**
 * Purchase Order — AMEND Workflow
 *
 * Parity with legacy CustomerPO.aspx.cs (AMEND path, lines 795-856):
 *   - POST /{id}/amend archives CUSTPO_MASTER → CUSTPO_AM_MASTER
 *   - POST /{id}/amend archives CUSTPO_DETAIL → CUSTPO_AMD_DETAIL
 *   - POST /{id}/amend increments CPOM_AM_COUNT and sets CPOM_AM_DATE
 *   - POST /{id}/amend releases the lock (MODIFY = 0)
 *   - PUT /{id}   (plain edit / MODIFY path) does NOT touch CPOM_AM_COUNT
 *
 * COLUMN NOTES:
 *   CUSTPO_AM_MASTER: CPOM_AM_CODE (IDENTITY PK), CPOM_CODE (FK → original PO)
 *   CUSTPO_AMD_DETAIL: AMD_AM_CODE (FK → CUSTPO_AM_MASTER.CPOM_AM_CODE)
 *   CPOM_AM_COUNT: NULL on create; set to 1 on first amend, 2 on second, …
 *   CPOM_AM_DATE:  NULL on create; set to NOW() on each amend
 */

import { test, expect, uniqueRef } from '../../fixtures';
import {
  getPoByCode,
  getPoDetails,
  getAmendmentArchives,
  getAmendmentArchiveDetails,
} from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE_1   = 9001;
const ITEM_CODE_2   = 9002;
const UOM_CODE      = 1;

async function createPo(
  apiUrl: string,
  accessToken: string,
  companyId: number,
  poNumber: string,
  overrides?: object
): Promise<number> {
  const now = new Date().toISOString();
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
    customerCode:   CUSTOMER_CODE,
    poNumber,
    poType:         1,
    poDate:         now,
    customerPoDate: now,
    creditDays:     30,
    companyId,
    grandTotal:     10000,
    paymentTerms:   'Net 30',
    details: [
      { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 10, rate: 1000, amount: 10000 },
    ],
    ...overrides,
  });
  if (resp.status !== 201) throw new Error(`PO create failed: HTTP ${resp.status}`);
  return (resp.data as { poCode: number }).poCode;
}

function amendBody(
  companyId: number,
  poNumber: string,
  paymentTerms: string,
  qty = 10,
  overrides?: object
) {
  return {
    customerCode:   CUSTOMER_CODE,
    poNumber,
    poType:         1,
    poDate:         new Date().toISOString(),
    customerPoDate: new Date().toISOString(),
    creditDays:     30,
    companyId,
    grandTotal:     qty * 1000,
    paymentTerms,
    details: [
      { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: qty, rate: 1000, amount: qty * 1000 },
    ],
    ...overrides,
  };
}

function editBody(
  companyId: number,
  poCode: number,
  poNumber: string,
  paymentTerms: string,
  qty = 10
) {
  return {
    poCode,
    customerCode:   CUSTOMER_CODE,
    poNumber,
    poType:         1,
    poDate:         new Date().toISOString(),
    customerPoDate: new Date().toISOString(),
    creditDays:     30,
    companyId,
    grandTotal:     qty * 1000,
    paymentTerms,
    details: [
      { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: qty, rate: 1000, amount: qty * 1000 },
    ],
  };
}

test.describe('Purchase Order — AMEND workflow', () => {

  // ── 1. Baseline: fresh PO has no amendment data ─────────────────────────────

  test('create PO → CPOM_AM_COUNT = 0, CPOM_AM_DATE null, no archive rows', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-BASE');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber);

    const po = await getPoByCode(db, poCode);
    expect(po).not.toBeNull();
    expect(po!.CPOM_AM_COUNT ?? 0, 'New PO must have CPOM_AM_COUNT = 0').toBe(0);
    expect(po!.CPOM_AM_DATE, 'New PO must have CPOM_AM_DATE = null').toBeNull();

    const archives = await getAmendmentArchives(db, poCode);
    expect(archives, 'No CUSTPO_AM_MASTER rows until first amend').toHaveLength(0);
  });

  // ── 2. Plain PUT (MODIFY path) must NOT touch AM_COUNT ──────────────────────

  test('PUT (plain edit) → CPOM_AM_COUNT stays 0, no archive rows created', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-PUT');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber);

    const resp = await apiRequest(
      apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`,
      editBody(companyId, poCode, poNumber, 'Net 45')
    );
    expect([200, 204]).toContain(resp.status);

    const po = await getPoByCode(db, poCode);
    expect(po!.CPOM_AM_COUNT ?? 0, 'PUT must NOT increment CPOM_AM_COUNT').toBe(0);
    expect(po!.CPOM_AM_DATE, 'PUT must NOT set CPOM_AM_DATE').toBeNull();

    const archives = await getAmendmentArchives(db, poCode);
    expect(archives, 'PUT must NOT create CUSTPO_AM_MASTER rows').toHaveLength(0);
  });

  // ── 3. POST /amend increments AM_COUNT and releases lock ────────────────────

  test('POST /amend → CPOM_AM_COUNT becomes 1, CPOM_AM_DATE set, MODIFY = 0', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-AM1');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber);

    const resp = await apiRequest(
      apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/amend`,
      amendBody(companyId, poNumber, 'Net 45')
    );
    expect(resp.status).toBe(200);

    const po = await getPoByCode(db, poCode);
    expect(po!.CPOM_AM_COUNT, 'AMEND must increment CPOM_AM_COUNT to 1').toBe(1);
    expect(po!.CPOM_AM_DATE, 'AMEND must set CPOM_AM_DATE').not.toBeNull();
    expect(po!.MODIFY, 'AMEND must release lock (MODIFY = 0)').toBeFalsy();
  });

  // ── 4. POST /amend creates a row in CUSTPO_AM_MASTER ────────────────────────

  test('POST /amend → one row written to CUSTPO_AM_MASTER', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-ARCH');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber);

    await apiRequest(
      apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/amend`,
      amendBody(companyId, poNumber, 'Net 45')
    );

    const archives = await getAmendmentArchives(db, poCode);
    expect(archives, 'Exactly one CUSTPO_AM_MASTER row after first amend').toHaveLength(1);
    expect(archives[0].CPOM_CODE).toBe(poCode);
    expect(archives[0].CPOM_AM_COUNT).toBe(1);
    expect(archives[0].CPOM_AM_DATE).not.toBeNull();
  });

  // ── 5. POST /amend copies detail rows into CUSTPO_AMD_DETAIL ────────────────

  test('POST /amend → CUSTPO_AMD_DETAIL receives the pre-amend detail rows', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-DET');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber, {
      details: [
        { itemCode: ITEM_CODE_1, uomCode: UOM_CODE, orderedQuantity: 5, rate: 1000, amount: 5000 },
        { itemCode: ITEM_CODE_2, uomCode: UOM_CODE, orderedQuantity: 3, rate: 200,  amount: 600  },
      ],
      grandTotal: 5600,
    });

    await apiRequest(
      apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/amend`,
      amendBody(companyId, poNumber, 'Net 60', 8)
    );

    const archives = await getAmendmentArchives(db, poCode);
    expect(archives).toHaveLength(1);

    const amdDetails = await getAmendmentArchiveDetails(db, archives[0].CPOM_AM_CODE);
    expect(amdDetails, 'Both pre-amend detail rows must be archived').toHaveLength(2);

    const itemCodes = amdDetails.map(d => d.CPOD_I_CODE).sort();
    expect(itemCodes).toEqual([ITEM_CODE_1, ITEM_CODE_2].sort());

    const item1 = amdDetails.find(d => d.CPOD_I_CODE === ITEM_CODE_1)!;
    expect(item1.CPOD_ORD_QTY, 'Archived qty must match the pre-amend state').toBe(5);
  });

  // ── 6. POST /amend replaces live details with amended data ──────────────────

  test('POST /amend → live CUSTPO_DETAIL is replaced with new detail data', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-LIVE');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber);

    // Original: qty=10. Amend: qty=7, new payment terms.
    await apiRequest(
      apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/amend`,
      amendBody(companyId, poNumber, 'Net 90 Amended', 7)
    );

    const liveDetails = await getPoDetails(db, poCode);
    expect(liveDetails, 'Live details must have exactly one row after amend').toHaveLength(1);
    expect(liveDetails[0].CPOD_ORD_QTY, 'Live qty must reflect the amended value').toBe(7);

    const po = await getPoByCode(db, poCode);
    expect(po!.CPOM_PAY_TERM, 'Master payment terms must reflect the amended value').toContain('Net 90');
  });

  // ── 7. Two consecutive amends → AM_COUNT = 2, two archive rows ──────────────

  test('two consecutive amends → CPOM_AM_COUNT = 2 and two CUSTPO_AM_MASTER rows', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-2X');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber);

    await apiRequest(
      apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/amend`,
      amendBody(companyId, poNumber, 'First amend', 9)
    );
    await apiRequest(
      apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/amend`,
      amendBody(companyId, poNumber, 'Second amend', 8)
    );

    const po = await getPoByCode(db, poCode);
    expect(po!.CPOM_AM_COUNT, 'CPOM_AM_COUNT must be 2 after two amends').toBe(2);

    const archives = await getAmendmentArchives(db, poCode);
    expect(archives, 'Two archive rows must exist after two amends').toHaveLength(2);
    const counts = archives.map(a => a.CPOM_AM_COUNT).sort();
    expect(counts, 'Archive rows must record amendment counts 1 and 2').toEqual([1, 2]);
  });

  // ── 8. Mixed: PUT then AMEND — AM_COUNT reflects only the amend ─────────────

  test('PUT then POST /amend → CPOM_AM_COUNT = 1 (only amend counts)', async ({
    db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('AMD-MIX');
    const poCode   = await createPo(apiUrl, accessToken, companyId, poNumber);

    // Plain edit — should not increment
    await apiRequest(
      apiUrl, accessToken, 'PUT', `/CustomerPo/${poCode}`,
      editBody(companyId, poCode, poNumber, 'Net 45')
    );

    // Then amend — should set AM_COUNT to 1
    await apiRequest(
      apiUrl, accessToken, 'POST', `/CustomerPo/${poCode}/amend`,
      amendBody(companyId, poNumber, 'Net 60 Amended')
    );

    const po = await getPoByCode(db, poCode);
    expect(po!.CPOM_AM_COUNT, 'Only the AMEND increments AM_COUNT — PUT does not').toBe(1);

    const archives = await getAmendmentArchives(db, poCode);
    expect(archives, 'Exactly one archive row (from amend, not from PUT)').toHaveLength(1);
  });
});
