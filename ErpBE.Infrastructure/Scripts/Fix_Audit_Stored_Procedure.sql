-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-21
-- Description: Fix SP_GetAuditTrail to use proper column aliases
-- =============================================

USE [db_a2ea4b_sunv2]
GO

-- Fix SP_GetAuditTrail with proper column aliases
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
    
    -- Get paginated results with proper column aliases
    SELECT
        [AUDIT_ID] AS AuditId,
        [TABLE_NAME] AS TableName,
        [RECORD_ID] AS RecordId,
        [ACTION_TYPE] AS ActionType,
        [OLD_VALUES] AS OldValues,
        [NEW_VALUES] AS NewValues,
        [CREATED_DATE] AS CreatedDate,
        [CREATED_BY] AS CreatedBy,
        [MODIFIED_DATE] AS ModifiedDate,
        [MODIFIED_BY] AS ModifiedBy,
        [IP_ADDRESS] AS IpAddress,
        [USER_AGENT] AS UserAgent,
        [SESSION_ID] AS SessionId
    FROM [dbo].[AUDIT_TRAIL]
    WHERE [TABLE_NAME] = @TableName
    AND (@RecordId IS NULL OR [RECORD_ID] = @RecordId)
    ORDER BY [CREATED_DATE] DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT 'SP_GetAuditTrail updated with proper column aliases!';
