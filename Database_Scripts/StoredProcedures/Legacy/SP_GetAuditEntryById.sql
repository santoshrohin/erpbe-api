CREATE OR ALTER PROCEDURE [dbo].[SP_GetAuditEntryById]
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