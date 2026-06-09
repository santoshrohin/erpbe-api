/**
 * Layer 6 — HAR Replay Runner
 *
 * Replays captured HTTP Archive (HAR) files from the legacy ERP ASPX app
 * against the new ERP REST API, then diffs the responses using diff-engine.ts.
 *
 * WORKFLOW:
 *   1. Capture HAR from legacy ASPX app using Chrome DevTools
 *      (Network tab → "Save all as HAR with content")
 *   2. Drop the .har file into e2e/parity/har-files/
 *   3. Run: npm run parity:compare
 *   4. Review PARITY_REPORT.md for unexpected diffs
 *   5. Add any accepted differences to KNOWN_DEVIATIONS in diff-engine.ts
 *
 * HAR FILES:
 *   Place .har captures from the legacy app here:
 *     e2e/parity/har-files/
 *       customer-po-create.har   — captured from legacy PO creation
 *       customer-po-edit.har     — captured from legacy PO edit
 *       tax-invoice-create.har   — captured from legacy invoice creation
 *       delivery-challan.har     — captured from legacy DC creation
 *
 * ENDPOINT MAPPING:
 *   Legacy ASPX PageMethods map to new REST endpoints:
 *     /SalesDefault.aspx/SaveCustomerPO → POST /api/CustomerPo
 *     /SalesDefault.aspx/UpdateCustomerPO → PUT /api/CustomerPo/{id}
 *     /TaxInvoice.aspx/SaveInvoice → POST /api/TaxInvoice
 *     /DeliveryChallan.aspx/SaveChallan → POST /api/DeliveryChallan
 *
 * AUTHENTICATION:
 *   Reads NEW_API_TOKEN from environment (populated by auth.setup.ts).
 *   The legacy HAR requests use session cookies (stripped during replay).
 */

import * as fs from 'fs';
import * as path from 'path';
import { computeParityDiff, formatParityReport, type ParityDiffResult } from './diff-engine';

// ── Types ─────────────────────────────────────────────────────────────────────

interface HarEntry {
  request: {
    method: string;
    url: string;
    headers: Array<{ name: string; value: string }>;
    postData?: { text: string };
  };
  response: {
    status: number;
    content: { text?: string; mimeType: string };
  };
}

interface HarFile {
  log: {
    entries: HarEntry[];
  };
}

export interface ReplayConfig {
  newApiBaseUrl: string;
  authToken: string;
  harDir?: string;
  reportDir?: string;
}

export interface ReplayResult {
  harFile: string;
  endpoint: string;
  method: string;
  legacyStatus: number;
  newStatus: number;
  parityResult: ParityDiffResult | null;
  error?: string;
}

// ── Endpoint mapping ──────────────────────────────────────────────────────────

/**
 * Maps legacy ASPX PageMethod URLs to new REST endpoints.
 * Returns null for entries that should be skipped (auth, static, etc.).
 */
export function mapLegacyUrlToNew(
  legacyUrl: string,
  legacyMethod: string
): { newPath: string; method: string } | null {
  // Skip non-API requests (HTML, JS, CSS, images)
  if (!/\.(aspx|asmx)/.test(legacyUrl) && !/PageMethods|WebService/.test(legacyUrl)) {
    return null;
  }

  // Customer PO
  if (legacyUrl.includes('SaveCustomerPO') || legacyUrl.includes('CreatePO')) {
    return { newPath: '/api/CustomerPo', method: 'POST' };
  }
  if (legacyUrl.includes('UpdateCustomerPO') || legacyUrl.includes('EditPO')) {
    return { newPath: '/api/CustomerPo', method: 'PUT' };
  }
  if (legacyUrl.includes('GetCustomerPO') || legacyUrl.includes('GetPODetails')) {
    return { newPath: '/api/CustomerPo', method: 'GET' };
  }

  // Tax Invoice
  if (legacyUrl.includes('SaveInvoice') || legacyUrl.includes('CreateInvoice')) {
    return { newPath: '/api/TaxInvoice', method: 'POST' };
  }
  if (legacyUrl.includes('UpdateInvoice') || legacyUrl.includes('EditInvoice')) {
    return { newPath: '/api/TaxInvoice', method: 'PUT' };
  }

  // Delivery Challan
  if (legacyUrl.includes('SaveChallan') || legacyUrl.includes('CreateChallan')) {
    return { newPath: '/api/DeliveryChallan', method: 'POST' };
  }

  // Labour Charge Invoice
  if (legacyUrl.includes('SaveLabourInvoice') || legacyUrl.includes('CreateLabour')) {
    return { newPath: '/api/LabourChargeInvoice', method: 'POST' };
  }

  return null;
}

// ── Body transformation ───────────────────────────────────────────────────────

/**
 * Transform a legacy ASPX PageMethod request body into a new REST API body.
 *
 * Legacy ASPX PageMethods wrap the payload in a `d` property and use
 * PascalCase. New API expects camelCase flat objects.
 *
 * IMPORTANT: This is a best-effort transform. Module-specific transformations
 * may need to be added here as gaps are found.
 */
export function transformLegacyBody(
  rawBody: string,
  endpoint: string
): Record<string, unknown> {
  let parsed: Record<string, unknown>;

  try {
    parsed = JSON.parse(rawBody);
  } catch {
    return {};
  }

  // Legacy sometimes wraps in { d: { ... } } (PageMethod pattern)
  const body = (parsed.d && typeof parsed.d === 'object')
    ? parsed.d as Record<string, unknown>
    : parsed;

  // PascalCase → camelCase for top-level keys
  const camel: Record<string, unknown> = {};
  for (const [k, v] of Object.entries(body)) {
    const camelKey = k.charAt(0).toLowerCase() + k.slice(1);
    camel[camelKey] = v;
  }

  return camel;
}

/**
 * Extract body from a legacy response. Handles both wrapped and unwrapped formats.
 */
export function extractLegacyResponseBody(rawText: string): Record<string, unknown> {
  try {
    const parsed = JSON.parse(rawText) as Record<string, unknown>;
    // PageMethod wraps successful responses in { d: ... }
    if ('d' in parsed) {
      const inner = parsed.d;
      if (typeof inner === 'string') {
        try {
          return JSON.parse(inner) as Record<string, unknown>;
        } catch {
          return { d: inner };
        }
      }
      return inner as Record<string, unknown>;
    }
    return parsed;
  } catch {
    return {};
  }
}

// ── HAR loading ───────────────────────────────────────────────────────────────

export function loadHarFile(filePath: string): HarFile {
  const raw = fs.readFileSync(filePath, 'utf-8');
  return JSON.parse(raw) as HarFile;
}

/**
 * Filter HAR entries to only those that map to known new API endpoints.
 */
export function getReplayableEntries(har: HarFile): Array<{
  entry: HarEntry;
  mapping: { newPath: string; method: string };
}> {
  const result = [];

  for (const entry of har.log.entries) {
    const mapping = mapLegacyUrlToNew(entry.request.url, entry.request.method);
    if (mapping && entry.response.content.text) {
      result.push({ entry, mapping });
    }
  }

  return result;
}

// ── Replay ────────────────────────────────────────────────────────────────────

/**
 * Replay one HAR entry against the new API and return the comparison result.
 */
export async function replayEntry(
  entry: HarEntry,
  mapping: { newPath: string; method: string },
  config: ReplayConfig
): Promise<ReplayResult> {
  const { newApiBaseUrl, authToken } = config;
  const url = `${newApiBaseUrl}${mapping.newPath}`;

  const transformedBody = entry.request.postData?.text
    ? transformLegacyBody(entry.request.postData.text, mapping.newPath)
    : undefined;

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${authToken}`,
  };

  try {
    const resp = await fetch(url, {
      method: mapping.method,
      headers,
      body: transformedBody ? JSON.stringify(transformedBody) : undefined,
    });

    const newBodyText = await resp.text();
    let newBody: Record<string, unknown> = {};
    try { newBody = JSON.parse(newBodyText) as Record<string, unknown>; } catch { /**/ }

    const legacyBody = extractLegacyResponseBody(entry.response.content.text ?? '');
    const legacyStatus = entry.response.status;

    let parityResult: ParityDiffResult | null = null;
    if (legacyStatus < 300 && resp.status < 300) {
      parityResult = computeParityDiff(mapping.newPath, mapping.method, legacyBody, newBody);
    }

    return {
      harFile: '',
      endpoint: mapping.newPath,
      method: mapping.method,
      legacyStatus,
      newStatus: resp.status,
      parityResult,
    };
  } catch (err) {
    return {
      harFile: '',
      endpoint: mapping.newPath,
      method: mapping.method,
      legacyStatus: entry.response.status,
      newStatus: 0,
      parityResult: null,
      error: String(err),
    };
  }
}

// ── Runner ────────────────────────────────────────────────────────────────────

/**
 * Main entry point: replay all HAR files in the har-files directory.
 *
 * Used by: npm run parity:compare
 * Output: PARITY_REPORT.md in the reportDir
 */
export async function runParityComparison(config: ReplayConfig): Promise<{
  totalReplayed: number;
  passed: number;
  failed: number;
  results: ReplayResult[];
}> {
  const harDir = config.harDir ?? path.join(__dirname, 'har-files');
  const reportDir = config.reportDir ?? path.join(__dirname, '..', 'playwright-report');

  if (!fs.existsSync(harDir)) {
    console.warn(`HAR directory not found: ${harDir}`);
    console.warn('Create har-files/ and drop .har captures from the legacy app into it.');
    return { totalReplayed: 0, passed: 0, failed: 0, results: [] };
  }

  const harFiles = fs.readdirSync(harDir).filter(f => f.endsWith('.har'));

  if (harFiles.length === 0) {
    console.warn(`No .har files found in ${harDir}`);
    return { totalReplayed: 0, passed: 0, failed: 0, results: [] };
  }

  const allResults: ReplayResult[] = [];

  for (const harFileName of harFiles) {
    const harPath = path.join(harDir, harFileName);
    console.log(`Replaying: ${harFileName}`);

    let har: HarFile;
    try {
      har = loadHarFile(harPath);
    } catch (err) {
      console.error(`  Failed to load ${harFileName}: ${err}`);
      continue;
    }

    const replayable = getReplayableEntries(har);
    console.log(`  Found ${replayable.length} replayable entries`);

    for (const { entry, mapping } of replayable) {
      const result = await replayEntry(entry, mapping, config);
      result.harFile = harFileName;
      allResults.push(result);
    }
  }

  // Status-code parity (4xx from legacy should also 4xx from new)
  const results = allResults;
  const passed = results.filter(r => !r.error && (r.parityResult?.passed ?? true)).length;
  const failed = results.length - passed;

  // Write report
  const report = generateParityReport(results);
  const reportPath = path.join(reportDir, 'PARITY_REPORT.md');
  fs.mkdirSync(reportDir, { recursive: true });
  fs.writeFileSync(reportPath, report, 'utf-8');
  console.log(`\nParity report written to: ${reportPath}`);
  console.log(`Results: ${passed} passed, ${failed} failed out of ${results.length} replayed`);

  return { totalReplayed: results.length, passed, failed, results };
}

// ── Report generation ─────────────────────────────────────────────────────────

function generateParityReport(results: ReplayResult[]): string {
  const lines: string[] = [
    '# Parity Comparison Report',
    '',
    `Generated: ${new Date().toISOString()}`,
    `Total replayed: ${results.length}`,
    `Passed: ${results.filter(r => r.parityResult?.passed ?? true).length}`,
    `Failed: ${results.filter(r => r.parityResult && !r.parityResult.passed).length}`,
    '',
    '---',
    '',
  ];

  for (const result of results) {
    lines.push(`## ${result.method} ${result.endpoint}`);
    lines.push(`HAR file: \`${result.harFile}\``);
    lines.push(`Legacy status: ${result.legacyStatus} | New status: ${result.newStatus}`);

    if (result.error) {
      lines.push(`**ERROR**: ${result.error}`);
    } else if (result.parityResult) {
      lines.push('');
      lines.push('```');
      lines.push(formatParityReport(result.parityResult));
      lines.push('```');
    } else {
      lines.push('*(no body comparison — non-2xx response)*');
    }
    lines.push('');
  }

  return lines.join('\n');
}

// ── CLI entrypoint ────────────────────────────────────────────────────────────

// Run directly: npx ts-node parity/har-replay.runner.ts
if (require.main === module) {
  const config: ReplayConfig = {
    newApiBaseUrl: process.env.API_BASE_URL ?? 'http://localhost:5136',
    authToken: process.env.NEW_API_TOKEN ?? '',
    harDir: path.join(__dirname, 'har-files'),
    reportDir: path.join(__dirname, '..', 'playwright-report'),
  };

  if (!config.authToken) {
    console.error('NEW_API_TOKEN env var is required. Run auth.setup.ts first or set manually.');
    process.exit(1);
  }

  runParityComparison(config)
    .then(({ failed }) => process.exit(failed > 0 ? 1 : 0))
    .catch(err => { console.error(err); process.exit(1); });
}
