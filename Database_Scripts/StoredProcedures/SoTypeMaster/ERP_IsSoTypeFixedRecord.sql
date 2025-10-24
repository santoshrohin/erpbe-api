-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Check if SO Type is a fixed record (cannot be deleted)
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_IsSoTypeFixedRecord...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_IsSoTypeFixedRecord]
    @SO_T_CODE INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Fixed records are -2147483648 and -2147483647 (legacy business rule)
    IF @SO_T_CODE IN (-2147483648, -2147483647)
    BEGIN
        SELECT CAST(1 AS BIT) AS IsFixed;
    END
    ELSE
    BEGIN
        SELECT CAST(0 AS BIT) AS IsFixed;
    END
END
GO

PRINT 'ERP_IsSoTypeFixedRecord created successfully!';
GO

