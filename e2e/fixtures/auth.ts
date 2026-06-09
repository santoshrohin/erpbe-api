/**
 * Auth helper — performs a real API login call and returns the JWT + user profile.
 *
 * The backend uses legacy XOR encryption for passwords (LegacyEncryption.cs).
 * The frontend sends the raw password and the backend handles decryption.
 * We send the raw password here too — same as the frontend does.
 */

import { expect } from '@playwright/test';

export interface LoginCredentials {
  username: string;
  password: string;
  companyId: number;
  financialYearCode: number;
}

export interface AuthResult {
  accessToken: string;
  user: {
    username: string;
    companyId: number;
    financialYearCode: number;
    roles: string[];
    permissions?: Record<string, string>;
  };
}

export async function loginViaApi(
  apiUrl: string,
  credentials: LoginCredentials
): Promise<AuthResult> {
  const response = await fetch(`${apiUrl}/Login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials),
  });

  if (!response.ok) {
    const body = await response.text();
    throw new Error(
      `Login failed: ${response.status} ${response.statusText}\n${body}`
    );
  }

  const data = await response.json();

  if (!data.accessToken) {
    throw new Error(`Login response missing accessToken: ${JSON.stringify(data)}`);
  }

  // Decode user info from JWT payload (no signature verification needed here)
  const payload = JSON.parse(
    Buffer.from(data.accessToken.split('.')[1], 'base64').toString('utf-8')
  );

  const roles: string[] = Array.isArray(
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
  )
    ? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    : [payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']].filter(Boolean);

  return {
    accessToken: data.accessToken,
    user: {
      username: credentials.username,
      companyId: credentials.companyId,
      financialYearCode: credentials.financialYearCode,
      roles,
      permissions: data.permissions ?? {},
    },
  };
}

/**
 * Makes an authenticated API request using the JWT from the auth result.
 * Use this in tests to call the API directly without going through the UI.
 */
export async function apiRequest(
  apiUrl: string,
  accessToken: string,
  method: string,
  path: string,
  body?: unknown
): Promise<{ status: number; data: unknown }> {
  const response = await fetch(`${apiUrl}${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  let data: unknown;
  const contentType = response.headers.get('content-type') ?? '';
  if (contentType.includes('application/json')) {
    data = await response.json();
  } else if (contentType.includes('application/pdf') || contentType.includes('octet-stream')) {
    data = await response.arrayBuffer();
  } else {
    data = await response.text();
  }

  return { status: response.status, data };
}
