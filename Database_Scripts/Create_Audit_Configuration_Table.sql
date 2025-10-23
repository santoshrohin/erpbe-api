-- =============================================
-- Create AUDIT_CONFIGURATION table
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating AUDIT_CONFIGURATION table...';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AUDIT_CONFIGURATION')
BEGIN
    CREATE TABLE [dbo].[AUDIT_CONFIGURATION] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Endpoint] NVARCHAR(200) NOT NULL,
        [HttpMethod] NVARCHAR(10) NOT NULL,
        [EntityName] NVARCHAR(100) NOT NULL,
        [EntityIdProperty] NVARCHAR(50) NULL,
        [IsEnabled] BIT NOT NULL DEFAULT 1,
        [TrackPropertyChanges] BIT NOT NULL DEFAULT 1,
        [TrackOldValues] BIT NOT NULL DEFAULT 1,
        [TrackNewValues] BIT NOT NULL DEFAULT 1,
        [Description] NVARCHAR(500) NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [UQ_Audit_Endpoint_Method] UNIQUE ([Endpoint], [HttpMethod])
    );
    
    PRINT 'AUDIT_CONFIGURATION table created successfully!';
END
ELSE
BEGIN
    PRINT 'AUDIT_CONFIGURATION table already exists.';
END
GO

