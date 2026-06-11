CREATE OR ALTER PROCEDURE [dbo].[ERP_DeleteDeliveryChallan]
    @ChallanCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DELIVERY_CHALLAN_MASTER
    SET    ES_DELETE = 1
    WHERE  DCM_CODE    = @ChallanCode
      AND  DCM_CM_CODE = @CompanyCode
      AND  ES_DELETE   = 0;

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
