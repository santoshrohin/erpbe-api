-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Get item category by name and company ID
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_GetItemCategoryMasterByName...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetItemCategoryMasterByName]
    @I_CAT_NAME NVARCHAR(50),
    @I_CAT_CM_COMP_ID INT
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
    WHERE I_CAT_NAME = @I_CAT_NAME
      AND I_CAT_CM_COMP_ID = @I_CAT_CM_COMP_ID;
END
GO

PRINT 'SP_GetItemCategoryMasterByName created successfully!';
GO



