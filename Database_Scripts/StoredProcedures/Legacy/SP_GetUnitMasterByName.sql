CREATE OR ALTER PROCEDURE [dbo].[SP_GetUnitMasterByName]
    @UnitName VARCHAR(10),
    @CompanyId INT
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
        -- Audit fields from audit trail (latest record)
        ISNULL(AUDIT.CREATED_DATE, GETDATE()) AS CreatedDate,
        AUDIT.MODIFIED_DATE AS ModifiedDate,
        ISNULL(AUDIT.CREATED_BY, 'System') AS CreatedBy,
        AUDIT.MODIFIED_BY AS ModifiedBy
    FROM ITEM_UNIT_MASTER UOM
    LEFT JOIN (
        SELECT 
            RECORD_ID,
            CREATED_DATE,
            MODIFIED_DATE,
            CREATED_BY,
            MODIFIED_BY,
            ROW_NUMBER() OVER (PARTITION BY RECORD_ID ORDER BY CREATED_DATE DESC) as rn
        FROM AUDIT_TRAIL 
        WHERE TABLE_NAME = 'ITEM_UNIT_MASTER'
    ) AUDIT ON UOM.I_UOM_CODE = AUDIT.RECORD_ID AND AUDIT.rn = 1
    WHERE UOM.I_UOM_NAME = @UnitName 
    AND UOM.I_UOM_CM_COMP_ID = @CompanyId 
    AND UOM.ES_DELETE = 0;
END