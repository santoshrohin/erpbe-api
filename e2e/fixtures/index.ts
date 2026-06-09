/**
 * Combined test fixture — extends Playwright's `test` with:
 *   - `db`: DbClient connected to SQL Server
 *   - `apiUrl`: base URL for direct API calls
 *   - `accessToken`: JWT obtained via login API (worker-scoped, one login per worker)
 *   - `companyId`: from env / auth
 *
 * Import this instead of '@playwright/test' in all spec files:
 *   import { test, expect } from '../fixtures';
 */

import { test as base, expect } from '@playwright/test';
import { DbClient, createDbClient } from './db';
import { loginViaApi } from './auth';

export { expect };

interface ErpFixtures {
  db: DbClient;
  apiUrl: string;
  companyId: number;
  accessToken: string;
  // `page` is overridden to inject sessionStorage auth token before each navigation.
  // Playwright's storageState() does NOT capture sessionStorage — only localStorage
  // and cookies. The auth store reads `erp_session_token` from sessionStorage, so
  // without this injection every UI test would land on the login page.
  page: import('@playwright/test').Page;
}

// Worker-scoped so login runs once per worker, not once per test
interface ErpWorkerFixtures {
  workerAccessToken: string;
}

export const test = base.extend<ErpFixtures, ErpWorkerFixtures>({
  db: async ({}, use) => {
    const client = createDbClient();
    await client.connect();
    await use(client);
    await client.disconnect();
  },

  apiUrl: async ({}, use) => {
    await use(process.env.API_URL || 'http://localhost:5136/api');
  },

  companyId: async ({}, use) => {
    await use(parseInt(process.env.TEST_COMPANY_ID || '1'));
  },

  // Worker-scoped: login once per worker via API (no browser page needed)
  workerAccessToken: [async ({}, use) => {
    const apiUrl = process.env.API_URL || 'http://localhost:5136/api';
    const { accessToken } = await loginViaApi(apiUrl, {
      username: process.env.TEST_USERNAME || 'Mohan',
      password: process.env.TEST_PASSWORD || '1234',
      companyId: parseInt(process.env.TEST_COMPANY_ID || '1'),
      financialYearCode: parseInt(process.env.TEST_FY_CODE || '-2147483641'),
    });
    await use(accessToken);
  }, { scope: 'worker' }],

  accessToken: async ({ workerAccessToken }, use) => {
    await use(workerAccessToken);
  },

  // Override `page` to inject the JWT token into sessionStorage before every
  // page navigation. Playwright's storageState only preserves localStorage and
  // cookies; sessionStorage is ephemeral and is reset on each new context.
  // addInitScript fires synchronously before React initialises on every goto(),
  // so the auth store's readSessionToken() always finds a valid token.
  page: async ({ page, workerAccessToken }, use) => {
    await page.addInitScript((token: string) => {
      sessionStorage.setItem('erp_session_token', token);
    }, workerAccessToken);
    await use(page);
  },
});

/** Unique PO/Invoice number for this test run — avoids DB collisions between parallel runs. */
export function uniqueRef(prefix: string): string {
  return `${prefix}-E2E-${Date.now().toString(36).toUpperCase()}`;
}

/** Today's date in YYYY-MM-DD format (used for form inputs). */
export function todayIso(): string {
  return new Date().toISOString().split('T')[0];
}

/** Verify that a toast/success message appears after a form submission. */
export async function expectSuccessToast(page: import('@playwright/test').Page): Promise<void> {
  // The app uses a ToastProvider with role="status" or class containing "toast"
  const toast = page.locator('[role="status"], [class*="toast"], [class*="Toast"]').first();
  await expect(toast).toBeVisible({ timeout: 8_000 });
}
