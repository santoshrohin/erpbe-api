CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateProductionToStoreDetail]
    @ProductionCode INT,
    @ItemCode       INT           = NULL,
    @Quantity       DECIMAL(18,3) = 0,
    @Remark         VARCHAR(260)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PRODUCTION_TO_STORE_DETAIL
        (PSD_PS_CODE, PSD_I_CODE, PSD_QTY, PSD_REMARK)
    VALUES
        (@ProductionCode, @ItemCode, @Quantity, @Remark);
END
GO
