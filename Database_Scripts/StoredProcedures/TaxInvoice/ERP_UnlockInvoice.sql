-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-10-24
-- Description: Unlocks an invoice after editing (sets MODIFY = 0)
-- =============================================
CREATE PROCEDURE [dbo].[ERP_UnlockInvoice]
    @InvoiceCode INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE INVOICE_MASTER 
        SET MODIFY = 0 
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

