/**
 * Layer 6 — Parity Diff Engine
 *
 * Compares legacy ERP responses against new ERP API responses field-by-field.
 * Produces structured diffs that map to PARITY_MATRIX.md entries.
 *
 * WHAT THIS IS FOR:
 *   The legacy ASPX app at /Users/kavitamhaske/Documents/LegacySunElectro
 *   served JSON from PageMethods (ASMX). HAR captures from that app give us
 *   ground-truth response shapes. This engine diffs those against new API
 *   responses to find field renames, missing fields, or computation differences.
 *
 * DIFF CATEGORIES:
 *   MISSING   — field present in legacy, absent in new
 *   EXTRA     — field present in new, absent in legacy (usually safe)
 *   MISMATCH  — field present in both but value differs (after normalization)
 *   TYPE      — same field but different JS types (string vs number)
 *
 * NORMALIZATION:
 *   Before comparing, values are normalized to remove representation noise:
 *   - Dates: both parsed to UTC ISO string (handles "MM/DD/YYYY" vs "YYYY-MM-DDT...")
 *   - Numbers: both rounded to 2 dp (removes floating-point drift)
 *   - Strings: trimmed (removes trailing whitespace common in legacy data)
 *   - Null/undefined: treated as equivalent (legacy sometimes omits, new sends null)
 */

// ── Types ─────────────────────────────────────────────────────────────────────

export type DiffCategory = 'MISSING' | 'EXTRA' | 'MISMATCH' | 'TYPE' | 'OK';

export interface FieldDiff {
  path: string;
  category: DiffCategory;
  legacyValue: unknown;
  newValue: unknown;
  note?: string;
}

export interface ParityDiffResult {
  endpoint: string;
  method: string;
  totalFields: number;
  diffs: FieldDiff[];
  missing: FieldDiff[];
  mismatches: FieldDiff[];
  extras: FieldDiff[];
  /** Fields in knownDeviations are expected to differ — excluded from failure count. */
  unexpectedDiffs: FieldDiff[];
  passed: boolean;
}

/**
 * Known deviations: paths that are expected to differ between legacy and new.
 * Add entries here for each accepted difference so they don't block CI.
 *
 * Format: 'endpoint:path' or just 'path' to match across all endpoints.
 */
export const KNOWN_DEVIATIONS: Array<{
  path: string | RegExp;
  endpoint?: string;
  reason: string;
}> = [
  // Legacy uses numeric 1/0 for BIT columns; new API serializes as boolean
  { path: /\.(MODIFY|ES_DELETE)$/, reason: 'BIT column: legacy returns 0/1, new returns boolean' },

  // Legacy date format: "MM/DD/YYYY HH:mm:ss"; new: ISO 8601
  { path: /Date$/, reason: 'Date format normalization: legacy MM/DD/YYYY vs new ISO 8601' },

  // Legacy sends CPOM_AM_COUNT as string; new sends as number
  { path: 'CPOM_AM_COUNT', reason: 'Legacy returns amendment count as string' },

  // Legacy TaxInvoice serializes as "INM_TYPE" field; new omits it from GET responses
  { path: 'INM_TYPE', reason: 'Legacy includes internal type discriminator; new does not expose it' },
];

// ── Normalization ──────────────────────────────────────────────────────────────

/**
 * Normalize a value before comparison so representation differences don't
 * create false positives.
 */
export function normalizeValue(value: unknown): unknown {
  if (value === null || value === undefined) return null;

  if (typeof value === 'string') {
    const trimmed = value.trim();

    // Date strings: normalize to UTC epoch ms for comparison
    if (trimmed && isDateString(trimmed)) {
      const parsed = new Date(trimmed).getTime();
      return isNaN(parsed) ? trimmed : parsed;
    }

    return trimmed;
  }

  if (typeof value === 'number') {
    // Round to 2 dp to absorb floating-point drift
    return Math.round(value * 100) / 100;
  }

  // BIT columns: coerce to boolean
  if (value === 1 || value === true) return true;
  if (value === 0 || value === false) return false;

  return value;
}

function isDateString(s: string): boolean {
  // Matches ISO 8601, MM/DD/YYYY, DD-MM-YYYY, and similar
  return (
    /^\d{4}-\d{2}-\d{2}(T.*)?$/.test(s) ||
    /^\d{1,2}\/\d{1,2}\/\d{4}/.test(s) ||
    /^\d{1,2}-\d{1,2}-\d{4}/.test(s)
  );
}

// ── Path matching ──────────────────────────────────────────────────────────────

function isKnownDeviation(path: string, endpoint: string): boolean {
  return KNOWN_DEVIATIONS.some(dev => {
    const pathMatches =
      dev.path instanceof RegExp
        ? dev.path.test(path)
        : path === dev.path || path.endsWith(`.${dev.path}`);
    const endpointMatches = !dev.endpoint || endpoint.includes(dev.endpoint);
    return pathMatches && endpointMatches;
  });
}

// ── Core diff ─────────────────────────────────────────────────────────────────

/**
 * Recursively diff two objects. Returns a flat list of FieldDiff entries.
 */
export function diffObjects(
  legacy: Record<string, unknown>,
  next: Record<string, unknown>,
  parentPath = ''
): FieldDiff[] {
  const diffs: FieldDiff[] = [];
  const allKeys = new Set([...Object.keys(legacy), ...Object.keys(next)]);

  for (const key of allKeys) {
    const path = parentPath ? `${parentPath}.${key}` : key;
    const hasLegacy = Object.prototype.hasOwnProperty.call(legacy, key);
    const hasNew = Object.prototype.hasOwnProperty.call(next, key);

    if (!hasLegacy) {
      diffs.push({ path, category: 'EXTRA', legacyValue: undefined, newValue: next[key] });
      continue;
    }

    if (!hasNew) {
      diffs.push({ path, category: 'MISSING', legacyValue: legacy[key], newValue: undefined });
      continue;
    }

    const legacyVal = legacy[key];
    const newVal = next[key];

    // Recurse into objects
    if (
      legacyVal !== null &&
      newVal !== null &&
      typeof legacyVal === 'object' &&
      typeof newVal === 'object' &&
      !Array.isArray(legacyVal) &&
      !Array.isArray(newVal)
    ) {
      diffs.push(
        ...diffObjects(
          legacyVal as Record<string, unknown>,
          newVal as Record<string, unknown>,
          path
        )
      );
      continue;
    }

    // Array: diff element by element (by index)
    if (Array.isArray(legacyVal) && Array.isArray(newVal)) {
      const len = Math.max(legacyVal.length, newVal.length);
      for (let i = 0; i < len; i++) {
        const indexPath = `${path}[${i}]`;
        if (i >= legacyVal.length) {
          diffs.push({ path: indexPath, category: 'EXTRA', legacyValue: undefined, newValue: newVal[i] });
        } else if (i >= newVal.length) {
          diffs.push({ path: indexPath, category: 'MISSING', legacyValue: legacyVal[i], newValue: undefined });
        } else if (typeof legacyVal[i] === 'object' && typeof newVal[i] === 'object') {
          diffs.push(
            ...diffObjects(
              legacyVal[i] as Record<string, unknown>,
              newVal[i] as Record<string, unknown>,
              indexPath
            )
          );
        } else {
          const normalizedLegacy = normalizeValue(legacyVal[i]);
          const normalizedNew = normalizeValue(newVal[i]);
          if (normalizedLegacy !== normalizedNew) {
            const category: DiffCategory =
              typeof legacyVal[i] !== typeof newVal[i] ? 'TYPE' : 'MISMATCH';
            diffs.push({ path: indexPath, category, legacyValue: legacyVal[i], newValue: newVal[i] });
          }
        }
      }
      continue;
    }

    // Scalar comparison
    const normalizedLegacy = normalizeValue(legacyVal);
    const normalizedNew = normalizeValue(newVal);

    if (normalizedLegacy !== normalizedNew) {
      const category: DiffCategory =
        typeof legacyVal !== typeof newVal ? 'TYPE' : 'MISMATCH';
      diffs.push({ path, category, legacyValue: legacyVal, newValue: newVal });
    }
  }

  return diffs;
}

// ── Full parity diff ──────────────────────────────────────────────────────────

/**
 * Compare a legacy response payload against a new API response payload.
 * Classifies diffs, filters known deviations, returns a pass/fail result.
 */
export function computeParityDiff(
  endpoint: string,
  method: string,
  legacyBody: Record<string, unknown>,
  newBody: Record<string, unknown>
): ParityDiffResult {
  const allDiffs = diffObjects(legacyBody, newBody);

  const missing = allDiffs.filter(d => d.category === 'MISSING');
  const mismatches = allDiffs.filter(d => d.category === 'MISMATCH' || d.category === 'TYPE');
  const extras = allDiffs.filter(d => d.category === 'EXTRA');

  const unexpectedDiffs = [...missing, ...mismatches].filter(
    d => !isKnownDeviation(d.path, endpoint)
  );

  return {
    endpoint,
    method,
    totalFields: Object.keys(legacyBody).length,
    diffs: allDiffs,
    missing,
    mismatches,
    extras,
    unexpectedDiffs,
    passed: unexpectedDiffs.length === 0,
  };
}

// ── Report formatting ─────────────────────────────────────────────────────────

/**
 * Render a human-readable parity report for logging or CI artifacts.
 */
export function formatParityReport(result: ParityDiffResult): string {
  const lines: string[] = [];
  const status = result.passed ? '✅ PASS' : '❌ FAIL';

  lines.push(`${status} — ${result.method} ${result.endpoint}`);
  lines.push(`Total legacy fields: ${result.totalFields}`);
  lines.push(`Missing in new: ${result.missing.length}`);
  lines.push(`Value mismatches: ${result.mismatches.length}`);
  lines.push(`Extra in new (safe): ${result.extras.length}`);
  lines.push(`Unexpected diffs (blocking): ${result.unexpectedDiffs.length}`);

  if (result.unexpectedDiffs.length > 0) {
    lines.push('');
    lines.push('── Unexpected differences (must fix) ──');
    for (const d of result.unexpectedDiffs) {
      lines.push(`  [${d.category}] ${d.path}`);
      lines.push(`    Legacy: ${JSON.stringify(d.legacyValue)}`);
      lines.push(`    New:    ${JSON.stringify(d.newValue)}`);
      if (d.note) lines.push(`    Note:   ${d.note}`);
    }
  }

  if (result.missing.length > 0) {
    const knownMissing = result.missing.filter(d => isKnownDeviation(d.path, result.endpoint));
    if (knownMissing.length > 0) {
      lines.push('');
      lines.push('── Missing (known deviation — not blocking) ──');
      for (const d of knownMissing) {
        lines.push(`  ${d.path}: legacy=${JSON.stringify(d.legacyValue)}`);
      }
    }
  }

  return lines.join('\n');
}
