CREATE PROCEDURE [dbo].[SP_UpdateUnitMaster]
    @UnitId INT,
    @UnitName VARCHAR(10),
    @UnitDescription VARCHAR(100) = NULL,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    -- Check if unit exists
    IF NOT EXISTS (SELECT 1 FROM ITEM_UNIT_MASTER WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0)
    BEGIN
        RAISERROR('Unit not found', 16, 1);
        RETURN;
    END
    
    -- Check if unit name already exists for another unit in the same company
    IF EXISTS (
        SELECT 1 FROM ITEM_UNIT_MASTER 
        WHERE I_UOM_NAME = @UnitName 
        AND I_UOM_CM_COMP_ID = (SELECT I_UOM_CM_COMP_ID FROM ITEM_UNIT_MASTER WHERE I_UOM_CODE = @UnitId)
        AND I_UOM_CODE != @UnitId 
        AND ES_DELETE = 0
    )
    BEGIN
        RAISERROR('Unit with this name already exists for this company', 16, 1);
        RETURN;
    END
    
    -- Update unit
    UPDATE ITEM_UNIT_MASTER
    SET
        I_UOM_NAME = @UnitName,
        I_UOM_DESC = @UnitDescription,
        MODIFY = 1
    WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0;
    
    SELECT @@ROWCOUNT;
END
