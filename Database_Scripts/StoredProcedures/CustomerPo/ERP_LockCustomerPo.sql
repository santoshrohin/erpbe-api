CREATE OR ALTER PROCEDURE [dbo].[ERP_LockCustomerPo]
    @PoCode         INT,
    @LockedByUserId INT = 0,
    @TimeoutMinutes INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    -- Atomic check-and-set: acquire only if unlocked OR if the existing lock has expired.
    -- Treats NULL MODIFY_TIME (rows locked before this migration) as expired.
    UPDATE CUSTPO_MASTER
    SET    MODIFY      = 1,
           MODIFY_TIME = GETDATE(),
           MODIFY_BY   = @LockedByUserId
    WHERE  CPOM_CODE = @PoCode
      AND  ES_DELETE  = 0
      AND  (
               MODIFY = 0
               OR MODIFY_TIME IS NULL
               OR MODIFY_TIME < DATEADD(MINUTE, -@TimeoutMinutes, GETDATE())
           );

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
