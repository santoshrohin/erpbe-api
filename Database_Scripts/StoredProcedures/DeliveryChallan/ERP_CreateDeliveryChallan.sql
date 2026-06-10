CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateDeliveryChallan]
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

    DECLARE @NextNo DECIMAL(18,0);
    SELECT @NextNo = ISNULL(MAX(DCM_NO), 0) + 1
    FROM DELIVERY_CHALLAN_MASTER
    WHERE DCM_CM_CODE = @CompanyCode;

    INSERT INTO DELIVERY_CHALLAN_MASTER
        (DCM_CM_CODE, DCM_P_CODE, DCM_TYPE, DCM_NO, DCM_DATE,
         DCM_INV_NO, DCM_THROUGH, DCM_VEH_NO, DCM_LR_NO,
         DCM_ORDER_NO, DCM_ORDER_DATE, ES_DELETE, MODIFY,
         DCM_MAT_TYPE, DCM_IS_RETURNABLE)
    VALUES
        (@CompanyCode, @CustomerCode, @Type, @NextNo, @ChallanDate,
         @InvoiceNumber, @Through, @VehicleNumber, @LrNumber,
         @OrderNumber, @OrderDate, 0, 0,
         @MaterialType, @IsReturnable);

    SELECT SCOPE_IDENTITY() AS ChallanCode, @NextNo AS ChallanNumber;
END
GO
