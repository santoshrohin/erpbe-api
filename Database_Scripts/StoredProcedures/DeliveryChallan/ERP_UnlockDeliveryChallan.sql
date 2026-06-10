CREATE OR ALTER PROCEDURE [dbo].[ERP_UnlockDeliveryChallan]
    @ChallanCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DELIVERY_CHALLAN_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  DCM_CODE    = @ChallanCode
      AND  DCM_CM_CODE = @CompanyCode;
END
GO
