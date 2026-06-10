CREATE OR ALTER PROCEDURE [dbo].[ERP_UpdateProductionToStore]
    @ProductionCode INT,
    @CompanyCode    INT,
    @GinDate        SMALLDATETIME = NULL,
    @Type           VARCHAR(10)   = NULL,
    @PersonName     VARCHAR(260)  = NULL,
    @MrCode         DECIMAL(18,0) = NULL,
    @CustomerCode   INT           = NULL,
    @BatchNo        INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE PRODUCTION_TO_STORE_MASTER
    SET    PS_GIN_DATE    = @GinDate,
           PS_TYPE        = @Type,
           PS_PERSON_NAME = @PersonName,
           PS_MR_CODE     = @MrCode,
           PS_P_CODE      = @CustomerCode,
           PS_BATCH_NO    = @BatchNo
    WHERE  PS_CODE         = @ProductionCode
      AND  PS_CM_COMP_CODE = @CompanyCode
      AND  ES_DELETE       = 0;

    -- Hard-delete old details (table has no ES_DELETE); caller re-inserts via ERP_CreateProductionToStoreDetail
    DELETE FROM PRODUCTION_TO_STORE_DETAIL
    WHERE  PSD_PS_CODE = @ProductionCode;
END
GO
