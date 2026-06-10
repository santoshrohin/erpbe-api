CREATE OR ALTER PROCEDURE [dbo].[ERP_LockInvoice]
    @InvoiceCode    BIGINT,
    @LockedByUserId INT = 0,
    @TimeoutMinutes INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE INVOICE_MASTER
    SET    MODIFY      = 1,
           MODIFY_TIME = GETDATE(),
           MODIFY_BY   = @LockedByUserId
    WHERE  INM_CODE   = @InvoiceCode
      AND  ES_DELETE  = 0
      AND  INM_TYPE  <> 'OutJWINM'   -- exclude LCI records
      AND  (
               MODIFY = 0
               OR MODIFY_TIME IS NULL
               OR MODIFY_TIME < DATEADD(MINUTE, -@TimeoutMinutes, GETDATE())
           );

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
