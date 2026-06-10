CREATE OR ALTER PROCEDURE [dbo].[SP_CreateUnitMaster]
    @UnitName VARCHAR(100),
    @UnitDescription VARCHAR(100) = NULL,
    @CompanyId INT,
    @IsActive BIT = 1,
    @UnitId INT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;
    
    -- Check if unit name already exists for this company
    IF EXISTS (SELECT 1 FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = @UnitName AND I_UOM_CM_COMP_ID = @CompanyId AND ES_DELETE = 0)
    BEGIN
        RAISERROR('Unit with this name already exists for this company', 16, 1);
        RETURN;
    END
    
    -- Insert new unit
    INSERT INTO ITEM_UNIT_MASTER (
        I_UOM_CM_COMP_ID,
        I_UOM_NAME,
        I_UOM_DESC,
        ES_DELETE,
        MODIFY
    )
    VALUES (
        @CompanyId,
        @UnitName,
        @UnitDescription,
        0, -- ES_DELETE
        0  -- MODIFY
    );
    
    SET @UnitId = SCOPE_IDENTITY();
    SELECT @UnitId AS UnitId;
END