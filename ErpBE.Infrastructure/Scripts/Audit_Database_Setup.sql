USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Audit and Logging Database Setup>
-- =============================================

PRINT 'Starting Audit and Logging Database Setup...';
PRINT '==========================================';

-- Step 1: Create AuditEntry table
PRINT 'Step 1: Creating AuditEntry table...';
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuditEntry' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[AuditEntry](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [EntityName] [nvarchar](100) NOT NULL,
        [EntityId] [nvarchar](50) NOT NULL,
        [Action] [nvarchar](50) NOT NULL,
        [UserId] [nvarchar](100) NOT NULL,
        [UserName] [nvarchar](100) NULL,
        [UserRole] [nvarchar](50) NULL,
        [CompanyId] [nvarchar](50) NULL,
        [IpAddress] [nvarchar](100) NULL,
        [UserAgent] [nvarchar](500) NULL,
        [Endpoint] [nvarchar](200) NULL,
        [HttpMethod] [nvarchar](50) NULL,
        [Timestamp] [datetime] NOT NULL DEFAULT(GETUTCDATE()),
        [Description] [nvarchar](1000) NULL,
        [OldValues] [nvarchar](max) NULL,
        [NewValues] [nvarchar](max) NULL,
        CONSTRAINT [PK_AuditEntry] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    -- Create indexes for better performance
    CREATE NONCLUSTERED INDEX [IX_AuditEntry_EntityName_EntityId] ON [dbo].[AuditEntry] ([EntityName], [EntityId]);
    CREATE NONCLUSTERED INDEX [IX_AuditEntry_UserId] ON [dbo].[AuditEntry] ([UserId]);
    CREATE NONCLUSTERED INDEX [IX_AuditEntry_Timestamp] ON [dbo].[AuditEntry] ([Timestamp]);
    CREATE NONCLUSTERED INDEX [IX_AuditEntry_Action] ON [dbo].[AuditEntry] ([Action]);
    
    PRINT 'AuditEntry table created successfully.';
END
ELSE
BEGIN
    PRINT 'AuditEntry table already exists.';
END

-- Step 2: Create AuditPropertyChange table
PRINT 'Step 2: Creating AuditPropertyChange table...';
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuditPropertyChange' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[AuditPropertyChange](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [AuditEntryId] [int] NOT NULL,
        [PropertyName] [nvarchar](100) NOT NULL,
        [OldValue] [nvarchar](500) NULL,
        [NewValue] [nvarchar](500) NULL,
        [PropertyType] [nvarchar](50) NULL,
        CONSTRAINT [PK_AuditPropertyChange] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AuditPropertyChange_AuditEntry] FOREIGN KEY([AuditEntryId]) REFERENCES [dbo].[AuditEntry] ([Id]) ON DELETE CASCADE
    );
    
    -- Create index for better performance
    CREATE NONCLUSTERED INDEX [IX_AuditPropertyChange_AuditEntryId] ON [dbo].[AuditPropertyChange] ([AuditEntryId]);
    
    PRINT 'AuditPropertyChange table created successfully.';
END
ELSE
BEGIN
    PRINT 'AuditPropertyChange table already exists.';
END

-- Step 3: Create AuditConfiguration table
PRINT 'Step 3: Creating AuditConfiguration table...';
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuditConfiguration' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[AuditConfiguration](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Endpoint] [nvarchar](200) NOT NULL,
        [HttpMethod] [nvarchar](10) NOT NULL,
        [EntityName] [nvarchar](100) NOT NULL,
        [EntityIdProperty] [nvarchar](50) NULL,
        [IsEnabled] [bit] NOT NULL DEFAULT(1),
        [TrackPropertyChanges] [bit] NOT NULL DEFAULT(1),
        [TrackOldValues] [bit] NOT NULL DEFAULT(1),
        [TrackNewValues] [bit] NOT NULL DEFAULT(1),
        [Description] [nvarchar](500) NULL,
        [CreatedDate] [datetime] NOT NULL DEFAULT(GETUTCDATE()),
        [CreatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_AuditConfiguration] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_AuditConfiguration_Endpoint_Method] UNIQUE ([Endpoint], [HttpMethod])
    );
    
    PRINT 'AuditConfiguration table created successfully.';
END
ELSE
BEGIN
    PRINT 'AuditConfiguration table already exists.';
END

-- Step 4: Create stored procedures
PRINT 'Step 4: Creating stored procedures...';

-- SP_CreateAuditEntry
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CreateAuditEntry')
    DROP PROCEDURE [dbo].[SP_CreateAuditEntry];
GO

CREATE PROCEDURE [dbo].[SP_CreateAuditEntry]
    @EntityName NVARCHAR(100),
    @EntityId NVARCHAR(50),
    @Action NVARCHAR(50),
    @UserId NVARCHAR(100),
    @UserName NVARCHAR(100) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @CompanyId NVARCHAR(50) = NULL,
    @IpAddress NVARCHAR(100) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @Endpoint NVARCHAR(200) = NULL,
    @HttpMethod NVARCHAR(50) = NULL,
    @Description NVARCHAR(1000) = NULL,
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL,
    @Timestamp DATETIME = NULL,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Timestamp IS NULL
        SET @Timestamp = GETUTCDATE();
    
    INSERT INTO [dbo].[AuditEntry] (
        [EntityName], [EntityId], [Action], [UserId], [UserName], [UserRole],
        [CompanyId], [IpAddress], [UserAgent], [Endpoint], [HttpMethod],
        [Description], [OldValues], [NewValues], [Timestamp]
    )
    VALUES (
        @EntityName, @EntityId, @Action, @UserId, @UserName, @UserRole,
        @CompanyId, @IpAddress, @UserAgent, @Endpoint, @HttpMethod,
        @Description, @OldValues, @NewValues, @Timestamp
    );
    
    SET @Id = SCOPE_IDENTITY();
END
GO

-- SP_GetAuditEntryById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAuditEntryById')
    DROP PROCEDURE [dbo].[SP_GetAuditEntryById];
GO

CREATE PROCEDURE [dbo].[SP_GetAuditEntryById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ae.[Id], ae.[EntityName], ae.[EntityId], ae.[Action], ae.[UserId],
        ae.[UserName], ae.[UserRole], ae.[CompanyId], ae.[IpAddress],
        ae.[UserAgent], ae.[Endpoint], ae.[HttpMethod], ae.[Timestamp],
        ae.[Description], ae.[OldValues], ae.[NewValues]
    FROM [dbo].[AuditEntry] ae
    WHERE ae.[Id] = @Id;
    
    -- Get property changes
    SELECT 
        apc.[Id], apc.[PropertyName], apc.[OldValue], apc.[NewValue], apc.[PropertyType]
    FROM [dbo].[AuditPropertyChange] apc
    WHERE apc.[AuditEntryId] = @Id;
END
GO

-- SP_GetAuditEntriesWithFilters
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAuditEntriesWithFilters')
    DROP PROCEDURE [dbo].[SP_GetAuditEntriesWithFilters];
GO

CREATE PROCEDURE [dbo].[SP_GetAuditEntriesWithFilters]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'Timestamp',
    @SortDirection NVARCHAR(10) = 'DESC',
    @SearchTerm NVARCHAR(100) = NULL,
    @EntityName NVARCHAR(100) = NULL,
    @EntityId NVARCHAR(50) = NULL,
    @Action NVARCHAR(50) = NULL,
    @UserId NVARCHAR(100) = NULL,
    @UserName NVARCHAR(100) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Endpoint NVARCHAR(200) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM [dbo].[AuditEntry] ae
    WHERE (@SearchTerm IS NULL OR 
           ae.[EntityName] LIKE '%' + @SearchTerm + '%' OR
           ae.[EntityId] LIKE '%' + @SearchTerm + '%' OR
           ae.[UserName] LIKE '%' + @SearchTerm + '%' OR
           ae.[Description] LIKE '%' + @SearchTerm + '%')
    AND (@EntityName IS NULL OR ae.[EntityName] = @EntityName)
    AND (@EntityId IS NULL OR ae.[EntityId] = @EntityId)
    AND (@Action IS NULL OR ae.[Action] = @Action)
    AND (@UserId IS NULL OR ae.[UserId] = @UserId)
    AND (@UserName IS NULL OR ae.[UserName] LIKE '%' + @UserName + '%')
    AND (@FromDate IS NULL OR ae.[Timestamp] >= @FromDate)
    AND (@ToDate IS NULL OR ae.[Timestamp] <= @ToDate)
    AND (@Endpoint IS NULL OR ae.[Endpoint] LIKE '%' + @Endpoint + '%');
    
    -- Get paginated results
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        ae.[Id], ae.[EntityName], ae.[EntityId], ae.[Action], ae.[UserId],
        ae.[UserName], ae.[UserRole], ae.[CompanyId], ae.[IpAddress],
        ae.[UserAgent], ae.[Endpoint], ae.[HttpMethod], ae.[Timestamp],
        ae.[Description], ae.[OldValues], ae.[NewValues]
    FROM [dbo].[AuditEntry] ae
    WHERE (@SearchTerm IS NULL OR 
           ae.[EntityName] LIKE '%' + @SearchTerm + '%' OR
           ae.[EntityId] LIKE '%' + @SearchTerm + '%' OR
           ae.[UserName] LIKE '%' + @SearchTerm + '%' OR
           ae.[Description] LIKE '%' + @SearchTerm + '%')
    AND (@EntityName IS NULL OR ae.[EntityName] = @EntityName)
    AND (@EntityId IS NULL OR ae.[EntityId] = @EntityId)
    AND (@Action IS NULL OR ae.[Action] = @Action)
    AND (@UserId IS NULL OR ae.[UserId] = @UserId)
    AND (@UserName IS NULL OR ae.[UserName] LIKE '%' + @UserName + '%')
    AND (@FromDate IS NULL OR ae.[Timestamp] >= @FromDate)
    AND (@ToDate IS NULL OR ae.[Timestamp] <= @ToDate)
    AND (@Endpoint IS NULL OR ae.[Endpoint] LIKE '%' + @Endpoint + '%')
    ORDER BY 
        CASE WHEN @SortBy = 'EntityName' AND @SortDirection = 'ASC' THEN ae.[EntityName] END ASC,
        CASE WHEN @SortBy = 'EntityName' AND @SortDirection = 'DESC' THEN ae.[EntityName] END DESC,
        CASE WHEN @SortBy = 'Action' AND @SortDirection = 'ASC' THEN ae.[Action] END ASC,
        CASE WHEN @SortBy = 'Action' AND @SortDirection = 'DESC' THEN ae.[Action] END DESC,
        CASE WHEN @SortBy = 'UserName' AND @SortDirection = 'ASC' THEN ae.[UserName] END ASC,
        CASE WHEN @SortBy = 'UserName' AND @SortDirection = 'DESC' THEN ae.[UserName] END DESC,
        CASE WHEN @SortBy = 'Timestamp' AND @SortDirection = 'ASC' THEN ae.[Timestamp] END ASC,
        CASE WHEN @SortBy = 'Timestamp' AND @SortDirection = 'DESC' THEN ae.[Timestamp] END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP_GetAuditHistory
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAuditHistory')
    DROP PROCEDURE [dbo].[SP_GetAuditHistory];
GO

CREATE PROCEDURE [dbo].[SP_GetAuditHistory]
    @EntityName NVARCHAR(100),
    @EntityId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ae.[Id], ae.[EntityName], ae.[EntityId], ae.[Action], ae.[UserId],
        ae.[UserName], ae.[UserRole], ae.[CompanyId], ae.[IpAddress],
        ae.[UserAgent], ae.[Endpoint], ae.[HttpMethod], ae.[Timestamp],
        ae.[Description], ae.[OldValues], ae.[NewValues]
    FROM [dbo].[AuditEntry] ae
    WHERE ae.[EntityName] = @EntityName AND ae.[EntityId] = @EntityId
    ORDER BY ae.[Timestamp] DESC;
END
GO

-- SP_GetAuditConfiguration
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAuditConfiguration')
    DROP PROCEDURE [dbo].[SP_GetAuditConfiguration];
GO

CREATE PROCEDURE [dbo].[SP_GetAuditConfiguration]
    @Endpoint NVARCHAR(200),
    @HttpMethod NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id], [Endpoint], [HttpMethod], [EntityName], [EntityIdProperty],
        [IsEnabled], [TrackPropertyChanges], [TrackOldValues], [TrackNewValues],
        [Description], [CreatedDate], [CreatedBy]
    FROM [dbo].[AuditConfiguration]
    WHERE [Endpoint] = @Endpoint AND [HttpMethod] = @HttpMethod;
END
GO

-- SP_CreateAuditConfiguration
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CreateAuditConfiguration')
    DROP PROCEDURE [dbo].[SP_CreateAuditConfiguration];
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
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[AuditConfiguration] (
        [Endpoint], [HttpMethod], [EntityName], [EntityIdProperty],
        [IsEnabled], [TrackPropertyChanges], [TrackOldValues], [TrackNewValues],
        [Description], [CreatedBy]
    )
    VALUES (
        @Endpoint, @HttpMethod, @EntityName, @EntityIdProperty,
        @IsEnabled, @TrackPropertyChanges, @TrackOldValues, @TrackNewValues,
        @Description, @CreatedBy
    );
    
    SET @Id = SCOPE_IDENTITY();
END
GO

-- SP_UpdateAuditConfiguration
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_UpdateAuditConfiguration')
    DROP PROCEDURE [dbo].[SP_UpdateAuditConfiguration];
GO

CREATE PROCEDURE [dbo].[SP_UpdateAuditConfiguration]
    @Id INT,
    @IsEnabled BIT,
    @TrackPropertyChanges BIT,
    @TrackOldValues BIT,
    @TrackNewValues BIT,
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[AuditConfiguration]
    SET 
        [IsEnabled] = @IsEnabled,
        [TrackPropertyChanges] = @TrackPropertyChanges,
        [TrackOldValues] = @TrackOldValues,
        [TrackNewValues] = @TrackNewValues,
        [Description] = @Description
    WHERE [Id] = @Id;
END
GO

-- SP_GetAllAuditConfigurations
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetAllAuditConfigurations')
    DROP PROCEDURE [dbo].[SP_GetAllAuditConfigurations];
GO

CREATE PROCEDURE [dbo].[SP_GetAllAuditConfigurations]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id], [Endpoint], [HttpMethod], [EntityName], [EntityIdProperty],
        [IsEnabled], [TrackPropertyChanges], [TrackOldValues], [TrackNewValues],
        [Description], [CreatedDate], [CreatedBy]
    FROM [dbo].[AuditConfiguration]
    ORDER BY [Endpoint], [HttpMethod];
END
GO

-- Step 5: Insert default audit configurations
PRINT 'Step 5: Inserting default audit configurations...';

-- Configure audit for Farmer endpoints
IF NOT EXISTS (SELECT 1 FROM AuditConfiguration WHERE Endpoint = '/api/Farmer' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO [dbo].[AuditConfiguration] ([Endpoint], [HttpMethod], [EntityName], [EntityIdProperty], [IsEnabled], [Description], [CreatedBy])
    VALUES ('/api/Farmer', 'POST', 'Farmer', 'Id', 1, 'Audit farmer creation', 'System');
END

IF NOT EXISTS (SELECT 1 FROM AuditConfiguration WHERE Endpoint = '/api/Farmer' AND HttpMethod = 'PUT')
BEGIN
    INSERT INTO [dbo].[AuditConfiguration] ([Endpoint], [HttpMethod], [EntityName], [EntityIdProperty], [IsEnabled], [Description], [CreatedBy])
    VALUES ('/api/Farmer', 'PUT', 'Farmer', 'Id', 1, 'Audit farmer updates', 'System');
END

-- Configure audit for Branch endpoints
IF NOT EXISTS (SELECT 1 FROM AuditConfiguration WHERE Endpoint = '/api/Branch' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO [dbo].[AuditConfiguration] ([Endpoint], [HttpMethod], [EntityName], [EntityIdProperty], [IsEnabled], [Description], [CreatedBy])
    VALUES ('/api/Branch', 'POST', 'Branch', 'Id', 1, 'Audit branch creation', 'System');
END

-- Configure audit for Placement endpoints
IF NOT EXISTS (SELECT 1 FROM AuditConfiguration WHERE Endpoint = '/api/Placement' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO [dbo].[AuditConfiguration] ([Endpoint], [HttpMethod], [EntityName], [EntityIdProperty], [IsEnabled], [Description], [CreatedBy])
    VALUES ('/api/Placement', 'POST', 'Placement', 'Id', 1, 'Audit placement creation', 'System');
END

PRINT 'Default audit configurations inserted successfully.';

-- Step 6: Display final results
PRINT 'Step 6: Final setup summary...';
PRINT '==========================================';
PRINT 'AUDIT AND LOGGING SETUP COMPLETED!';
PRINT '==========================================';
PRINT 'Tables Created:';
PRINT '- AuditEntry (main audit log)';
PRINT '- AuditPropertyChange (property-level changes)';
PRINT '- AuditConfiguration (configurable audit settings)';
PRINT '==========================================';
PRINT 'Stored Procedures Created:';
PRINT '- SP_CreateAuditEntry';
PRINT '- SP_GetAuditEntryById';
PRINT '- SP_GetAuditEntriesWithFilters';
PRINT '- SP_GetAuditHistory';
PRINT '- SP_GetAuditConfiguration';
PRINT '- SP_CreateAuditConfiguration';
PRINT '- SP_UpdateAuditConfiguration';
PRINT '- SP_GetAllAuditConfigurations';
PRINT '==========================================';
PRINT 'Default audit configurations created for:';
PRINT '- Farmer POST/PUT operations';
PRINT '- Branch POST operations';
PRINT '- Placement POST operations';
PRINT '==========================================';
PRINT 'You can now configure additional audit settings via the API!';
PRINT '==========================================';
