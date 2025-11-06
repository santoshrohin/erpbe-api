IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetAuditConfiguration]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetAuditConfiguration]
GO

CREATE PROCEDURE [dbo].[SP_GetAuditConfiguration]
    @Endpoint NVARCHAR(200),
    @HttpMethod NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Endpoint, HttpMethod, EntityName, EntityIdProperty,
        IsEnabled, TrackPropertyChanges, TrackOldValues, TrackNewValues,
        [Description], CreatedDate, CreatedBy
    FROM AUDIT_CONFIGURATION
    WHERE Endpoint = @Endpoint
      AND HttpMethod = @HttpMethod;
END
GO

