CREATE PROCEDURE SP_GetLinesWithFilters
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'LineName',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(255) = NULL,
    @LineName NVARCHAR(100) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Build dynamic WHERE clause
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 '
    DECLARE @OrderByClause NVARCHAR(MAX) = ''
    DECLARE @SQL NVARCHAR(MAX)
    
    -- Add search term condition
    IF @SearchTerm IS NOT NULL AND @SearchTerm != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND LineName LIKE ''%' + @SearchTerm + '%'''
    END
    
    -- Add specific filter conditions
    IF @LineName IS NOT NULL AND @LineName != ''
        SET @WhereClause = @WhereClause + ' AND LineName LIKE ''%' + @LineName + '%'''
    
    -- Validate and build ORDER BY clause
    IF @SortBy IN ('LineId', 'LineName')
    BEGIN
        SET @OrderByClause = ' ORDER BY ' + @SortBy + ' ' + UPPER(@SortDirection)
    END
    ELSE
    BEGIN
        SET @OrderByClause = ' ORDER BY LineName ASC'
    END
    
    -- Get total count
    SET @SQL = 'SELECT @TotalCount = COUNT(*) FROM LineMaster' + @WhereClause
    EXEC sp_executesql @SQL, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT
    
    -- Get paged data
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize
    SET @SQL = 'SELECT * FROM LineMaster' + @WhereClause + @OrderByClause + 
               ' OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY'
    
    EXEC sp_executesql @SQL
END
