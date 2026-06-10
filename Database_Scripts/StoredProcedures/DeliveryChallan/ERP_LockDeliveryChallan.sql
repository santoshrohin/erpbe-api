CREATE OR ALTER PROCEDURE [dbo].[ERP_LockDeliveryChallan]
    @ChallanCode    INT,
    @CompanyCode    INT,
    @LockedByUserId INT = 0,
    @TimeoutMinutes INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DELIVERY_CHALLAN_MASTER
    SET    MODIFY      = 1,
           MODIFY_TIME = GETDATE(),
           MODIFY_BY   = @LockedByUserId
    WHERE  DCM_CODE    = @ChallanCode
      AND  DCM_CM_CODE = @CompanyCode
      AND  ES_DELETE   = 0
      AND  (
               MODIFY = 0
               OR MODIFY_TIME IS NULL
               OR MODIFY_TIME < DATEADD(MINUTE, -@TimeoutMinutes, GETDATE())
           );

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
