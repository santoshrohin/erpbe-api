CREATE OR ALTER PROCEDURE [dbo].[SP_LogAuditTrail]
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