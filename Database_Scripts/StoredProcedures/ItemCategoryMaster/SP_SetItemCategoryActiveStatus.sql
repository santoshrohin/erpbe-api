-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Set item category active status
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_SetItemCategoryActiveStatus...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_SetItemCategoryActiveStatus]
    @I_CAT_CODE INT,
    @ES_DELETE BIT
AS
BEGIN
    SET NOCOUNT OFF;
    
    UPDATE ITEM_CATEGORY_MASTER
    SET ES_DELETE = @ES_DELETE
    WHERE I_CAT_CODE = @I_CAT_CODE;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

PRINT 'SP_SetItemCategoryActiveStatus created successfully!';
GO



