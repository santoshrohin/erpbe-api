-- Sets INM_IS_AUTHORIZED = 1 for the given invoice.
-- Gated in the API by PermissionBit.BackDate (bit 6).
CREATE OR ALTER PROCEDURE [dbo].[ERP_ApproveTaxInvoice]
    @InvoiceCode BIGINT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE INVOICE_MASTER
    SET    INM_IS_AUTHORIZED = 1
    WHERE  INM_CODE      = @InvoiceCode
      AND  INM_CM_CODE = @CompanyCode
      AND  ES_DELETE     = 0;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
