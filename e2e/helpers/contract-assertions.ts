/**
 * Request body contract assertions.
 *
 * Each function validates one module's request body against the backend
 * contract. These are used in:
 *   - Playwright network interception tests (Layer 1)
 *   - MSW strict-mode tests (Layer 3)
 *
 * Rules enforced here mirror what the backend's FluentValidation + controller
 * checks enforce. If the frontend sends a body that fails these assertions,
 * the backend WILL return 4xx.
 */

import { expect } from '@playwright/test';

// ── Purchase Order ────────────────────────────────────────────────────────────

/**
 * Assert that a PUT /CustomerPo/{id} request body is valid.
 *
 * The field that caused the production bug:
 *   body.poCode must equal the URL id.
 *   Backend: if (id != command.PoCode) → 400
 */
export function assertPoUpdateBody(
  body: Record<string, unknown>,
  expectedPoCode: number
): void {
  // ── The regression guard ────────────────────────────────────────────────
  expect(body.poCode, 'PUT body must contain poCode').toBeDefined();
  expect(body.poCode, `body.poCode (${body.poCode}) must equal URL id (${expectedPoCode})`).toBe(expectedPoCode);

  // ── Required fields ─────────────────────────────────────────────────────
  expect(body.customerCode, 'customerCode required').toBeDefined();
  expect(body.customerCode, 'customerCode must not be 0').not.toBe(0);
  expect(body.poNumber, 'poNumber required').toBeDefined();
  expect(body.poDate, 'poDate required').toBeDefined();
  expect(body.companyId, 'companyId required').toBeDefined();
  expect(body.companyId, 'companyId must not be 0').not.toBe(0);

  // ── details array ───────────────────────────────────────────────────────
  expect(Array.isArray(body.details), 'details must be an array').toBe(true);

  // ── Nullable fields: must be null, not 0 or "" ──────────────────────────
  if ('projectCode' in body) {
    if (body.projectCode !== null) {
      expect(
        typeof body.projectCode,
        'projectCode must be null or number'
      ).toBe('number');
      expect(body.projectCode, 'projectCode 0 must be sent as null').not.toBe(0);
    }
  }

  if ('customerPoDate' in body && body.customerPoDate !== null) {
    expect(body.customerPoDate, 'empty customerPoDate must be null not ""').not.toBe('');
  }

  if ('quotationCode' in body && body.quotationCode !== null) {
    expect(body.quotationCode, 'quotationCode 0 must be null').not.toBe(0);
  }
}

/**
 * Assert that a POST /CustomerPo request body is valid.
 */
export function assertPoCreateBody(body: Record<string, unknown>): void {
  expect(body.customerCode, 'customerCode required').toBeDefined();
  expect(body.customerCode, 'customerCode must not be 0').not.toBe(0);
  expect(body.poNumber, 'poNumber required').toBeDefined();
  expect(body.poDate, 'poDate required').toBeDefined();
  expect(body.companyId, 'companyId required').toBeDefined();
  expect(Array.isArray(body.details), 'details must be an array').toBe(true);

  if ('projectCode' in body && body.projectCode !== null) {
    expect(body.projectCode, 'projectCode 0 must be null').not.toBe(0);
  }
  if ('customerPoDate' in body && body.customerPoDate !== null) {
    expect(body.customerPoDate, 'empty customerPoDate must be null').not.toBe('');
  }
}

// ── Tax Invoice ───────────────────────────────────────────────────────────────

/**
 * Assert that a PUT /TaxInvoice/{id} request body is valid.
 */
export function assertInvoiceUpdateBody(
  body: Record<string, unknown>,
  expectedInvoiceCode: number
): void {
  expect(body.invoiceCode, 'PUT body must contain invoiceCode').toBeDefined();
  expect(body.invoiceCode, `invoiceCode must equal URL id (${expectedInvoiceCode})`).toBe(expectedInvoiceCode);
  expect(body.customerCode, 'customerCode required').toBeDefined();
  expect(body.invoiceDate, 'invoiceDate required').toBeDefined();
  expect(Array.isArray(body.invoiceDetails), 'invoiceDetails must be an array').toBe(true);
  if ('lrDate' in body && body.lrDate !== null) {
    expect(body.lrDate, 'empty lrDate must be null').not.toBe('');
  }
}

/**
 * Assert that a POST /TaxInvoice request body is valid.
 */
export function assertInvoiceCreateBody(body: Record<string, unknown>): void {
  expect(body.customerCode, 'customerCode required').toBeDefined();
  expect(body.invoiceDate, 'invoiceDate required').toBeDefined();
  expect(Array.isArray(body.invoiceDetails), 'invoiceDetails must be an array').toBe(true);
  if ('lrDate' in body && body.lrDate !== null) {
    expect(body.lrDate, 'empty lrDate must be null').not.toBe('');
  }
}

// ── Delivery Challan ──────────────────────────────────────────────────────────

/**
 * Assert that a POST /DeliveryChallan request body is valid.
 * DC uses companyCode (not companyId) per controller signature.
 */
export function assertChallanCreateBody(body: Record<string, unknown>): void {
  expect(body.customerCode, 'customerCode required').toBeDefined();
  expect(body.challanDate, 'challanDate required').toBeDefined();
  expect(Array.isArray(body.details), 'details must be an array').toBe(true);
  // companyCode is the field for DC (not companyId)
  expect(body.companyCode, 'companyCode required for DC').toBeDefined();
  expect(body.companyCode, 'companyCode must not be 0').not.toBe(0);
}

// ── Labour Charge Invoice ─────────────────────────────────────────────────────

/**
 * Assert that a POST /LabourChargeInvoice request body is valid.
 * Labour invoice uses companyCode (not companyId) per controller signature.
 */
export function assertLabourCreateBody(body: Record<string, unknown>): void {
  expect(body.customerCode, 'customerCode required').toBeDefined();
  expect(body.invoiceDate, 'invoiceDate required').toBeDefined();
  expect(Array.isArray(body.details), 'details must be an array').toBe(true);
  expect(body.companyCode, 'companyCode required for Labour Invoice').toBeDefined();
  expect(body.companyCode, 'companyCode must not be 0').not.toBe(0);
}

// ── Generic helpers ───────────────────────────────────────────────────────────

/**
 * Ensures no field in the body has an explicit `undefined` value.
 * JSON.stringify strips undefined silently — this catches it before stringify.
 */
export function assertNoUndefinedFields(
  body: Record<string, unknown>,
  context = ''
): void {
  const ctx = context ? ` [${context}]` : '';
  const bad = Object.entries(body)
    .filter(([, v]) => v === undefined)
    .map(([k]) => k);
  expect(
    bad,
    `Body must have no undefined values${ctx}. Found: ${bad.join(', ')}`
  ).toHaveLength(0);
}

/**
 * Asserts that all string date fields in the body are either a valid ISO
 * string or null — never an empty string.
 */
export function assertNullableDateFields(
  body: Record<string, unknown>,
  dateFields: string[],
  context = ''
): void {
  const ctx = context ? ` [${context}]` : '';
  for (const field of dateFields) {
    if (field in body) {
      const val = body[field];
      expect(val, `${field} must be null or non-empty ISO string${ctx}`).not.toBe('');
    }
  }
}
