CREATE OR ALTER PROCEDURE [dbo].[SP_GetUnitMasterById]
    @UnitId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UOM.I_UOM_CODE AS Id,
        UOM.I_UOM_CM_COMP_ID AS CompanyId,
        UOM.I_UOM_NAME AS UnitName,
        UOM.I_UOM_DESC AS UnitDescription,
        CASE WHEN UOM.ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
        UOM.ES_DELETE AS IsDeleted,
        UOM.MODIFY AS IsModified,
        ISNULL(AT.CREATED_DATE, GETDATE()) AS CreatedDate,
        AT.MODIFIED_DATE AS ModifiedDate,
        ISNULL(AT.CREATED_BY, 'System') AS CreatedBy,
        AT.MODIFIED_BY AS ModifiedBy
    FROM ITEM_UNIT_MASTER UOM
    LEFT JOIN AUDIT_TRAIL AT ON AT.TABLE_NAME = 'ITEM_UNIT_MASTER' AND AT.RECORD_ID = UOM.I_UOM_CODE
    WHERE UOM.I_UOM_CODE = @UnitId;
END