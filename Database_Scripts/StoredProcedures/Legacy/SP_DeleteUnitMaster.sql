CREATE OR ALTER PROCEDURE [dbo].[SP_DeleteUnitMaster]
    @UnitId INT
AS
BEGIN
    SET NOCOUNT off;
    
    -- Soft delete unit
    UPDATE ITEM_UNIT_MASTER
    SET
        ES_DELETE = 1,
        MODIFY = 1
    WHERE I_UOM_CODE = @UnitId;
    
    SELECT @@ROWCOUNT;
END