-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-01-24
-- Description: Soft deletes a Tax Invoice (sets ES_DELETE = 1) and reverses stock
-- =============================================
ALTER PROCEDURE [dbo].[ERP_DeleteTaxInvoice]
    @InvoiceCode BIGINT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Reverse stock (delete all stock ledger entries for this invoice)
        DELETE FROM STOCK_LEDGER
        WHERE STL_DOC_NO = @InvoiceCode
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

        -- Soft delete invoice master
        -- SET NOCOUNT OFF before the final UPDATE so Dapper's ExecuteAsync returns the row count
        SET NOCOUNT OFF;
        UPDATE INVOICE_MASTER
        SET ES_DELETE = 1
        WHERE INM_CODE = @InvoiceCode AND INM_CM_CODE = @CompanyCode AND ES_DELETE = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

