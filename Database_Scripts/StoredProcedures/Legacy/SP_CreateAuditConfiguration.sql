CREATE OR ALTER PROCEDURE [dbo].[SP_CreateAuditConfiguration]
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