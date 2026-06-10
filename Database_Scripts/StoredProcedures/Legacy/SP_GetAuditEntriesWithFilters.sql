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