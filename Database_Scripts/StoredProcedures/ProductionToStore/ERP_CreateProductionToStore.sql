CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateProductionToStore]
    @CompanyCode  INT,
    @GinDate      SMALLDATETIME = NULL,
    @Type         VARCHAR(10)   = NULL,
    @PersonName   VARCHAR(260)  = NULL,
    @MrCode       DECIMAL(18,0) = NULL,
    @CustomerCode INT           = NULL,
    @BatchNo      INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextGinNo DECIMAL(18,0);
    SELECT @NextGinNo = ISNULL(MAX(PS_GIN_NO), 0) + 1
    FROM PRODUCTION_TO_STORE_MASTER
    WHERE PS_CM_COMP_CODE = @CompanyCode;

    INSERT INTO PRODUCTION_TO_STORE_MASTER
        (PS_GIN_NO, PS_GIN_DATE, PS_TYPE, PS_PERSON_NAME,
         PS_MR_CODE, PS_P_CODE, PS_BATCH_NO, PS_CM_COMP_CODE,
         MODIFY, ES_DELETE)
    VALUES
        (@NextGinNo, @GinDate, @Type, @PersonName,
         @MrCode, @CustomerCode, @BatchNo, @CompanyCode,
         0, 0);

    SELECT SCOPE_IDENTITY() AS ProductionCode, @NextGinNo AS GinNumber;
END
GO
