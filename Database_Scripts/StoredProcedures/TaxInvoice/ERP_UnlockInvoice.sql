CREATE OR ALTER PROCEDURE [dbo].[ERP_UnlockInvoice]
    @InvoiceCode BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE INVOICE_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  INM_CODE  = @InvoiceCode
      AND  INM_TYPE <> 'OutJWINM';   -- exclude LCI records

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
