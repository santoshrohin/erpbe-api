-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-01-24
-- Description: Soft deletes a Tax Invoice (sets ES_DELETE = 1) and reverses stock
-- =============================================
ALTER PROCEDURE [dbo].[ERP_DeleteTaxInvoice]
    @InvoiceCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        -- Reverse stock (delete all stock ledger entries for this invoice)
        DELETE FROM STOCK_LEDGER
        WHERE STL_DOC_NO = @InvoiceCode 
        AND STL_DOC_TYPE = 'TAXINV';
        
        -- Soft delete invoice details
        UPDATE INVOICE_DETAIL
        SET ES_DELETE = 1
        WHERE IND_INM_CODE = @InvoiceCode;

        -- Soft delete invoice master
        UPDATE INVOICE_MASTER
        SET ES_DELETE = 1
        WHERE INM_CODE = @InvoiceCode AND INM_CM_CODE = @CompanyCode;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

