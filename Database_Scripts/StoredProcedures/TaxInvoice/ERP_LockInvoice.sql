-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-10-24
-- Description: Locks an invoice for editing (sets MODIFY = 1)
-- =============================================
CREATE PROCEDURE [dbo].[ERP_LockInvoice]
    @InvoiceCode INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        UPDATE INVOICE_MASTER 
        SET MODIFY = 1 
        WHERE INM_CODE = @InvoiceCode;

        -- Return the number of rows affected
        SELECT @@ROWCOUNT AS RowsAffected;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

