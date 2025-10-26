-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Update an existing item category
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_UpdateItemCategoryMaster...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateItemCategoryMaster]
    @I_CAT_CODE INT,
    @I_CAT_NAME NVARCHAR(50),
    @I_CAT_SHORTCLOSE BIT,
    @ES_DELETE BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ITEM_CATEGORY_MASTER
    SET 
        I_CAT_NAME = @I_CAT_NAME,
        I_CAT_SHORTCLOSE = @I_CAT_SHORTCLOSE,
        ES_DELETE = @ES_DELETE
    WHERE I_CAT_CODE = @I_CAT_CODE;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

PRINT 'SP_UpdateItemCategoryMaster created successfully!';
GO



