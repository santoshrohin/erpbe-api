/**
 * Labour Charge Invoice — Print
 *
 * GET /LabourChargeInvoice/{id}/print?companyCode=
 * Must return a PDF with correct content (invoice heading, customer, amounts).
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
const PO_CODE = 9001;

test.describe('Labour Charge Invoice — Print', () => {

  test('print returns PDF blob > 1KB with %PDF magic bytes', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 5, rate: 500, amount: 2500 }],
    });
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/LabourChargeInvoice/${invoiceCode}/print?companyCode=${companyId}`
    );
    expect(printResp.status, 'Print must return 200').toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    expect(buffer.byteLength, 'PDF must be > 1KB').toBeGreaterThan(1024);

    const magic = String.fromCharCode(...new Uint8Array(buffer.slice(0, 4)));
    expect(magic, 'Must start with PDF magic bytes').toBe('%PDF');
  });

  test('print PDF contains LABOUR CHARGE INVOICE heading and customer name', async ({
    companyId, apiUrl, accessToken,
  }) => {
    const createResp = await apiRequest(apiUrl, accessToken, 'POST', '/LabourChargeInvoice', {
      companyCode: companyId,
      invoiceDate: new Date().toISOString(),
      customerCode: CUSTOMER_CODE,
      customerPoCode: PO_CODE,
      details: [{ invoiceQuantity: 7, rate: 400, amount: 2800 }],
    });
    expect(createResp.status).toBe(201);
    const invoiceCode = (createResp.data as { invoiceCode: number }).invoiceCode;

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/LabourChargeInvoice/${invoiceCode}/print?companyCode=${companyId}`
    );
    expect(printResp.status).toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    const text = await extractPdfText(buffer);

    expect(text, 'PDF must contain LABOUR CHARGE INVOICE heading').toContain('LABOUR CHARGE INVOICE');
    expect(text, 'PDF must contain customer name').toContain('E2E Test Customer');
    expect(text, 'PDF must contain rate').toContain('400.00');
  });
});
