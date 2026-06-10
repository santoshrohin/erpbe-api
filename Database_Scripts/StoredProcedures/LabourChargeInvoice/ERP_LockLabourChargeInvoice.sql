CREATE OR ALTER PROCEDURE [dbo].[ERP_LockLabourChargeInvoice]
    @InvoiceCode    INT,
    @CompanyCode    INT,
    @LockedByUserId INT = 0,
    @TimeoutMinutes INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE INVOICE_MASTER
    SET    MODIFY      = 1,
           MODIFY_TIME = GETDATE(),
           MODIFY_BY   = @LockedByUserId
    WHERE  INM_CODE    = @InvoiceCode
      AND  INM_CM_CODE = @CompanyCode
      AND  INM_TYPE    = 'OutJWINM'
      AND  ES_DELETE   = 0
      AND  (
               MODIFY = 0
               OR MODIFY_TIME IS NULL
               OR MODIFY_TIME < DATEADD(MINUTE, -@TimeoutMinutes, GETDATE())
           );

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
