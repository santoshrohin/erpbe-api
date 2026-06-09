/**
 * Tax Invoice — Lock / Unlock workflow
 *
 * Parity:
 *   Legacy: MODIFY column in INVOICE_MASTER
 *   New:    ERP_LockInvoice / ERP_UnlockInvoice SPs toggle MODIFY
 *
 * Also tests the new checkbox selection feature (was invisible before fix).
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { isTaxInvoiceLocked } from '../../helpers/db-queries';
import { apiRequest } from '../../fixtures/auth';

const COMPANY_CODE = 1;
const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const UOM_CODE = 1;
const PO_CODE = 9001;
const STORE_CODE = 1;

async function createInvoice(apiUrl: string, accessToken: string) {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    invoiceDetails: [
      { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 500, storeCode: STORE_CODE },
    ],
  });
  if (resp.status !== 201) throw new Error(`Create invoice failed: ${resp.status}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Tax Invoice — Lock / Unlock', () => {

  test('lock invoice → grid shows Locked + DB MODIFY=1', async ({
    page, db, companyId, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    await page.goto('/transactions/tax-invoice');
    await page.waitForLoadState('networkidle');

    // AG Grid renders the same row-id in multiple DOM containers (main + pinned).
    // Asserting toBeVisible() on the row itself fails because one of the containers
    // may be clipped.  Use row-id to scope directly to the button/status instead.
    const lockBtn = page.locator(`[row-id="${invoiceCode}"] button[title="Lock"]`).first();
    await expect(lockBtn).toBeVisible({ timeout: 8_000 });
    await lockBtn.click();

    // Wait for this specific row to update to Locked status
    await expect(
      page.locator(`[row-id="${invoiceCode}"] span:has-text("Locked")`).first()
    ).toBeVisible({ timeout: 8_000 });

    // DB
    const locked = await isTaxInvoiceLocked(db, invoiceCode);
    expect(locked, 'INVOICE_MASTER.MODIFY must be 1').toBe(true);
  });

  // ── Phase 3: Unlock DB assertion ────────────────────────────────────────────

  test('unlock invoice → DB MODIFY=0 + edit and delete re-enabled', async ({
    page, db, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    // Lock via API
    const lockResp = await apiRequest(apiUrl, accessToken, 'POST', `/TaxInvoice/${invoiceCode}/lock?companyId=${COMPANY_CODE}`, null);
    expect([200, 204]).toContain(lockResp.status);

    // Confirm locked in DB
    const lockedBefore = await isTaxInvoiceLocked(db, invoiceCode);
    expect(lockedBefore, 'MODIFY must be 1 after lock').toBe(true);

    // Unlock via API
    const unlockResp = await apiRequest(apiUrl, accessToken, 'POST', `/TaxInvoice/${invoiceCode}/unlock?companyId=${COMPANY_CODE}`, null);
    expect([200, 204]).toContain(unlockResp.status);

    // DB: MODIFY must be 0 after unlock
    const lockedAfter = await isTaxInvoiceLocked(db, invoiceCode);
    expect(lockedAfter, 'INVOICE_MASTER.MODIFY must be 0 after unlock').toBe(false);

    // UI: Unlock button must now read "Lock" again (edit/delete re-enabled)
    await page.goto('/transactions/tax-invoice');
    await page.waitForLoadState('networkidle');

    const lockBtn = page.locator(`[row-id="${invoiceCode}"] button[title="Lock"]`).first();
    await expect(lockBtn, 'Lock button must be visible again after unlock').toBeVisible({ timeout: 8_000 });

    // Edit and Delete buttons must be enabled after unlock
    const editBtn = page.locator(`[row-id="${invoiceCode}"] button[title="Edit"]`).first();
    await expect(editBtn, 'Edit button must be enabled after unlock').toBeEnabled({ timeout: 5_000 });

    const deleteBtn = page.locator(`[row-id="${invoiceCode}"] button[title="Delete"]`).first();
    await expect(deleteBtn, 'Delete button must be enabled after unlock').toBeEnabled({ timeout: 5_000 });
  });

  test('locked invoice → edit and delete buttons disabled', async ({
    page, apiUrl, accessToken,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    // Lock via API
    await apiRequest(apiUrl, accessToken, 'POST', `/TaxInvoice/${invoiceCode}/lock?companyId=${COMPANY_CODE}`, null);

    await page.goto('/transactions/tax-invoice');
    await page.waitForLoadState('networkidle');

    const editBtn = page.locator(`[row-id="${invoiceCode}"] button[title="Edit"]`).first();
    await expect(editBtn, 'Edit must be disabled when locked').toBeDisabled({ timeout: 8_000 });

    const deleteBtn = page.locator(`[row-id="${invoiceCode}"] button[title="Delete"]`).first();
    await expect(deleteBtn, 'Delete must be disabled when locked').toBeDisabled({ timeout: 5_000 });
  });

  test('checkbox selection visible — can select invoices for batch print', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    // Ensure at least one invoice exists
    await createInvoice(apiUrl, accessToken);

    await page.goto('/transactions/tax-invoice');
    await page.waitForLoadState('networkidle');

    // AG Grid checkbox column must be rendered
    // The header checkbox is in a column header with no text label
    const headerCheckbox = page.locator('[col-id="_select"] input[type="checkbox"], .ag-header-select-all').first();
    await expect(headerCheckbox).toBeVisible({ timeout: 8_000 });

    // Click header checkbox to select all
    await headerCheckbox.click();

    // Selection banner must appear
    await expect(page.locator(':has-text("selected"), :has-text("invoice")').filter({ hasText: /\d+/ }).first())
      .toBeVisible({ timeout: 5_000 });

    // Print Multiple button must be enabled after selection
    const printMultiBtn = page.locator('button:has-text("Print Multiple")');
    await expect(printMultiBtn).toBeEnabled({ timeout: 5_000 });
  });

  test('Print Multiple button always visible (was hidden when no rows selected)', async ({
    page,
  }) => {
    await page.goto('/transactions/tax-invoice');
    await page.waitForLoadState('networkidle');

    // Must be visible even with 0 rows selected
    const printMultiBtn = page.locator('button:has-text("Print Multiple")');
    await expect(printMultiBtn).toBeVisible({ timeout: 8_000 });

    // Disabled when nothing selected
    await expect(printMultiBtn).toBeDisabled();
  });
});
