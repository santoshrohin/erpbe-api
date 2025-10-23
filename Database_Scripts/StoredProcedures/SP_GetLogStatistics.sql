-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Get log statistics grouped by level
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_GetLogStatistics...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetLogStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ISNULL(Level, 'Information') AS Level,
        COUNT(*) AS Count,
        MIN(TimeStamp) AS OldestLog,
        MAX(TimeStamp) AS NewestLog
    FROM Logs
    GROUP BY Level;
END
GO

PRINT 'SP_GetLogStatistics created successfully!';
GO

