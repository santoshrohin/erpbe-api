-- =============================================================================
-- Migration: Add INM_PARENT_CODE to INVOICE_MASTER for supplementary invoices
-- Parity: Legacy INVOICE_MASTER had INM_SUPPLEMENTORY BIT but no parent ref.
--         New system adds explicit parent reference for traceability.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('INVOICE_MASTER')
      AND name = 'INM_PARENT_CODE'
)
BEGIN
    ALTER TABLE dbo.INVOICE_MASTER
        ADD INM_PARENT_CODE INT NULL;

    PRINT 'Added INM_PARENT_CODE INT NULL to INVOICE_MASTER';
END
ELSE
BEGIN
    PRINT 'INM_PARENT_CODE already exists — skipping';
END
GO
