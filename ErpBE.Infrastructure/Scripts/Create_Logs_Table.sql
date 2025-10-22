-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-19
-- Description: Create Logs table for Serilog SQL Server sink
-- Database: db_a2ea4b_farmerplacement
-- =============================================

USE [db_a2ea4b_farmerplacement]
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

-- Test insert to verify table works
INSERT INTO [dbo].[Logs] ([Message], [MessageTemplate], [Level], [TimeStamp], [Exception], [Properties], [UserId], [RequestId], [ActionName])
VALUES ('Test log entry', 'Test log entry', 'Information', GETDATE(), NULL, '{}', 'SYSTEM', 'TEST-001', 'TableCreation')

PRINT 'Test log entry inserted successfully'
GO

-- Verify the table and data
SELECT COUNT(*) as LogCount FROM [dbo].[Logs]
SELECT TOP 5 * FROM [dbo].[Logs] ORDER BY [TimeStamp] DESC
GO
