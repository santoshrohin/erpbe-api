/**
 * Delivery Challan — Print
 *
 * GET /DeliveryChallan/{id}/print?companyCode=
 * Must return a PDF with correct content (company, customer, item, challan heading).
 */

import { test, expect } from '../../fixtures';
import { apiRequest } from '../../fixtures/auth';

// eslint-disable-next-line @typescript-eslint/no-require-imports
const { PDFParse } = require('pdf-parse');
async function extractPdfText(buffer: Buffer): Promise<string> {
  const parser = new PDFParse({ data: buffer });
  const result = await parser.getText();
  return result.text ?? '';
}

const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const UOM_CODE = 1;

test.describe('Delivery Challan — Print', () => {

  test('print returns PDF blob > 1KB with %PDF magic bytes', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan', {
      companyCode: companyId,
      challanDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 2 }],
    });
    expect(createResp.status).toBe(201);
    const challanCode = (createResp.data as { challanCode: number }).challanCode;

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/DeliveryChallan/${challanCode}/print?companyCode=${companyId}`
    );
    expect(printResp.status, 'Print must return 200').toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    expect(buffer.byteLength, 'PDF must be > 1KB').toBeGreaterThan(1024);

    const magic = String.fromCharCode(...new Uint8Array(buffer.slice(0, 4)));
    expect(magic, 'Must start with PDF magic bytes').toBe('%PDF');
  });

  test('print PDF contains DELIVERY CHALLAN heading and customer name', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/DeliveryChallan', {
      companyCode: companyId,
      challanDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      details: [{ itemCode: ITEM_CODE, uomCode: UOM_CODE, orderedQuantity: 3 }],
    });
    expect(createResp.status).toBe(201);
    const challanCode = (createResp.data as { challanCode: number }).challanCode;

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/DeliveryChallan/${challanCode}/print?companyCode=${companyId}`
    );
    expect(printResp.status).toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    const text = await extractPdfText(buffer);

    expect(text, 'PDF must contain DELIVERY CHALLAN heading').toContain('DELIVERY CHALLAN');
    expect(text, 'PDF must contain customer name').toContain('E2E Test Customer');
    expect(text, 'PDF must contain ordered quantity').toContain('3.000');
  });
});
