IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetAuditHistory]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetAuditHistory]
GO

CREATE PROCEDURE [dbo].[SP_GetAuditHistory]
    @EntityName NVARCHAR(100),
    @EntityId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        AUDIT_ID as Id, EntityName, EntityId, [Action], UserId, UserName, UserRole,
        CompanyId, IpAddress, UserAgent, Endpoint, HttpMethod,
        [Description], OldValues, NewValues, [Timestamp]
    FROM AUDIT_TRAIL
    WHERE EntityName = @EntityName
      AND EntityId = @EntityId
    ORDER BY [Timestamp] DESC;
END
GO

