/**
 * Auth Setup — runs ONCE before any test worker starts.
 *
 * Why: Logging in via UI on every test wastes ~3-5 seconds per test
 * and hits the login rate limiter. Instead we:
 *   1. POST /api/Login to get a JWT access token
 *   2. Inject it into sessionStorage as 'erp_session_token'
 *   3. Inject user profile into localStorage as 'user'
 *   4. Save the browser storage state to .auth/admin.json
 *
 * All test workers then start with this pre-authenticated state.
 * The token is valid for the configured AccessTokenExpiryMinutes.
 */

import { test as setup, expect } from '@playwright/test';
import path from 'path';
import { loginViaApi } from './auth';

const STORAGE_STATE_PATH = path.join(__dirname, '..', '.auth', 'admin.json');

setup('authenticate as admin', async ({ page }) => {
  const apiUrl = process.env.API_URL || 'http://localhost:5136/api';
  const username = process.env.TEST_USERNAME || 'Mohan';
  const password = process.env.TEST_PASSWORD || '1234';
  const companyId = parseInt(process.env.TEST_COMPANY_ID || '1');
  const financialYearCode = parseInt(process.env.TEST_FY_CODE || '-2147483641');

  const { accessToken, user } = await loginViaApi(apiUrl, {
    username,
    password,
    companyId,
    financialYearCode,
  });

  // Inject token BEFORE React starts so the Zustand auth store finds it during
  // initialization. page.evaluate() after goto('/login') is too late — the store
  // already initialised with isAuthenticated=false on the login page.
  await page.addInitScript(
    ({ token, userData }) => {
      sessionStorage.setItem('erp_session_token', token);
      localStorage.setItem('user', JSON.stringify(userData));
    },
    { token: accessToken, userData: user }
  );

  // Navigate directly to a protected route — addInitScript fires before React runs.
  await page.goto('/transactions/purchase-order');
  await page.waitForLoadState('networkidle');
  await expect(page).not.toHaveURL(/\/login/, { timeout: 10_000 });

  await page.context().storageState({ path: STORAGE_STATE_PATH });
  console.log(`Auth state saved to ${STORAGE_STATE_PATH}`);
});
