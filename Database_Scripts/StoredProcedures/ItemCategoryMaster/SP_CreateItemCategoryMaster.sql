-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Create a new item category
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_CreateItemCategoryMaster...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_CreateItemCategoryMaster]
    @I_CAT_NAME NVARCHAR(50),
    @I_CAT_CM_COMP_ID INT,
    @I_CAT_SHORTCLOSE BIT,
    @ES_DELETE BIT
AS
BEGIN
    SET NOCOUNT OFF;
    
    INSERT INTO ITEM_CATEGORY_MASTER (
        I_CAT_NAME,
        I_CAT_CM_COMP_ID,
        I_CAT_SHORTCLOSE,
        ES_DELETE,
        MODIFY
    )
    VALUES (
        @I_CAT_NAME,
        @I_CAT_CM_COMP_ID,
        @I_CAT_SHORTCLOSE,
        @ES_DELETE,
        0 -- MODIFY flag set to false
    );
    
    SELECT SCOPE_IDENTITY() AS CategoryId;
END
GO

PRINT 'SP_CreateItemCategoryMaster created successfully!';
GO



