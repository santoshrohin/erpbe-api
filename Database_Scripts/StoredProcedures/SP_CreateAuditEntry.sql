IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CreateAuditEntry]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_CreateAuditEntry]
GO

CREATE PROCEDURE [dbo].[SP_CreateAuditEntry]
    @EntityName NVARCHAR(100),
    @EntityId NVARCHAR(50),
    @Action NVARCHAR(50),
    @UserId NVARCHAR(50),
    @UserName NVARCHAR(100) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @CompanyId NVARCHAR(50) = NULL,
    @IpAddress NVARCHAR(50) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @Endpoint NVARCHAR(200) = NULL,
    @HttpMethod NVARCHAR(10) = NULL,
    @Description NVARCHAR(500) = NULL,
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL,
    @Timestamp DATETIME = NULL,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;
    
    IF @Timestamp IS NULL
        SET @Timestamp = GETUTCDATE();
    
    INSERT INTO AUDIT_TRAIL (
        EntityName, EntityId, [Action], UserId, UserName, UserRole,
        CompanyId, IpAddress, UserAgent, Endpoint, HttpMethod,
        [Description], OldValues, NewValues, [Timestamp]
    )
    VALUES (
        @EntityName, @EntityId, @Action, @UserId, @UserName, @UserRole,
        @CompanyId, @IpAddress, @UserAgent, @Endpoint, @HttpMethod,
        @Description, @OldValues, @NewValues, @Timestamp
    );
    
    SET @Id = SCOPE_IDENTITY();
END
GO

