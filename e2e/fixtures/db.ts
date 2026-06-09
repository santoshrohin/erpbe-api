/**
 * Database fixture for Playwright E2E tests.
 *
 * Provides a direct SQL Server connection so tests can:
 *   - Assert database state after UI actions
 *   - Verify that creates/updates/deletes actually persisted
 *   - Check stock ledger entries, audit columns, lock flags
 *   - Verify soft-delete vs hard-delete behavior
 *
 * Connection: reads from environment variables so the same tests
 * work both locally (hosted DB) and in Docker (containerized DB).
 *
 * Usage:
 *   import { test } from '../fixtures';   ← use the combined fixture
 *   test('...', async ({ page, db }) => {
 *     await page.click('...');
 *     const row = await db.queryOne('SELECT * FROM CUSTPO_MASTER WHERE ...');
 *     expect(row.CPOM_PONO).toBe('...');
 *   });
 */

import sql from 'mssql';

export interface DbConfig {
  server: string;
  database: string;
  user: string;
  password: string;
  options?: {
    encrypt?: boolean;
    trustServerCertificate?: boolean;
  };
}

export class DbClient {
  private pool: sql.ConnectionPool | null = null;
  private config: sql.config;

  constructor(config: DbConfig) {
    this.config = {
      server: config.server,
      database: config.database,
      user: config.user,
      password: config.password,
      options: {
        encrypt: config.options?.encrypt ?? false,
        trustServerCertificate: config.options?.trustServerCertificate ?? true,
      },
      connectionTimeout: 15_000,
      requestTimeout: 30_000,
    };
  }

  async connect(): Promise<void> {
    this.pool = await sql.connect(this.config);
  }

  async disconnect(): Promise<void> {
    if (this.pool) {
      await this.pool.close();
      this.pool = null;
    }
  }

  /** Execute a query and return all rows. */
  async query<T = Record<string, unknown>>(
    queryText: string,
    params?: Record<string, unknown>
  ): Promise<T[]> {
    if (!this.pool) throw new Error('DB not connected — call connect() first');
    const req = this.pool.request();
    if (params) {
      for (const [key, value] of Object.entries(params)) {
        req.input(key, value);
      }
    }
    const result = await req.query(queryText);
    return result.recordset as T[];
  }

  /** Execute a query and return the first row, or null if empty. */
  async queryOne<T = Record<string, unknown>>(
    queryText: string,
    params?: Record<string, unknown>
  ): Promise<T | null> {
    const rows = await this.query<T>(queryText, params);
    return rows[0] ?? null;
  }

  /** Execute a command (INSERT/UPDATE/DELETE) and return rows affected. */
  async execute(
    queryText: string,
    params?: Record<string, unknown>
  ): Promise<number> {
    if (!this.pool) throw new Error('DB not connected — call connect() first');
    const req = this.pool.request();
    if (params) {
      for (const [key, value] of Object.entries(params)) {
        req.input(key, value);
      }
    }
    const result = await req.query(queryText);
    return result.rowsAffected[0] ?? 0;
  }

  /** Wait for a condition to be true, polling the DB every 500ms. */
  async waitFor<T = Record<string, unknown>>(
    queryText: string,
    predicate: (row: T | null) => boolean,
    options?: { timeoutMs?: number; intervalMs?: number }
  ): Promise<T | null> {
    const timeout = options?.timeoutMs ?? 10_000;
    const interval = options?.intervalMs ?? 500;
    const deadline = Date.now() + timeout;

    while (Date.now() < deadline) {
      const row = await this.queryOne<T>(queryText);
      if (predicate(row)) return row;
      await new Promise(r => setTimeout(r, interval));
    }
    throw new Error(`DB condition not met within ${timeout}ms for: ${queryText}`);
  }
}

/** Build a DbClient from environment variables. */
export function createDbClient(): DbClient {
  return new DbClient({
    server: process.env.DB_SERVER || 'SQL5111.site4now.net',
    database: process.env.DB_DATABASE || 'db_a2ea4b_sunv2',
    user: process.env.DB_USER || 'db_a2ea4b_sunv2_admin',
    password: process.env.DB_PASSWORD || 'abcd@1234',
    options: {
      encrypt: false,
      trustServerCertificate: true,
    },
  });
}
