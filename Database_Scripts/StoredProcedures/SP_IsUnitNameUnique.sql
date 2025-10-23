-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Check if unit name is unique
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_IsUnitNameUnique...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_IsUnitNameUnique]
    @UnitName NVARCHAR(100),
    @CompanyId INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) as Count
    FROM ITEM_UNIT_MASTER
    WHERE I_UOM_NAME = @UnitName
      AND I_UOM_CM_ID = @CompanyId
      AND (@ExcludeId IS NULL OR I_UOM_CODE != @ExcludeId);
END
GO

PRINT 'SP_IsUnitNameUnique created successfully!';
GO

