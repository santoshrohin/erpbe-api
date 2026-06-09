/**
 * Purchase Order — View and Amend UI
 *
 * Covers the View page (read-only, no lock) and Amend page (lock acquired)
 * UI flows that are missing from po-legacy-equivalence.spec.ts.
 *
 * Parity with legacy:
 *   - View page never touches MODIFY (ViewCustomerPO.aspx.cs acquires no lock)
 *   - Amend page acquires MODIFY lock on mount — same as Edit before it
 *   - Cancel on amend → MODIFY = 0 (lock released)
 *   - Navigate away from amend → MODIFY = 0 (lock released via cleanup)
 *   - Submit amend → CPOM_AM_COUNT incremented, MODIFY = 0
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { getPoByCode, isPoLocked, getAmendmentArchives } from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const CUSTOMER_CODE = 9001;
const ITEM_CODE     = 9001;
const UOM_CODE      = 1;

function basePoPayload(companyId: number, poNumber: string) {
  const now = new Date().toISOString();
  return {
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
      { itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 10, rate: 1000, amount: 10000 },
    ],
  };
}

async function createTestPo(
  apiUrl: string,
  accessToken: string,
  companyId: number,
  poNumber: string
): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', basePoPayload(companyId, poNumber));
  if (resp.status !== 201) throw new Error(`PO create failed: HTTP ${resp.status}`);
  return (resp.data as { poCode: number }).poCode;
}

// ─── View page ────────────────────────────────────────────────────────────────

test.describe('UI — View page', () => {

  test('view page loads with "View Purchase Order" title and does NOT acquire lock', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('UI-VIEW');
    const poCode   = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto(`/transactions/purchase-order/view/${poCode}`);
    await page.waitForLoadState('networkidle');

    // Page title visible
    await expect(page.locator('text=View Purchase Order').first()).toBeVisible({ timeout: 10_000 });

    // PO Number visible in the body
    await expect(page.locator(`text=${poNumber}`).first()).toBeVisible({ timeout: 10_000 });

    // Lock must NOT have been acquired
    expect(await isPoLocked(db, poCode), 'View page must not acquire lock').toBeFalsy();
  });

  test('view page shows Amendment Count field', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('UI-VIEW-AMD');
    const poCode   = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto(`/transactions/purchase-order/view/${poCode}`);
    await page.waitForLoadState('networkidle');

    await expect(page.locator('text=Amendment Count').first()).toBeVisible({ timeout: 10_000 });
  });

  test('Amend button on view page navigates to amend page', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('UI-VIEW-TO-AMD');
    const poCode   = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto(`/transactions/purchase-order/view/${poCode}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('text=View Purchase Order').first()).toBeVisible({ timeout: 10_000 });

    // Click the Amend button
    await page.locator('button:has-text("Amend")').click();

    // Should navigate to amend page and acquire lock
    await page.waitForURL(`**/purchase-order/amend/${poCode}`, { timeout: 8_000 });
    await expect(page.locator('text=Amend Purchase Order').first()).toBeVisible({ timeout: 10_000 });
    // Wait for form to load (lock acquired async after purchaseOrder data loads)
    await expect(page.locator('[name="poNumber"]').first()).toBeVisible({ timeout: 10_000 });
    await page.waitForTimeout(1500);
    expect(await isPoLocked(db, poCode), 'Lock must be acquired on amend page').toBeTruthy();

    // Cleanup: navigate away to release lock
    await page.goto('/transactions/purchase-order');
    await page.waitForTimeout(1000);
  });
});

// ─── Amend page ───────────────────────────────────────────────────────────────

test.describe('UI — Amend page lock lifecycle', () => {

  test('opening amend page acquires lock (MODIFY = 1)', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('UI-AMD-LOCK');
    const poCode   = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto(`/transactions/purchase-order/amend/${poCode}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('text=Amend Purchase Order').first()).toBeVisible({ timeout: 10_000 });
    // Wait for form data to load and lock to be acquired
    await expect(page.locator('[name="poNumber"]').first()).toBeVisible({ timeout: 10_000 });
    await page.waitForTimeout(1500);

    expect(await isPoLocked(db, poCode), 'MODIFY must be 1 after opening amend page').toBeTruthy();
  });

  test('Cancel on amend page releases lock (MODIFY = 0)', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('UI-AMD-CANCEL');
    const poCode   = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto(`/transactions/purchase-order/amend/${poCode}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('[name="poNumber"]').first()).toBeVisible({ timeout: 10_000 });
    await page.waitForTimeout(1500);

    expect(await isPoLocked(db, poCode), 'Lock must be acquired before cancel').toBeTruthy();

    await page.locator('button[type="button"]:has-text("Cancel")').click();
    await page.waitForURL('**/purchase-order', { timeout: 8_000 });

    expect(await isPoLocked(db, poCode), 'MODIFY must be 0 after cancel').toBeFalsy();
  });

  test('navigating away from amend page releases lock', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('UI-AMD-NAV');
    const poCode   = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto(`/transactions/purchase-order/amend/${poCode}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('[name="poNumber"]').first()).toBeVisible({ timeout: 10_000 });
    await page.waitForTimeout(1500);
    expect(await isPoLocked(db, poCode)).toBeTruthy();

    // Click "Back to Purchase Orders" — this is a React Router navigation (navigate() hook),
    // NOT a full page reload. The component unmounts inside the same document so the browser
    // does not cancel the in-flight unlock fetch that fires in the useEffect cleanup.
    await page.locator('button:has-text("Back to Purchase Orders")').click();
    await page.waitForURL('**/purchase-order', { timeout: 8_000 });
    await page.waitForTimeout(1500);

    expect(await isPoLocked(db, poCode), 'MODIFY must be 0 after navigating away').toBeFalsy();
  });
});

// ─── Amend submission ─────────────────────────────────────────────────────────

test.describe('UI — Amend submission', () => {

  test('submit amend → CPOM_AM_COUNT = 1, lock released, archive row created', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('UI-AMD-SAVE');
    const poCode   = await createTestPo(apiUrl, accessToken, companyId, poNumber);

    await page.goto(`/transactions/purchase-order/amend/${poCode}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('[name="poNumber"]').first()).toBeVisible({ timeout: 10_000 });
    await page.waitForTimeout(1500);

    // Form is pre-populated with existing PO data — just save as-is
    await page.locator('button[type="submit"]:has-text("Save")').click();

    // The amend page navigates to the listing after success (1500 ms delay in the component).
    // Navigation is the proof of success — no toast assertion needed since the toast uses
    // styled-components with dynamic class names (no stable role or class selector).
    await page.waitForURL('**/purchase-order', { timeout: 15_000 });

    // DB verification
    const po = await getPoByCode(db, poCode);
    expect(po!.CPOM_AM_COUNT, 'CPOM_AM_COUNT must be 1 after amend').toBe(1);
    expect(po!.CPOM_AM_DATE, 'CPOM_AM_DATE must be set after amend').not.toBeNull();
    expect(po!.MODIFY, 'Lock must be released after successful amend').toBeFalsy();

    const archives = await getAmendmentArchives(db, poCode);
    expect(archives, 'One CUSTPO_AM_MASTER archive row must exist after amend').toHaveLength(1);
  });
});
