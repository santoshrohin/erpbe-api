IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CreateAuditConfiguration]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_CreateAuditConfiguration]
GO

CREATE PROCEDURE [dbo].[SP_CreateAuditConfiguration]
    @Endpoint NVARCHAR(200),
    @HttpMethod NVARCHAR(10),
    @EntityName NVARCHAR(100),
    @EntityIdProperty NVARCHAR(50) = NULL,
    @IsEnabled BIT = 1,
    @TrackPropertyChanges BIT = 1,
    @TrackOldValues BIT = 1,
    @TrackNewValues BIT = 1,
    @Description NVARCHAR(500) = NULL,
    @CreatedBy NVARCHAR(100) = NULL,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;
    
    INSERT INTO AUDIT_CONFIGURATION (
        Endpoint, HttpMethod, EntityName, EntityIdProperty,
        IsEnabled, TrackPropertyChanges, TrackOldValues, TrackNewValues,
        [Description], CreatedDate, CreatedBy
    )
    VALUES (
        @Endpoint, @HttpMethod, @EntityName, @EntityIdProperty,
        @IsEnabled, @TrackPropertyChanges, @TrackOldValues, @TrackNewValues,
        @Description, GETUTCDATE(), @CreatedBy
    );
    
    SET @Id = SCOPE_IDENTITY();
END
GO

