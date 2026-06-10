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