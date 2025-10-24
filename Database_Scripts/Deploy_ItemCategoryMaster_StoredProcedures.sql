-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Deploy all Item Category Master stored procedures
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT '========================================';
PRINT 'Starting deployment of Item Category Master stored procedures...';
PRINT '========================================';
GO

-- Execute all stored procedures in order
:r .\StoredProcedures\ItemCategoryMaster\SP_CreateItemCategoryMaster.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_UpdateItemCategoryMaster.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_DeleteItemCategoryMaster.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_GetItemCategoryMasterById.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_GetItemCategoryMasters.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_GetItemCategoryMasterByName.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_IsItemCategoryNameUnique.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_SetItemCategoryActiveStatus.sql
:r .\StoredProcedures\ItemCategoryMaster\SP_CheckItemCategoryUsage.sql

PRINT '========================================';
PRINT 'All Item Category Master stored procedures deployed successfully!';
PRINT '========================================';
GO



