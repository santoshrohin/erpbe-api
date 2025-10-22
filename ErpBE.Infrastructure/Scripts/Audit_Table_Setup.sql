-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-21
-- Description: Audit Trail Table Setup
-- =============================================

USE [db_a2ea4b_sunv2]
GO

-- Create Audit Trail Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND name = 'AUDIT_TRAIL')
BEGIN
    CREATE TABLE [dbo].[AUDIT_TRAIL] (
        [AUDIT_ID] INT IDENTITY(1,1) PRIMARY KEY,
        [TABLE_NAME] NVARCHAR(100) NOT NULL,
        [RECORD_ID] INT NOT NULL,
        [ACTION_TYPE] NVARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
        [OLD_VALUES] NVARCHAR(MAX) NULL,     -- JSON of old values
        [NEW_VALUES] NVARCHAR(MAX) NULL,     -- JSON of new values
        [CREATED_DATE] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CREATED_BY] NVARCHAR(100) NOT NULL,
        [MODIFIED_DATE] DATETIME2 NULL,
        [MODIFIED_BY] NVARCHAR(100) NULL,
        [IP_ADDRESS] NVARCHAR(50) NULL,
        [USER_AGENT] NVARCHAR(500) NULL,
        [SESSION_ID] NVARCHAR(100) NULL
    );
    
    -- Create indexes for better performance
    CREATE INDEX IX_AUDIT_TRAIL_TABLE_RECORD ON [dbo].[AUDIT_TRAIL] ([TABLE_NAME], [RECORD_ID]);
    CREATE INDEX IX_AUDIT_TRAIL_CREATED_DATE ON [dbo].[AUDIT_TRAIL] ([CREATED_DATE]);
    CREATE INDEX IX_AUDIT_TRAIL_ACTION_TYPE ON [dbo].[AUDIT_TRAIL] ([ACTION_TYPE]);
    
    PRINT 'AUDIT_TRAIL table created successfully!';
END
ELSE
BEGIN
    PRINT 'AUDIT_TRAIL table already exists.';
END
GO

-- Create stored procedure to log audit trail
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_LogAuditTrail')
    DROP PROCEDURE [dbo].[SP_LogAuditTrail]
GO

CREATE PROCEDURE [dbo].[SP_LogAuditTrail]
    @TableName NVARCHAR(100),
    @RecordId INT,
    @ActionType NVARCHAR(20),
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100),
    @IPAddress NVARCHAR(50) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @SessionId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[AUDIT_TRAIL] (
        [TABLE_NAME],
        [RECORD_ID],
        [ACTION_TYPE],
        [OLD_VALUES],
        [NEW_VALUES],
        [CREATED_BY],
        [IP_ADDRESS],
        [USER_AGENT],
        [SESSION_ID]
    )
    VALUES (
        @TableName,
        @RecordId,
        @ActionType,
        @OldValues,
        @NewValues,
        @CreatedBy,
        @IPAddress,
        @UserAgent,
        @SessionId
    );
    
    SELECT SCOPE_IDENTITY() AS AuditId;
END
GO

-- Create stored procedure to get audit trail for a record
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAuditTrail')
    DROP PROCEDURE [dbo].[SP_GetAuditTrail]
GO

CREATE PROCEDURE [dbo].[SP_GetAuditTrail]
    @TableName NVARCHAR(100),
    @RecordId INT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM [dbo].[AUDIT_TRAIL]
    WHERE [TABLE_NAME] = @TableName
    AND (@RecordId IS NULL OR [RECORD_ID] = @RecordId);
    
    -- Get paginated results
    SELECT
        [AUDIT_ID],
        [TABLE_NAME],
        [RECORD_ID],
        [ACTION_TYPE],
        [OLD_VALUES],
        [NEW_VALUES],
        [CREATED_DATE],
        [CREATED_BY],
        [MODIFIED_DATE],
        [MODIFIED_BY],
        [IP_ADDRESS],
        [USER_AGENT],
        [SESSION_ID]
    FROM [dbo].[AUDIT_TRAIL]
    WHERE [TABLE_NAME] = @TableName
    AND (@RecordId IS NULL OR [RECORD_ID] = @RecordId)
    ORDER BY [CREATED_DATE] DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT 'Audit Trail setup completed successfully!';
PRINT 'Created: AUDIT_TRAIL table, SP_LogAuditTrail, SP_GetAuditTrail';
