/**
 * Domain-specific DB query helpers.
 *
 * Each function wraps a raw SQL query with meaningful naming so test files
 * read as business language, not SQL.
 *
 * COLUMN NAME NOTES (critical — wrong names cause silent 0/null returns):
 *   STOCK_LEDGER:             STL_I_CODE, STL_DOC_QTY, STL_DOC_NO, STL_DOC_TYPE, STL_STORE_TYPE
 *   INVOICE_DETAIL:           IND_INM_CODE, IND_I_CODE, IND_INQTY, IND_RATE, IND_AMT, E_BASIC_CentralT, E_EDU_CESS_State
 *   CUSTPO_DETAIL:            CPOD_CPOM_CODE, CPOD_I_CODE, CPOD_ORD_QTY, CPOD_RATE, CPOD_AMT
 *   DELIVERY_CHALLAN_MASTER:  DCM_CODE, DCM_CM_CODE, DCM_P_CODE (NOT DC_*)
 *   DELIVERY_CHALLAN_DETAIL:  DCD_DCM_CODE, DCD_I_CODE, DCD_ORD_QTY, DCD_UM_CODE
 *   INVOICE_MASTER (Labour):  INM_TYPE='OutJWINM' distinguishes from Tax Invoice (INM_TYPE='TAXINV')
 *
 * BIT COLUMNS (mssql v11 returns as JS boolean, NOT 0/1):
 *   Always use toBeTruthy()/toBeFalsy() — never toBe(1)/toBe(0)
 */

import type { DbClient } from '../fixtures/db';

// ── Purchase Order ────────────────────────────────────────────────────────────

export interface CustPoRow {
  CPOM_CODE: number;
  CPOM_P_CODE: number;
  CPOM_PONO: string;
  CPOM_TYPE: number;
  CPOM_DATE: Date;
  CPOM_CM_COMP_ID: number;
  CPOM_GRAND_TOT: number | null;
  CPOM_BASIC_AMT: number | null;
  CPOM_T_PER: number | null;
  CPOM_PROJECT_CODE: number | null;
  CPOM_PAY_TERM: string | null;
  CPOM_AM_COUNT: number | null;
  CPOM_AM_DATE: Date | null;
  MODIFY: boolean | number | null;
  ES_DELETE: boolean | number;
}

export interface CustPoDetailRow {
  CPOD_CPOM_CODE: number;
  CPOD_I_CODE: number;
  CPOD_UOM_CODE: number;
  CPOD_ORD_QTY: number;
  CPOD_RATE: number;
  CPOD_AMT: number | null;
  CPOD_ST_CODE: number | null;
}

export async function getLatestPo(db: DbClient, companyId: number): Promise<CustPoRow | null> {
  return db.queryOne<CustPoRow>(
    `SELECT TOP 1 * FROM CUSTPO_MASTER
     WHERE CPOM_CM_COMP_ID = @companyId AND ES_DELETE = 0
     ORDER BY CPOM_CODE DESC`,
    { companyId }
  );
}

export async function getPoByCode(db: DbClient, poCode: number): Promise<CustPoRow | null> {
  return db.queryOne<CustPoRow>(
    `SELECT * FROM CUSTPO_MASTER WHERE CPOM_CODE = @poCode`,
    { poCode }
  );
}

export async function getPoByNumber(db: DbClient, poNumber: string): Promise<CustPoRow | null> {
  return db.queryOne<CustPoRow>(
    `SELECT * FROM CUSTPO_MASTER WHERE CPOM_PONO = @poNumber`,
    { poNumber }
  );
}

export async function getPoDetails(db: DbClient, poCode: number): Promise<CustPoDetailRow[]> {
  return db.query<CustPoDetailRow>(
    `SELECT CPOD_CPOM_CODE, CPOD_I_CODE, CPOD_UOM_CODE, CPOD_ORD_QTY, CPOD_RATE, CPOD_AMT, CPOD_ST_CODE
     FROM CUSTPO_DETAIL
     WHERE CPOD_CPOM_CODE = @poCode
     ORDER BY CPOD_I_CODE`,
    { poCode }
  );
}

export async function isPoLocked(db: DbClient, poCode: number): Promise<boolean> {
  const row = await db.queryOne<{ MODIFY: boolean | number }>(
    `SELECT MODIFY FROM CUSTPO_MASTER WHERE CPOM_CODE = @poCode`,
    { poCode }
  );
  return !!row?.MODIFY;
}

export async function isPoDeleted(db: DbClient, poCode: number): Promise<boolean> {
  const row = await db.queryOne<{ ES_DELETE: boolean | number }>(
    `SELECT ES_DELETE FROM CUSTPO_MASTER WHERE CPOM_CODE = @poCode`,
    { poCode }
  );
  return !!row?.ES_DELETE;
}

// ── Tax Invoice ───────────────────────────────────────────────────────────────

export interface TaxInvoiceRow {
  INM_CODE: number;
  INM_NO: number | null;
  INM_DATE: Date;
  INM_P_CODE: number;
  INM_CPOM_CODE: number | null;
  INM_CM_CODE: number;
  INM_TYPE: string | null;
  INM_NET_AMT: number | null;
  INM_G_AMT: number | null;
  INM_SUPPLEMENTORY?: boolean | number | null;
  INM_PARENT_CODE?: number | null;
  MODIFY: boolean | number;
  ES_DELETE: boolean | number;
}

export interface InvoiceDetailRow {
  IND_INM_CODE: number;
  IND_I_CODE: number;
  IND_UOM_CODE: number | null;
  IND_INQTY: number;
  IND_RATE: number | null;
  IND_AMT: number | null;
  E_BASIC_CentralT: number | null;  // CGST percentage column name in DB
  E_EDU_CESS_State: number | null;  // SGST percentage column name in DB
  ES_DELETE: boolean | number;
}

// Filter by INM_TYPE='TAXINV' to prevent Labour Invoice rows bleeding in.
// Both share INVOICE_MASTER; the type distinguishes them.
export async function getLatestTaxInvoice(
  db: DbClient,
  companyCode: number
): Promise<TaxInvoiceRow | null> {
  return db.queryOne<TaxInvoiceRow>(
    `SELECT TOP 1 INM_CODE, INM_NO, INM_DATE, INM_P_CODE, INM_CPOM_CODE,
                  INM_CM_CODE, INM_TYPE, INM_NET_AMT, INM_G_AMT, MODIFY, ES_DELETE
     FROM INVOICE_MASTER
     WHERE INM_CM_CODE = @companyCode
       AND ES_DELETE = 0
       AND ISNULL(INM_TYPE, '') = 'TAXINV'
     ORDER BY INM_CODE DESC`,
    { companyCode }
  );
}

export async function getTaxInvoiceByCode(
  db: DbClient,
  invoiceCode: number
): Promise<TaxInvoiceRow | null> {
  return db.queryOne<TaxInvoiceRow>(
    `SELECT INM_CODE, INM_NO, INM_DATE, INM_P_CODE, INM_CPOM_CODE,
            INM_CM_CODE, INM_TYPE, INM_NET_AMT, INM_G_AMT, MODIFY, ES_DELETE
     FROM INVOICE_MASTER
     WHERE INM_CODE = @invoiceCode`,
    { invoiceCode }
  );
}

export async function getTaxInvoiceDetails(
  db: DbClient,
  invoiceCode: number
): Promise<InvoiceDetailRow[]> {
  return db.query<InvoiceDetailRow>(
    `SELECT IND_INM_CODE, IND_I_CODE, IND_UOM_CODE, IND_INQTY,
            IND_RATE, IND_AMT, E_BASIC_CentralT, E_EDU_CESS_State, ES_DELETE
     FROM INVOICE_DETAIL
     WHERE IND_INM_CODE = @invoiceCode AND ES_DELETE = 0
     ORDER BY IND_I_CODE`,
    { invoiceCode }
  );
}

export async function isTaxInvoiceLocked(db: DbClient, invoiceCode: number): Promise<boolean> {
  const row = await db.queryOne<{ MODIFY: boolean | number }>(
    `SELECT MODIFY FROM INVOICE_MASTER WHERE INM_CODE = @invoiceCode`,
    { invoiceCode }
  );
  return !!row?.MODIFY;
}

// ── Stock Ledger ──────────────────────────────────────────────────────────────
// IMPORTANT: STOCK_LEDGER uses STL_* column names (NOT SL_*).
// Columns: STL_I_CODE, STL_DOC_QTY, STL_DOC_NO, STL_DOC_TYPE, STL_STORE_TYPE
// There is no per-company or per-store column — balance is global per item.

export async function getStockBalance(
  db: DbClient,
  itemCode: number
): Promise<number> {
  const row = await db.queryOne<{ total: number }>(
    `SELECT ISNULL(SUM(STL_DOC_QTY), 0) AS total
     FROM STOCK_LEDGER
     WHERE STL_I_CODE = @itemCode`,
    { itemCode }
  );
  return row?.total ?? 0;
}

// Returns sum of stock entries specifically for one document (e.g. one challan).
// Negative means stock was deducted.
export async function getStockEntriesForDoc(
  db: DbClient,
  docNo: number,
  docType: string
): Promise<number> {
  const row = await db.queryOne<{ total: number }>(
    `SELECT ISNULL(SUM(STL_DOC_QTY), 0) AS total
     FROM STOCK_LEDGER
     WHERE STL_DOC_NO = @docNo AND STL_DOC_TYPE = @docType`,
    { docNo, docType }
  );
  return row?.total ?? 0;
}

// ── Delivery Challan ──────────────────────────────────────────────────────────
// Table: DELIVERY_CHALLAN_MASTER — columns use DCM_* prefix (not DC_*)
// PK: DCM_CODE, company: DCM_CM_CODE, customer: DCM_P_CODE

export interface DeliveryChallanRow {
  DCM_CODE: number;
  DCM_NO: number | null;
  DCM_DATE: Date;
  DCM_P_CODE: number | null;
  DCM_CM_CODE: number | null;
  MODIFY: boolean | number | null;
  ES_DELETE: boolean | number | null;
}

export interface ChallanDetailRow {
  DCD_DCM_CODE: number;
  DCD_I_CODE: number | null;
  DCD_ORD_QTY: number;
  DCD_UM_CODE: number | null;
  ES_DELETE: boolean | number | null;
}

export async function getLatestChallan(
  db: DbClient,
  companyCode: number
): Promise<DeliveryChallanRow | null> {
  return db.queryOne<DeliveryChallanRow>(
    `SELECT TOP 1 DCM_CODE, DCM_CM_CODE, DCM_P_CODE, DCM_NO, DCM_DATE, MODIFY, ES_DELETE
     FROM DELIVERY_CHALLAN_MASTER
     WHERE DCM_CM_CODE = @companyCode AND ES_DELETE = 0
     ORDER BY DCM_CODE DESC`,
    { companyCode }
  );
}

export async function getChallanDetails(
  db: DbClient,
  dcCode: number
): Promise<ChallanDetailRow[]> {
  return db.query<ChallanDetailRow>(
    `SELECT DCD_DCM_CODE, DCD_I_CODE, DCD_ORD_QTY, DCD_UM_CODE, ES_DELETE
     FROM DELIVERY_CHALLAN_DETAIL
     WHERE DCD_DCM_CODE = @dcCode AND ES_DELETE = 0
     ORDER BY DCD_I_CODE`,
    { dcCode }
  );
}

// ── Customer PO Amendment Archive ─────────────────────────────────────────────
// CUSTPO_AM_MASTER: a snapshot of CUSTPO_MASTER taken before each AMEND call.
//   PK: CPOM_AM_CODE (IDENTITY, starts at -2147483648)
//   FK: CPOM_CODE → CUSTPO_MASTER.CPOM_CODE (the original PO)
// CUSTPO_AMD_DETAIL: archived detail rows, one snapshot per CUSTPO_AM_MASTER row.
//   FK: AMD_AM_CODE → CUSTPO_AM_MASTER.CPOM_AM_CODE

export interface CustPoAmMasterRow {
  CPOM_AM_CODE: number;       // identity PK of the archive row
  CPOM_CODE: number;          // original PO code (FK → CUSTPO_MASTER)
  CPOM_PONO: string | null;
  CPOM_AM_COUNT: number | null;
  CPOM_AM_DATE: Date | null;
  CPOM_GRAND_TOT: number | null;
  CPOM_PAY_TERM: string | null;
}

export interface CustPoAmdDetailRow {
  CPOD_CPOM_CODE: number;
  CPOD_I_CODE: number;
  CPOD_UOM_CODE: number;
  CPOD_ORD_QTY: number;
  CPOD_RATE: number;
  CPOD_AMT: number | null;
  AMD_AM_CODE: number;
}

export async function getAmendmentArchives(
  db: DbClient,
  poCode: number
): Promise<CustPoAmMasterRow[]> {
  return db.query<CustPoAmMasterRow>(
    `SELECT CPOM_AM_CODE, CPOM_CODE, CPOM_PONO, CPOM_AM_COUNT, CPOM_AM_DATE, CPOM_GRAND_TOT, CPOM_PAY_TERM
     FROM CUSTPO_AM_MASTER
     WHERE CPOM_CODE = @poCode
     ORDER BY CPOM_AM_CODE`,
    { poCode }
  );
}

export async function getAmendmentArchiveDetails(
  db: DbClient,
  amCode: number
): Promise<CustPoAmdDetailRow[]> {
  return db.query<CustPoAmdDetailRow>(
    `SELECT CPOD_CPOM_CODE, CPOD_I_CODE, CPOD_UOM_CODE, CPOD_ORD_QTY, CPOD_RATE, CPOD_AMT, AMD_AM_CODE
     FROM CUSTPO_AMD_DETAIL
     WHERE AMD_AM_CODE = @amCode
     ORDER BY CPOD_I_CODE`,
    { amCode }
  );
}

// ── Labour Charge Invoice ─────────────────────────────────────────────────────
// Labour invoices share INVOICE_MASTER with Tax Invoices.
// INM_TYPE = 'OutJWINM' distinguishes them from Tax Invoices (INM_TYPE = 'TAXINV').

export interface LabourInvoiceRow {
  INM_CODE: number;
  INM_NO: number | null;
  INM_DATE: Date;
  INM_P_CODE: number;
  INM_CM_CODE: number;
  INM_TYPE: string | null;
  MODIFY: boolean | number;
  ES_DELETE: boolean | number;
}

export async function getLatestLabourInvoice(
  db: DbClient,
  companyCode: number
): Promise<LabourInvoiceRow | null> {
  return db.queryOne<LabourInvoiceRow>(
    `SELECT TOP 1 INM_CODE, INM_CM_CODE, INM_P_CODE, INM_NO, INM_DATE, INM_TYPE, MODIFY, ES_DELETE
     FROM INVOICE_MASTER
     WHERE INM_CM_CODE = @companyCode AND ES_DELETE = 0 AND INM_TYPE = 'OutJWINM'
     ORDER BY INM_CODE DESC`,
    { companyCode }
  );
}
