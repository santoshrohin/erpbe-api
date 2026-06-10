CREATE OR ALTER PROCEDURE [dbo].[ERP_DeleteProductionToStore]
    @ProductionCode INT,
    @CompanyCode    INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE PRODUCTION_TO_STORE_MASTER
    SET    ES_DELETE = 1
    WHERE  PS_CODE         = @ProductionCode
      AND  PS_CM_COMP_CODE = @CompanyCode
      AND  ES_DELETE       = 0;

    -- Hard-delete details (table has no ES_DELETE column)
    DELETE FROM PRODUCTION_TO_STORE_DETAIL
    WHERE  PSD_PS_CODE = @ProductionCode;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
