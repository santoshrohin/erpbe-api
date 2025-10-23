-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-23
-- Description: Deployment script for Audit Trail stored procedures
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Starting deployment of Audit stored procedures...';
GO

-- =============================================
-- SP_CreateAuditEntry
-- =============================================
PRINT 'Creating SP_CreateAuditEntry...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_CreateAuditEntry]
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
    
    SET @Id = SCOPE_IDENTITY(); -- Returns AUDIT_ID
END
GO

PRINT 'SP_CreateAuditEntry created successfully!';
GO

-- =============================================
-- SP_GetAuditEntryById
-- =============================================
PRINT 'Creating SP_GetAuditEntryById...';
GO

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
GO

PRINT 'SP_GetAuditEntryById created successfully!';
GO

-- =============================================
-- SP_GetAuditEntriesWithFilters
-- =============================================
PRINT 'Creating SP_GetAuditEntriesWithFilters...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetAuditEntriesWithFilters]
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SortBy NVARCHAR(50) = 'Timestamp',
    @SortDirection NVARCHAR(4) = 'DESC',
    @SearchTerm NVARCHAR(200) = NULL,
    @EntityName NVARCHAR(100) = NULL,
    @EntityId NVARCHAR(50) = NULL,
    @Action NVARCHAR(50) = NULL,
    @UserId NVARCHAR(50) = NULL,
    @UserName NVARCHAR(100) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Endpoint NVARCHAR(200) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Ensure valid page number and size
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 50;
    IF @PageSize > 100 SET @PageSize = 100;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM AUDIT_TRAIL
    WHERE 
        (@EntityName IS NULL OR EntityName = @EntityName)
        AND (@EntityId IS NULL OR EntityId = @EntityId)
        AND (@Action IS NULL OR [Action] = @Action)
        AND (@UserId IS NULL OR UserId = @UserId)
        AND (@UserName IS NULL OR UserName LIKE '%' + @UserName + '%')
        AND (@FromDate IS NULL OR [Timestamp] >= @FromDate)
        AND (@ToDate IS NULL OR [Timestamp] <= @ToDate)
        AND (@Endpoint IS NULL OR Endpoint = @Endpoint)
        AND (@SearchTerm IS NULL OR 
             EntityName LIKE '%' + @SearchTerm + '%' OR
             [Description] LIKE '%' + @SearchTerm + '%' OR
             UserName LIKE '%' + @SearchTerm + '%');
    
    -- Get paginated results
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = '
    SELECT 
        AUDIT_ID as Id, EntityName, EntityId, [Action], UserId, UserName, UserRole,
        CompanyId, IpAddress, UserAgent, Endpoint, HttpMethod,
        [Description], OldValues, NewValues, [Timestamp]
    FROM AUDIT_TRAIL
    WHERE 
        (@EntityName IS NULL OR EntityName = @EntityName)
        AND (@EntityId IS NULL OR EntityId = @EntityId)
        AND (@Action IS NULL OR [Action] = @Action)
        AND (@UserId IS NULL OR UserId = @UserId)
        AND (@UserName IS NULL OR UserName LIKE ''%'' + @UserName + ''%'')
        AND (@FromDate IS NULL OR [Timestamp] >= @FromDate)
        AND (@ToDate IS NULL OR [Timestamp] <= @ToDate)
        AND (@Endpoint IS NULL OR Endpoint = @Endpoint)
        AND (@SearchTerm IS NULL OR 
             EntityName LIKE ''%'' + @SearchTerm + ''%'' OR
             [Description] LIKE ''%'' + @SearchTerm + ''%'' OR
             UserName LIKE ''%'' + @SearchTerm + ''%'')
    ORDER BY ' + QUOTENAME(@SortBy) + ' ' + @SortDirection + '
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY';
    
    EXEC sp_executesql @SQL, 
        N'@EntityName NVARCHAR(100), @EntityId NVARCHAR(50), @Action NVARCHAR(50), 
          @UserId NVARCHAR(50), @UserName NVARCHAR(100), @FromDate DATETIME, 
          @ToDate DATETIME, @Endpoint NVARCHAR(200), @SearchTerm NVARCHAR(200), 
          @Offset INT, @PageSize INT',
        @EntityName, @EntityId, @Action, @UserId, @UserName, @FromDate, 
        @ToDate, @Endpoint, @SearchTerm, @Offset, @PageSize;
END
GO

PRINT 'SP_GetAuditEntriesWithFilters created successfully!';
GO

-- =============================================
-- SP_GetAuditHistory
-- =============================================
PRINT 'Creating SP_GetAuditHistory...';
GO

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
GO

PRINT 'SP_GetAuditHistory created successfully!';
GO

-- =============================================
-- SP_GetAuditConfiguration
-- =============================================
PRINT 'Creating SP_GetAuditConfiguration...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetAuditConfiguration]
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

PRINT 'SP_GetAuditConfiguration created successfully!';
GO

-- =============================================
-- SP_CreateAuditConfiguration
-- =============================================
PRINT 'Creating SP_CreateAuditConfiguration...';
GO

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
GO

PRINT 'SP_CreateAuditConfiguration created successfully!';
GO

-- =============================================
-- SP_UpdateAuditConfiguration
-- =============================================
PRINT 'Creating SP_UpdateAuditConfiguration...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateAuditConfiguration]
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

PRINT 'SP_UpdateAuditConfiguration created successfully!';
GO

-- =============================================
-- SP_GetAllAuditConfigurations
-- =============================================
PRINT 'Creating SP_GetAllAuditConfigurations...';
GO

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
GO

PRINT 'SP_GetAllAuditConfigurations created successfully!';
GO

PRINT 'All Audit stored procedures deployed successfully!';
GO

