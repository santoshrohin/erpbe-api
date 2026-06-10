CREATE OR ALTER PROCEDURE [dbo].[SP_GetAuditHistory]
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