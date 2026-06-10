CREATE OR ALTER PROCEDURE [dbo].[ERP_UpdateDeliveryChallan]
    @ChallanCode   INT,
    @CompanyCode   INT,
    @CustomerCode  INT           = NULL,
    @Type          VARCHAR(50)   = NULL,
    @ChallanDate   SMALLDATETIME = NULL,
    @InvoiceNumber VARCHAR(260)  = NULL,
    @Through       VARCHAR(260)  = NULL,
    @VehicleNumber VARCHAR(50)   = NULL,
    @LrNumber      VARCHAR(50)   = NULL,
    @OrderNumber   VARCHAR(50)   = NULL,
    @OrderDate     DATETIME      = NULL,
    @MaterialType  BIT           = 0,
    @IsReturnable  BIT           = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DELIVERY_CHALLAN_MASTER
    SET    DCM_P_CODE        = @CustomerCode,
           DCM_TYPE          = @Type,
           DCM_DATE          = @ChallanDate,
           DCM_INV_NO        = @InvoiceNumber,
           DCM_THROUGH       = @Through,
           DCM_VEH_NO        = @VehicleNumber,
           DCM_LR_NO         = @LrNumber,
           DCM_ORDER_NO      = @OrderNumber,
           DCM_ORDER_DATE    = @OrderDate,
           DCM_MAT_TYPE      = @MaterialType,
           DCM_IS_RETURNABLE = @IsReturnable
    WHERE  DCM_CODE    = @ChallanCode
      AND  DCM_CM_CODE = @CompanyCode
      AND  ES_DELETE   = 0;

    -- Restore stock: remove old STOCK_LEDGER DCOUT rows before caller re-inserts new details
    DELETE FROM STOCK_LEDGER
    WHERE  STL_DOC_NO   = @ChallanCode
      AND  STL_DOC_TYPE = 'DCOUT';

    -- Soft-delete old details; caller re-inserts via ERP_CreateDeliveryChallanDetail
    UPDATE DELIVERY_CHALLAN_DETAIL
    SET    ES_DELETE = 1
    WHERE  DCD_DCM_CODE = @ChallanCode;
END
GO
