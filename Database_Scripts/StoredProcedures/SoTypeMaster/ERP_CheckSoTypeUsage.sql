-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Check if SO Type is used in Customer PO Master
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_CheckSoTypeUsage...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_CheckSoTypeUsage]
    @SO_T_CODE INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS UsageCount
    FROM CUSTPO_MASTER
    WHERE CPOM_TYPE = @SO_T_CODE
        AND ES_DELETE = 0;
END
GO

PRINT 'ERP_CheckSoTypeUsage created successfully!';
GO

