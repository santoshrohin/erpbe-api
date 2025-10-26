-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-10-24
-- Description: Deletes invoice details for a given invoice code
-- =============================================
CREATE PROCEDURE [dbo].[ERP_DeleteInvoiceDetails]
    @InvoiceCode INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DELETE FROM INVOICE_DETAIL 
        WHERE IND_INM_CODE = @InvoiceCode;

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

