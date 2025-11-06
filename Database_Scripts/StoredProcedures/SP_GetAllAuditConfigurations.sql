IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetAllAuditConfigurations]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetAllAuditConfigurations]
GO

CREATE PROCEDURE [dbo].[SP_GetAllAuditConfigurations]
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
GO

