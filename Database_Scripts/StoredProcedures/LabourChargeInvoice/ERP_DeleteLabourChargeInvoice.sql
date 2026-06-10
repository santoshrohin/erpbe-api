-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-11
-- Description: Soft deletes a Labour Charge Invoice (INM_TYPE='OutJWINM'),
--              restores stock in STOCK_LEDGER, and reverses CUSTPO_DETAIL dispatched qty.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_DeleteLabourChargeInvoice]
    @InvoiceCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Restore stock: remove TAXINV entries in STOCK_LEDGER for this invoice
        DELETE FROM STOCK_LEDGER
        WHERE STL_DOC_NO   = @InvoiceCode
          AND STL_DOC_TYPE = 'TAXINV';

        -- Reverse dispatched qty in CUSTPO_DETAIL for all PO-linked line items
        UPDATE CD
        SET CPOD_DISPACH = ISNULL(CPOD_DISPACH, 0) - ID.IND_INQTY
        FROM CUSTPO_DETAIL CD
        INNER JOIN INVOICE_DETAIL ID ON CD.CPOD_CPOM_CODE = ID.IND_CPOM_CODE
                                    AND CD.CPOD_I_CODE    = ID.IND_I_CODE
        WHERE ID.IND_INM_CODE  = @InvoiceCode
          AND ID.IND_CPOM_CODE IS NOT NULL
          AND ISNULL(ID.ES_DELETE, 0) = 0;

        -- Soft delete invoice details
        UPDATE INVOICE_DETAIL
        SET ES_DELETE = 1
        WHERE IND_INM_CODE = @InvoiceCode;

        -- Soft delete invoice master (only if not already deleted) —
        -- SET NOCOUNT OFF so Dapper's ExecuteAsync returns this row count (1 = deleted, 0 = already gone)
        SET NOCOUNT OFF;
        UPDATE INVOICE_MASTER
        SET    ES_DELETE = 1
        WHERE  INM_CODE    = @InvoiceCode
          AND  INM_CM_CODE = @CompanyCode
          AND  INM_TYPE    = 'OutJWINM'
          AND  ES_DELETE   = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
