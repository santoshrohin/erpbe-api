-- ============================================================
-- Test fixtures — specific records that Playwright tests depend on
-- IDs are hardcoded so tests can reference them by a known value
-- ============================================================

-- PARTY_MASTER (Customer) — P_CODE = 9001
-- Tests create POs and invoices against this customer
IF NOT EXISTS (SELECT 1 FROM [dbo].[PARTY_MASTER] WHERE P_CODE = 9001)
BEGIN
    SET IDENTITY_INSERT [dbo].[PARTY_MASTER] ON;
    INSERT INTO [dbo].[PARTY_MASTER]
        (P_CODE, P_CM_COMP_ID, P_TYPE, P_NAME, P_CONTACT, P_ADD1,
         P_PHONE, P_EMAIL, P_ACTIVE_IND, ES_DELETE)
    VALUES
        (9001, 1, 1, 'E2E Test Customer Ltd', 'Test Contact',
         '456 Test Ave, Pune', '020-98765432',
         'e2e@testcustomer.com', 1, 0);
    SET IDENTITY_INSERT [dbo].[PARTY_MASTER] OFF;
END
GO

-- ITEM_MASTER — I_CODE = 9001 (standard item)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ITEM_MASTER] WHERE I_CODE = 9001)
BEGIN
    SET IDENTITY_INSERT [dbo].[ITEM_MASTER] ON;
    INSERT INTO [dbo].[ITEM_MASTER]
        (I_CODE, I_NAME, I_CODENO, I_ACTIVE_IND, ES_DELETE)
    VALUES
        (9001, 'E2E Test Item Alpha', 'E2E-ALPHA-001', 1, 0);
    SET IDENTITY_INSERT [dbo].[ITEM_MASTER] OFF;
END
GO

-- ITEM_MASTER — I_CODE = 9002 (second item for multi-line tests)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ITEM_MASTER] WHERE I_CODE = 9002)
BEGIN
    SET IDENTITY_INSERT [dbo].[ITEM_MASTER] ON;
    INSERT INTO [dbo].[ITEM_MASTER]
        (I_CODE, I_NAME, I_CODENO, I_ACTIVE_IND, ES_DELETE)
    VALUES
        (9002, 'E2E Test Item Beta', 'E2E-BETA-002', 1, 0);
    SET IDENTITY_INSERT [dbo].[ITEM_MASTER] OFF;
END
GO

-- CUSTPO_MASTER — CPOM_CODE = 9001 (base PO for TaxInvoice tests)
-- This PO must exist before Tax Invoice tests run (TaxInvoice references a PO)
IF NOT EXISTS (SELECT 1 FROM [dbo].[CUSTPO_MASTER] WHERE CPOM_CODE = 9001)
BEGIN
    SET IDENTITY_INSERT [dbo].[CUSTPO_MASTER] ON;
    INSERT INTO [dbo].[CUSTPO_MASTER]
        (CPOM_CODE, CPOM_P_CODE, CPOM_CM_COMP_ID, CPOM_PONO, CPOM_TYPE,
         CPOM_DATE, CPOM_CR_DAYS, CPOM_GRAND_TOT, CPOM_INV_FLAG,
         CPOM_AM_COUNT, MODIFY, ES_DELETE)
    VALUES
        (9001, 9001, 1, 'E2E-BASE-PO-9001', 1,
         GETDATE(), 30, 118000, 0, 0, 0, 0);
    SET IDENTITY_INSERT [dbo].[CUSTPO_MASTER] OFF;
END
GO

-- CUSTPO_DETAIL for PO 9001 — two line items
IF NOT EXISTS (SELECT 1 FROM [dbo].[CUSTPO_DETAIL] WHERE CPOD_CPOM_CODE = 9001)
BEGIN
    INSERT INTO [dbo].[CUSTPO_DETAIL]
        (CPOD_CPOM_CODE, CPOD_I_CODE, CPOD_UOM_CODE, CPOD_ORD_QTY, CPOD_RATE, CPOD_AMT,
         CPOD_STATUS, CPOD_DISPACH)
    VALUES
        (9001, 9001, 1, 500, 100, 50000, 0, 0),
        (9001, 9002, 1, 300, 200, 60000, 0, 0);
END
GO

-- STORE_MASTER — STORE_CODE = 1 (required for DeliveryChallan/IssueMaster stock operations)
IF NOT EXISTS (SELECT 1 FROM [dbo].[STORE_MASTER] WHERE STORE_CODE = 1)
BEGIN
    SET IDENTITY_INSERT [dbo].[STORE_MASTER] ON;
    INSERT INTO [dbo].[STORE_MASTER]
        (STORE_CODE, STORE_NAME, STORE_COMP_ID, ES_DELETE)
    VALUES (1, 'Main Store', 1, 0);
    SET IDENTITY_INSERT [dbo].[STORE_MASTER] OFF;
END
GO

-- STOCK_LEDGER — opening balance for test items
-- IMPORTANT: STOCK_LEDGER uses STL_* column names (NOT the legacy SL_* prefix).
-- Columns: STL_I_CODE, STL_DOC_QTY, STL_DOC_NO, STL_DOC_TYPE, STL_STORE_TYPE
-- Opening balance rows use STL_DOC_TYPE='OPEN' to distinguish from transactional entries.
-- The getStockBalance helper sums ALL STL_DOC_QTY rows for an item.
IF NOT EXISTS (SELECT 1 FROM [dbo].[STOCK_LEDGER]
               WHERE STL_I_CODE = 9001 AND STL_DOC_TYPE = 'OPEN')
BEGIN
    INSERT INTO [dbo].[STOCK_LEDGER]
        (STL_I_CODE, STL_DOC_TYPE, STL_DOC_QTY, STL_STORE_TYPE)
    VALUES (9001, 'OPEN', 1000, -2147483648);
END
IF NOT EXISTS (SELECT 1 FROM [dbo].[STOCK_LEDGER]
               WHERE STL_I_CODE = 9002 AND STL_DOC_TYPE = 'OPEN')
BEGIN
    INSERT INTO [dbo].[STOCK_LEDGER]
        (STL_I_CODE, STL_DOC_TYPE, STL_DOC_QTY, STL_STORE_TYPE)
    VALUES (9002, 'OPEN', 800, -2147483648);
END
GO

PRINT '=== Test fixtures deployed successfully ===';
GO
