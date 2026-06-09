import { defineConfig, devices } from '@playwright/test';

/**
 * ERP E2E Test Configuration
 *
 * Target: React frontend ↔ .NET API ↔ SQL Server
 * Auth:   JWT injected into sessionStorage before each test worker starts
 * DB:     mssql direct connection for post-action assertions
 *
 * Environment variables (override via .env.test or CI):
 *   BASE_URL      - React frontend URL  (default: http://localhost:5173)
 *   API_URL       - .NET API base URL   (default: http://localhost:5136/api)
 *   DB_SERVER     - SQL Server host     (default: localhost)
 *   DB_DATABASE   - Database name       (default: db_a2ea4b_sunv2)
 *   DB_USER       - SQL Server user     (default: db_a2ea4b_sunv2_admin)
 *   DB_PASSWORD   - SQL Server password
 *   TEST_USERNAME - ERP login username  (default: Mohan)
 *   TEST_PASSWORD - ERP login password  (default: 1234)
 *   TEST_COMPANY_ID - companyId         (default: 1)
 */

export default defineConfig({
  testDir: '.',
  testMatch: ['tests/**/*.spec.ts', 'parity/**/*.spec.ts', 'contract/**/*.spec.ts'],

  fullyParallel: false, // Serial within each file — DB state must be predictable
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,

  // One worker in CI to keep DB assertions deterministic; 2 locally for speed
  workers: process.env.CI ? 1 : 2,

  reporter: [
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
    ['list'],
  ],

  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:5173',

    // Capture on failure only
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',

    // Sufficient for ERP form submissions
    actionTimeout: 15_000,
    navigationTimeout: 30_000,
  },

  projects: [
    // ── Setup: authenticate once and save state ────────────────────────────────
    {
      name: 'setup',
      testMatch: '**/fixtures/auth.setup.ts',
    },

    // ── Chrome: all tests reuse the saved auth state ───────────────────────────
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: '.auth/admin.json',
      },
      dependencies: ['setup'],
    },
  ],

  // Start the React dev server locally if not running in Docker
  // Comment this out when using docker-compose.test.yml
  // webServer: {
  //   command: 'cd ../../FE_React && npm run dev',
  //   url: 'http://localhost:5173',
  //   reuseExistingServer: true,
  //   timeout: 60_000,
  // },
});
