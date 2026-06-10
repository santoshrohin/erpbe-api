CREATE OR ALTER PROCEDURE [dbo].[SP_GetItemCategoryMasterById]
    @I_CAT_CODE INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        I_CAT_CODE AS CategoryId,
        I_CAT_NAME AS CategoryName,
        I_CAT_CM_COMP_ID AS CompanyId,
        I_CAT_SHORTCLOSE AS IsAutoShortClose,
        CASE WHEN ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive
    FROM ITEM_CATEGORY_MASTER
    WHERE I_CAT_CODE = @I_CAT_CODE;
END