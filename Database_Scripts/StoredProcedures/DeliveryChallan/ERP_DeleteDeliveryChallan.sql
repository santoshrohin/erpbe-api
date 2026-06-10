CREATE OR ALTER PROCEDURE [dbo].[ERP_DeleteDeliveryChallan]
    @ChallanCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Abort if locked (MODIFY=1 means the record is locked)
    IF EXISTS (SELECT 1 FROM DELIVERY_CHALLAN_MASTER WHERE DCM_CODE = @ChallanCode AND MODIFY = 1 AND ES_DELETE = 0)
    BEGIN
        RAISERROR('Delivery Challan is locked and cannot be deleted.', 16, 1);
        RETURN;
    END

    UPDATE DELIVERY_CHALLAN_MASTER
    SET    ES_DELETE = 1
    WHERE  DCM_CODE    = @ChallanCode
      AND  DCM_CM_CODE = @CompanyCode
      AND  ES_DELETE   = 0
      AND  MODIFY      = 0;

    UPDATE DELIVERY_CHALLAN_DETAIL
    SET    ES_DELETE = 1
    WHERE  DCD_DCM_CODE = @ChallanCode;

    -- Restore stock: remove all DCOUT STOCK_LEDGER rows for this challan
    DELETE FROM STOCK_LEDGER
    WHERE  STL_DOC_NO   = @ChallanCode
      AND  STL_DOC_TYPE = 'DCOUT';

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
