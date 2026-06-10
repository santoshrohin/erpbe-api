CREATE OR ALTER PROCEDURE [dbo].[ERP_UnlockLabourChargeInvoice]
    @InvoiceCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE INVOICE_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  INM_CODE    = @InvoiceCode
      AND  INM_CM_CODE = @CompanyCode
      AND  INM_TYPE    = 'OutJWINM';
END
GO
