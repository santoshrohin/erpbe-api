CREATE OR ALTER PROCEDURE [dbo].[SP_IsUnitNameUnique] --'TEST_UNIT_e7d55356',1,null
    @UnitName VARCHAR(100),
    @CompanyId INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT OFF;
    
    SELECT count(*)
    FROM ITEM_UNIT_MASTER
    WHERE I_UOM_NAME = @UnitName
    AND I_UOM_CM_COMP_ID = @CompanyId
    AND ES_DELETE = 0
    AND (@ExcludeId IS NULL OR I_UOM_CODE != @ExcludeId);
END