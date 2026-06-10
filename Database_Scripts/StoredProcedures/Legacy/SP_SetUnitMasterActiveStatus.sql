CREATE OR ALTER PROCEDURE [dbo].[SP_SetUnitMasterActiveStatus]
    @UnitId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ITEM_UNIT_MASTER
    SET
        ES_DELETE = CASE WHEN @IsActive = 1 THEN 0 ELSE 1 END,
        MODIFY = 1
    WHERE I_UOM_CODE = @UnitId;
    
    SELECT @@ROWCOUNT;
END