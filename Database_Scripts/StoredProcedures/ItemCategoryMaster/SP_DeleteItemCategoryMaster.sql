-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Soft delete an item category
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_DeleteItemCategoryMaster...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_DeleteItemCategoryMaster]
    @I_CAT_CODE INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ITEM_CATEGORY_MASTER
    SET ES_DELETE = 1
    WHERE I_CAT_CODE = @I_CAT_CODE;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

PRINT 'SP_DeleteItemCategoryMaster created successfully!';
GO



