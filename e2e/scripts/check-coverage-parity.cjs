#!/usr/bin/env node
/**
 * CI Parity Gate — Coverage Check
 *
 * Fails if any registered workflow/module lacks tests in all three tiers:
 *   UI    — tests that navigate/interact via Playwright page.*
 *   API   — tests that call backend endpoints via apiRequest
 *   DB    — tests that assert on database state via db.* or db query helpers
 *
 * Run: node scripts/check-coverage-parity.cjs
 */

'use strict';

const fs = require('fs');
const path = require('path');

const TESTS_DIR = path.join(__dirname, '..', 'tests');

const MODULES = [
  { name: 'purchase-order', dir: 'purchase-order' },
  { name: 'tax-invoice', dir: 'tax-invoice' },
  { name: 'delivery-challan', dir: 'delivery-challan' },
  { name: 'labour-charge-invoice', dir: 'labour-charge-invoice' },
];

function readSpecFiles(moduleDir) {
  const dirPath = path.join(TESTS_DIR, moduleDir);
  if (!fs.existsSync(dirPath)) return '';
  return fs.readdirSync(dirPath)
    .filter(f => f.endsWith('.spec.ts'))
    .map(f => fs.readFileSync(path.join(dirPath, f), 'utf8'))
    .join('\n');
}

function checkCoverage() {
  return MODULES.map(mod => {
    const content = readSpecFiles(mod.dir);

    // UI tier: tests that interact with a browser via Playwright page object
    const hasUI = /\bpage\.(goto|click|fill|locator|waitFor)\b/.test(content);

    // API tier: tests that make HTTP calls to the backend
    const hasAPI = /\bapiRequest\s*\(/.test(content);

    // DB tier: tests that assert on database state
    const hasDB = /\bdb\.(queryOne|query)\b|getLatest|getPoBy|getTaxInvoice|getChallan|getStockEntries|isTaxInvoice/.test(content);

    return { name: mod.name, hasUI, hasAPI, hasDB };
  });
}

function main() {
  const results = checkCoverage();

  let anyFailed = false;
  const lines = ['', '=== Coverage Parity Report ===', ''];

  for (const r of results) {
    const ui = r.hasUI ? '✓' : '✗';
    const api = r.hasAPI ? '✓' : '✗';
    const db = r.hasDB ? '✓' : '✗';
    const ok = r.hasUI && r.hasAPI && r.hasDB;
    lines.push(`${ok ? 'PASS' : 'FAIL'} [${r.name}]  UI=${ui}  API=${api}  DB=${db}`);
    if (!ok) anyFailed = true;
  }

  lines.push('');
  lines.push(anyFailed
    ? 'RESULT: FAILED — some modules lack complete coverage. Add the missing test tier.'
    : 'RESULT: PASSED — all modules have UI + API + DB coverage.'
  );
  lines.push('');

  console.log(lines.join('\n'));

  if (anyFailed) process.exit(1);
}

main();
