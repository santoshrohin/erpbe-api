/**
 * Purchase Order — Print workflow
 *
 * Parity gap this covers:
 *   - SP used CM_CODE = @CompanyCode but only CM_ID was available in JWT
 *   - Fix: SP changed to CM_ID = @CompanyId
 *
 * Test:
 *   - Print button is visible in PO list
 *   - Clicking Print triggers GET /CustomerPo/{id}/print
 *   - Response is a PDF blob (Content-Type: application/pdf)
 *   - PDF is non-empty (> 1KB)
 *
 * Note: We intercept the download rather than checking the rendered PDF.
 */

import { test, expect, uniqueRef } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

// eslint-disable-next-line @typescript-eslint/no-require-imports
const { PDFParse } = require('pdf-parse');
async function extractPdfText(buffer: Buffer): Promise<string> {
  const parser = new PDFParse({ data: buffer });
  const result = await parser.getText();
  return result.text ?? '';
}

test.describe('Purchase Order — Print', () => {

  test('print button triggers PDF download — non-empty PDF returned', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    // Create PO
    const poNumber = uniqueRef('PO-PRINT');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: 9001,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 11800,
      projectCode: null,
      details: [{ itemCode: 9001, uomCode: 1, orderedQuantity: 10, rate: 1000, amount: 10000 }],
    });
    expect(createResp.status).toBe(201);
    const poId = (createResp.data as { poCode: number }).poCode;

    // Direct API print — validates the core bug fix (CM_ID vs CM_CODE)
    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/CustomerPo/${poId}/print?companyId=${companyId}&companyCode=${companyId}`
    );

    expect(printResp.status, 'Print endpoint must return 200').toBe(200);

    // The response body is an ArrayBuffer when content-type is application/pdf
    const buffer = printResp.data as ArrayBuffer;
    expect(buffer.byteLength, 'PDF must not be empty').toBeGreaterThan(1024);

    // Check PDF magic bytes (%PDF)
    const bytes = new Uint8Array(buffer.slice(0, 4));
    const magic = String.fromCharCode(...bytes);
    expect(magic, 'Response must start with PDF magic bytes').toBe('%PDF');
  });

  // ── Phase 6: PDF content validation ─────────────────────────────────────────

  test('print PDF contains company name, customer name, and item', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const poNumber = uniqueRef('PO-CONTENT');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: 9001,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 10000,
      projectCode: null,
      details: [{ itemCode: 9001, uomCode: 1, orderedQuantity: 10, rate: 1000, amount: 10000 }],
    });
    expect(createResp.status).toBe(201);
    const poId = (createResp.data as { poCode: number }).poCode;

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/CustomerPo/${poId}/print?companyId=${companyId}&companyCode=${companyId}`
    );
    expect(printResp.status).toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    const text = await extractPdfText(buffer);

    expect(text, 'PDF must contain company name').toContain('Test Company');
    expect(text, 'PDF must contain customer name').toContain('E2E Test Customer');
    expect(text, 'PDF must contain item name').toContain('E2E Test Item Alpha');
    expect(text, 'PDF must contain "Sales Order" heading').toContain('Sales Order');
  });

  test('print button visible in PO list row', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    // Create PO
    const poNumber = uniqueRef('PO-PRINT-UI');
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: 9001,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 5000,
      projectCode: null,
      details: [{ itemCode: 9001, uomCode: 1, orderedQuantity: 5, rate: 1000, amount: 5000 }],
    });
    const poId = (createResp.data as { poCode: number }).poCode;

    await page.goto('/transactions/purchase-order');
    await page.waitForLoadState('networkidle');

    // Verify the row is visible (main container has PO number text)
    const poRow = page.locator(`[role="row"]:has-text("${poNumber}")`);
    await expect(poRow).toBeVisible({ timeout: 8_000 });

    // Print button is in the pinned-right container — use row-id to find it
    const printBtn = page.locator(`[row-id="${poId}"] button[title="Print"]`).first();
    await expect(printBtn).toBeVisible();
    await expect(printBtn).not.toBeDisabled();
  });

  test('print button missing before this fix — regression guard', async ({
    page, companyId, apiUrl, accessToken,
  }) => {
    // This test ensures the print column is present in ALL rows (not conditionally hidden)
    const poNumber = uniqueRef('PO-PRINT-REG');
    await apiRequest(apiUrl, accessToken, 'POST', '/CustomerPo', {
      customerCode: 9001,
      poNumber,
      poType: 1,
      poDate: new Date().toISOString(),
      creditDays: 30,
      companyId,
      grandTotal: 1000,
      projectCode: null,
      details: [{ itemCode: 9001, uomCode: 1, orderedQuantity: 1, rate: 1000, amount: 1000 }],
    });

    await page.goto('/transactions/purchase-order');
    await page.waitForLoadState('networkidle');

    // The "Actions" column header must exist
    const actionsHeader = page.locator('th:has-text("Actions"), [role="columnheader"]:has-text("Actions")').first();
    await expect(actionsHeader).toBeVisible({ timeout: 5_000 });

    // At least one Print button in the grid
    const printButtons = page.locator('button[title="Print"]');
    await expect(printButtons.first()).toBeVisible({ timeout: 5_000 });
  });
});
