/**
 * Tax Invoice — Print (single + batch)
 *
 * Single print: GET /TaxInvoice/{id}/print?companyId=&copyType=
 * Batch print:  POST /TaxInvoice/print-batch
 *
 * Both must return application/pdf with non-empty content.
 * copyType: 0=Original, 1=Duplicate, 2=Triplicate, 3=Quadruplicate
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

const COMPANY_CODE = 1;
const CUSTOMER_CODE = 9001;
const ITEM_CODE = 9001;
const UOM_CODE = 1;
const PO_CODE = 9001;
const STORE_CODE = 1;

async function createInvoice(apiUrl: string, accessToken: string): Promise<number> {
  const resp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice', {
    companyCode: COMPANY_CODE,
    invoiceDate: new Date().toISOString(),
    customerCode: CUSTOMER_CODE,
    customerPoCode: PO_CODE,
    invoiceDetails: [
      { itemCode: ITEM_CODE, uomCode: UOM_CODE, invoiceQuantity: 1, rate: 500, storeCode: STORE_CODE },
    ],
  });
  if (resp.status !== 201) throw new Error(`Invoice create failed: ${resp.status}`);
  return (resp.data as { invoiceCode: number }).invoiceCode;
}

test.describe('Tax Invoice — Print', () => {

  test('single print (Original copy) → PDF blob > 1KB', async ({
    apiUrl, accessToken, companyId,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/TaxInvoice/${invoiceCode}/print?companyId=${companyId}&copyType=0`
    );

    expect(printResp.status, 'Single print must return 200').toBe(200);
    const buffer = printResp.data as ArrayBuffer;
    expect(buffer.byteLength).toBeGreaterThan(1024);

    const magic = String.fromCharCode(...new Uint8Array(buffer.slice(0, 4)));
    expect(magic).toBe('%PDF');
  });

  test('single print (Duplicate copy) → PDF returned', async ({
    apiUrl, accessToken, companyId,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);
    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/TaxInvoice/${invoiceCode}/print?companyId=${companyId}&copyType=1`
    );
    expect(printResp.status).toBe(200);
    const buffer = printResp.data as ArrayBuffer;
    expect(buffer.byteLength).toBeGreaterThan(1024);
  });

  test('batch print (2 invoices) → single PDF returned', async ({
    apiUrl, accessToken, companyId,
  }) => {
    const [inv1, inv2] = await Promise.all([
      createInvoice(apiUrl, accessToken),
      createInvoice(apiUrl, accessToken),
    ]);

    const batchResp = await apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice/print-batch', {
      companyId,
      invoices: [
        { invoiceCode: inv1, copyType: 0 },
        { invoiceCode: inv2, copyType: 0 },
      ],
    });

    expect(batchResp.status, 'Batch print must return 200').toBe(200);
    const buffer = batchResp.data as ArrayBuffer;
    expect(buffer.byteLength).toBeGreaterThan(1024);

    const magic = String.fromCharCode(...new Uint8Array(buffer.slice(0, 4)));
    expect(magic, 'Batch print must return a valid PDF').toBe('%PDF');
  });

  // ── Phase 6: PDF content validation ─────────────────────────────────────────

  test('print PDF contains company name, customer name, and amount', async ({
    apiUrl, accessToken, companyId, db,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/TaxInvoice/${invoiceCode}/print?companyId=${companyId}&copyType=0`
    );
    expect(printResp.status).toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    const text = await extractPdfText(buffer);

    expect(text, 'PDF must contain company name').toContain('Test Company');
    expect(text, 'PDF must contain customer name').toContain('E2E Test Customer');
    expect(text, 'PDF must contain rate/amount (500)').toContain('500.00');
  });

  test('print PDF Original copy label present', async ({
    apiUrl, accessToken, companyId,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/TaxInvoice/${invoiceCode}/print?companyId=${companyId}&copyType=0`
    );
    expect(printResp.status).toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    const text = await extractPdfText(buffer);
    // copyType=0 → Original; copyType=1 → Duplicate
    expect(text, 'PDF must contain "Original" copy label').toContain('Original');
  });

  test('print PDF Duplicate copy label present', async ({
    apiUrl, accessToken, companyId,
  }) => {
    const invoiceCode = await createInvoice(apiUrl, accessToken);

    const printResp = await apiRequest(
      apiUrl, accessToken, 'GET',
      `/TaxInvoice/${invoiceCode}/print?companyId=${companyId}&copyType=1`
    );
    expect(printResp.status).toBe(200);

    const buffer = Buffer.from(printResp.data as ArrayBuffer);
    const text = await extractPdfText(buffer);
    expect(text, 'PDF must contain "Duplicate" copy label').toContain('Duplicate');
  });

  test('batch print PDF is larger than single print (more content)', async ({
    apiUrl, accessToken, companyId,
  }) => {
    const [inv1, inv2, inv3] = await Promise.all([
      createInvoice(apiUrl, accessToken),
      createInvoice(apiUrl, accessToken),
      createInvoice(apiUrl, accessToken),
    ]);

    const [singleResp, batchResp] = await Promise.all([
      apiRequest(apiUrl, accessToken, 'GET', `/TaxInvoice/${inv1}/print?companyId=${companyId}&copyType=0`),
      apiRequest(apiUrl, accessToken, 'POST', '/TaxInvoice/print-batch', {
        companyId,
        invoices: [inv1, inv2, inv3].map(c => ({ invoiceCode: c, copyType: 0 })),
      }),
    ]);

    const singleSize = (singleResp.data as ArrayBuffer).byteLength;
    const batchSize = (batchResp.data as ArrayBuffer).byteLength;

    expect(batchSize).toBeGreaterThan(singleSize);
  });
});
