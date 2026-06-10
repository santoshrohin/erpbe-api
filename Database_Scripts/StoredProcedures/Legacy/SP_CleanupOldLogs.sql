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