-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Get unit master by name
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_GetUnitMasterByName...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetUnitMasterByName]
    @UnitName NVARCHAR(100),
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        I_UOM_CODE as UnitId,
        I_UOM_NAME as UnitName,
        I_UOM_DESCRIPTION as UnitDescription,
        I_UOM_CM_ID as CompanyId,
        ES_DELETE as IsActive
    FROM ITEM_UNIT_MASTER
    WHERE I_UOM_NAME = @UnitName
      AND I_UOM_CM_ID = @CompanyId;
END
GO

PRINT 'SP_GetUnitMasterByName created successfully!';
GO

