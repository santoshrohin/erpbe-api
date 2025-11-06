IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetAuditEntryById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetAuditEntryById]
GO

CREATE PROCEDURE [dbo].[SP_GetAuditEntryById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        AUDIT_ID as Id, EntityName, EntityId, [Action], UserId, UserName, UserRole,
        CompanyId, IpAddress, UserAgent, Endpoint, HttpMethod,
        [Description], OldValues, NewValues, [Timestamp]
    FROM AUDIT_TRAIL
    WHERE AUDIT_ID = @Id;
END
GO

