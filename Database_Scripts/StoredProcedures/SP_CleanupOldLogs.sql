-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Cleanup old logs based on retention days
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating SP_CleanupOldLogs...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_CleanupOldLogs]
    @DaysToKeep INT = 30,
    @DeletedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CutoffDate DATETIME = DATEADD(DAY, -@DaysToKeep, GETUTCDATE());
    
    DELETE FROM Logs
    WHERE TimeStamp < @CutoffDate;
    
    SET @DeletedCount = @@ROWCOUNT;
END
GO

PRINT 'SP_CleanupOldLogs created successfully!';
GO

