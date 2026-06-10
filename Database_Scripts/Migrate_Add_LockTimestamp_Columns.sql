-- ============================================================
-- Migration: Add lock timestamp and user columns
-- Applies to: CUSTPO_MASTER, DELIVERY_CHALLAN_MASTER, INVOICE_MASTER
--
-- MODIFY_TIME  — when the lock was acquired / last renewed
-- MODIFY_BY    — user_code of the user holding the lock
--
-- NULL on existing rows is intentional: the cleanup SP treats
-- NULL MODIFY_TIME as expired and will clear any stale MODIFY=1 rows.
--
-- To add a new module: ALTER its master table the same way below
-- and add it to ERP_CleanStaleLocks / ERP_GetActiveLocks / ERP_ForceUnlock.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CUSTPO_MASTER') AND name = 'MODIFY_TIME'
)
    ALTER TABLE CUSTPO_MASTER ADD MODIFY_TIME DATETIME NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CUSTPO_MASTER') AND name = 'MODIFY_BY'
)
    ALTER TABLE CUSTPO_MASTER ADD MODIFY_BY INT NULL;

GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('DELIVERY_CHALLAN_MASTER') AND name = 'MODIFY_TIME'
)
    ALTER TABLE DELIVERY_CHALLAN_MASTER ADD MODIFY_TIME DATETIME NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('DELIVERY_CHALLAN_MASTER') AND name = 'MODIFY_BY'
)
    ALTER TABLE DELIVERY_CHALLAN_MASTER ADD MODIFY_BY INT NULL;

GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('INVOICE_MASTER') AND name = 'MODIFY_TIME'
)
    ALTER TABLE INVOICE_MASTER ADD MODIFY_TIME DATETIME NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('INVOICE_MASTER') AND name = 'MODIFY_BY'
)
    ALTER TABLE INVOICE_MASTER ADD MODIFY_BY INT NULL;

GO

PRINT 'Migration complete: MODIFY_TIME and MODIFY_BY columns added.';
