CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllAuditConfigurations]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Endpoint, HttpMethod, EntityName, EntityIdProperty,
        IsEnabled, TrackPropertyChanges, TrackOldValues, TrackNewValues,
        [Description], CreatedDate, CreatedBy
    FROM AUDIT_CONFIGURATION
    ORDER BY Endpoint, HttpMethod;
END