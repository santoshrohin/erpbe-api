-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Check if item category is used in ITEM_MASTER
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_CheckItemCategoryUsage...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_CheckItemCategoryUsage]
    @I_CAT_CODE INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS Count
    FROM ITEM_MASTER
    WHERE I_CAT_CODE = @I_CAT_CODE
      AND ES_DELETE = 0;
END
GO

PRINT 'SP_CheckItemCategoryUsage created successfully!';
GO



