-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-19
-- Description: Create Logs table for Serilog SQL Server sink
-- =============================================

USE [YOUR_DATABASE_NAME] -- Replace with your actual database name
GO

-- Create Logs table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Logs' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Logs](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Message] [nvarchar](max) NULL,
        [MessageTemplate] [nvarchar](max) NULL,
        [Level] [nvarchar](128) NULL,
        [TimeStamp] [datetime] NOT NULL,
        [Exception] [nvarchar](max) NULL,
        [Properties] [nvarchar](max) NULL,
        [UserId] [nvarchar](50) NULL,
        [RequestId] [nvarchar](50) NULL,
        [ActionName] [nvarchar](100) NULL,
        CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    -- Create index on TimeStamp for better query performance
    CREATE NONCLUSTERED INDEX [IX_Logs_TimeStamp] ON [dbo].[Logs] ([TimeStamp] DESC)
    
    -- Create index on Level for filtering
    CREATE NONCLUSTERED INDEX [IX_Logs_Level] ON [dbo].[Logs] ([Level])
    
    -- Create index on UserId for user-specific queries
    CREATE NONCLUSTERED INDEX [IX_Logs_UserId] ON [dbo].[Logs] ([UserId])
    
    -- Create index on RequestId for request tracking
    CREATE NONCLUSTERED INDEX [IX_Logs_RequestId] ON [dbo].[Logs] ([RequestId])
    
    PRINT 'Logs table created successfully with indexes'
END
ELSE
BEGIN
    PRINT 'Logs table already exists'
END
GO

-- Create stored procedure for log cleanup
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CleanupOldLogs')
    DROP PROCEDURE [dbo].[SP_CleanupOldLogs]
GO

CREATE PROCEDURE [dbo].[SP_CleanupOldLogs]
    @DaysToKeep INT = 30,
    @DeletedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DELETE FROM [dbo].[Logs] 
        WHERE [TimeStamp] < DATEADD(day, -@DaysToKeep, GETDATE())
        
        SET @DeletedCount = @@ROWCOUNT
        
        SELECT @DeletedCount as DeletedCount
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- Create stored procedure for log statistics
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetLogStatistics')
    DROP PROCEDURE [dbo].[SP_GetLogStatistics]
GO

CREATE PROCEDURE [dbo].[SP_GetLogStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Level statistics for last 7 days
    SELECT 
        [Level],
        COUNT(*) as [Count]
    FROM [dbo].[Logs] 
    WHERE [TimeStamp] >= DATEADD(day, -7, GETDATE())
    GROUP BY [Level]
    ORDER BY [Count] DESC
    
    -- Daily statistics for last 30 days
    SELECT 
        CAST([TimeStamp] as DATE) as [Date],
        COUNT(*) as [TotalLogs],
        SUM(CASE WHEN [Level] = 'Error' THEN 1 ELSE 0 END) as [Errors],
        SUM(CASE WHEN [Level] = 'Warning' THEN 1 ELSE 0 END) as [Warnings],
        SUM(CASE WHEN [Level] = 'Information' THEN 1 ELSE 0 END) as [Information]
    FROM [dbo].[Logs] 
    WHERE [TimeStamp] >= DATEADD(day, -30, GETDATE())
    GROUP BY CAST([TimeStamp] as DATE)
    ORDER BY [Date] DESC
END
GO

PRINT 'Logs table setup completed successfully'
PRINT 'Created tables: Logs'
PRINT 'Created procedures: SP_CleanupOldLogs, SP_GetLogStatistics'
PRINT 'Created indexes: IX_Logs_TimeStamp, IX_Logs_Level, IX_Logs_UserId, IX_Logs_RequestId'
