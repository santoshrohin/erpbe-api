CREATE PROCEDURE SP_GetBranchesWithFilters
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'BranchName',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(255) = NULL,
    @BranchName NVARCHAR(100) = NULL,
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
        SET @WhereClause = @WhereClause + ' AND BranchName LIKE ''%' + @SearchTerm + '%'''
    END
    
    -- Add specific filter conditions
    IF @BranchName IS NOT NULL AND @BranchName != ''
        SET @WhereClause = @WhereClause + ' AND BranchName LIKE ''%' + @BranchName + '%'''
    
    -- Validate and build ORDER BY clause
    IF @SortBy IN ('BranchId', 'BranchName')
    BEGIN
        SET @OrderByClause = ' ORDER BY ' + @SortBy + ' ' + UPPER(@SortDirection)
    END
    ELSE
    BEGIN
        SET @OrderByClause = ' ORDER BY BranchName ASC'
    END
    
    -- Get total count
    SET @SQL = 'SELECT @TotalCount = COUNT(*) FROM BranchMaster' + @WhereClause
    EXEC sp_executesql @SQL, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT
    
    -- Get paged data
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize
    SET @SQL = 'SELECT * FROM BranchMaster' + @WhereClause + @OrderByClause + 
               ' OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY'
    
    EXEC sp_executesql @SQL
END
