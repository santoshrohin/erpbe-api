/**
 * Playwright network capture utilities.
 *
 * Intercepts real browser HTTP requests at the network level — not mocked,
 * not proxied. Whatever the browser physically sends is what gets captured.
 * This closes the gap that MSW tests and direct API tests cannot close:
 * "does the real UI form produce the correct request body?"
 *
 * Usage:
 *   const capture = interceptRequest(page, /api\/CustomerPo\/\d+/, 'PUT');
 *   await page.locator('button[type="submit"]').click();
 *   const req = await capture.next();
 *   assertPoUpdateBody(req.body, poCode);
 *   await capture.done();
 */

import type { Page, Route, Request } from '@playwright/test';

export interface CapturedRequest {
  method: string;
  url: string;
  /** Parsed JSON body, or null if the request has no body */
  body: Record<string, unknown> | null;
  /** All request headers */
  headers: Record<string, string>;
  /** URL pathname only e.g. /api/CustomerPo/104 */
  pathname: string;
  /** Last path segment e.g. "104" */
  resourceId: string | null;
}

export interface RequestCapture {
  /** Resolves with the next matching request, or rejects after timeout */
  next(timeoutMs?: number): Promise<CapturedRequest>;
  /** Remove the route handler */
  done(): Promise<void>;
}

/**
 * Intercepts requests matching urlPattern + method and resolves them via next().
 * By default routes are CONTINUED (real request goes through).
 * Pass fulfill to short-circuit with a fake response (prevents real DB writes).
 */
export function interceptRequest(
  page: Page,
  urlPattern: string | RegExp,
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH',
  options: {
    /** If provided, the request is fulfilled with this response instead of continuing */
    fulfill?: { status: number; body?: unknown };
  } = {}
): RequestCapture {
  const queue: CapturedRequest[] = [];
  const waiters: Array<(r: CapturedRequest) => void> = [];

  const handler = async (route: Route, request: Request) => {
    if (request.method() !== method) {
      await route.continue();
      return;
    }

    let body: Record<string, unknown> | null = null;
    try {
      const raw = request.postData();
      if (raw) body = JSON.parse(raw) as Record<string, unknown>;
    } catch {
      // non-JSON body — leave as null
    }

    const url = new URL(request.url());
    const segments = url.pathname.split('/').filter(Boolean);
    const captured: CapturedRequest = {
      method: request.method(),
      url: request.url(),
      body,
      headers: await request.allHeaders(),
      pathname: url.pathname,
      resourceId: segments.at(-1) ?? null,
    };

    if (waiters.length > 0) {
      waiters.shift()!(captured);
    } else {
      queue.push(captured);
    }

    if (options.fulfill) {
      await route.fulfill({
        status: options.fulfill.status,
        contentType: 'application/json',
        body: options.fulfill.body !== undefined
          ? JSON.stringify(options.fulfill.body)
          : '{}',
      });
    } else {
      await route.continue();
    }
  };

  page.route(urlPattern, handler);

  return {
    next(timeoutMs = 10_000): Promise<CapturedRequest> {
      if (queue.length > 0) {
        return Promise.resolve(queue.shift()!);
      }
      return new Promise<CapturedRequest>((resolve, reject) => {
        const timer = setTimeout(
          () => reject(new Error(`interceptRequest timed out after ${timeoutMs}ms waiting for ${method} ${String(urlPattern)}`)),
          timeoutMs
        );
        waiters.push(r => {
          clearTimeout(timer);
          resolve(r);
        });
      });
    },
    done: () => page.unroute(urlPattern, handler),
  };
}

/**
 * Shorthand: intercepts ONE request, returns it, then cleans up.
 *
 * Useful when you only need to assert a single request body:
 *   const req = await captureOneRequest(page, /api\/CustomerPo\/\d+/, 'PUT', { status: 200, body: {} });
 */
export async function captureOneRequest(
  page: Page,
  urlPattern: string | RegExp,
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH',
  fulfill: { status: number; body?: unknown }
): Promise<CapturedRequest> {
  const capture = interceptRequest(page, urlPattern, method, { fulfill });
  const req = await capture.next();
  await capture.done();
  return req;
}
