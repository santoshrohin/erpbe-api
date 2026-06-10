CREATE OR ALTER PROCEDURE [dbo].[SP_IsItemCategoryNameUnique]
    @I_CAT_NAME NVARCHAR(50),
    @I_CAT_CM_COMP_ID INT,
    @ExcludeCategoryId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS Count
    FROM ITEM_CATEGORY_MASTER
    WHERE I_CAT_NAME = @I_CAT_NAME
      AND I_CAT_CM_COMP_ID = @I_CAT_CM_COMP_ID
      AND (@ExcludeCategoryId IS NULL OR I_CAT_CODE != @ExcludeCategoryId)
      AND ES_DELETE = 0;
END