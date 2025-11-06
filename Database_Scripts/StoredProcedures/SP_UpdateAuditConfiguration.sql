IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_UpdateAuditConfiguration]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_UpdateAuditConfiguration]
GO

CREATE PROCEDURE [dbo].[SP_UpdateAuditConfiguration]
    @Id INT,
    @IsEnabled BIT = NULL,
    @TrackPropertyChanges BIT = NULL,
    @TrackOldValues BIT = NULL,
    @TrackNewValues BIT = NULL,
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT OFF;
    
    UPDATE AUDIT_CONFIGURATION
    SET 
        IsEnabled = ISNULL(@IsEnabled, IsEnabled),
        TrackPropertyChanges = ISNULL(@TrackPropertyChanges, TrackPropertyChanges),
        TrackOldValues = ISNULL(@TrackOldValues, TrackOldValues),
        TrackNewValues = ISNULL(@TrackNewValues, TrackNewValues),
        [Description] = ISNULL(@Description, [Description])
    WHERE Id = @Id;
END
GO

