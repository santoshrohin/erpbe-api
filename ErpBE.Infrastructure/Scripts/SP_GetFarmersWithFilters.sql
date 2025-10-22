CREATE PROCEDURE SP_GetFarmersWithFilters
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'FarmerName',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(255) = NULL,
    @BranchId INT = NULL,
    @LineId INT = NULL,
    @FarmerCode NVARCHAR(50) = NULL,
    @FarmerName NVARCHAR(100) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Build dynamic WHERE clause
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 '
    DECLARE @OrderByClause NVARCHAR(MAX) = ''
    DECLARE @SQL NVARCHAR(MAX)
    
    -- Add search term condition (searches across multiple columns)
    IF @SearchTerm IS NOT NULL AND @SearchTerm != ''
    BEGIN
        SET @WhereClause = @WhereClause + ' AND (FarmerName LIKE ''%' + @SearchTerm + '%'' OR FarmerCode LIKE ''%' + @SearchTerm + '%'' OR FarmerAddress LIKE ''%' + @SearchTerm + '%'')'
    END
    
    -- Add specific filter conditions
    IF @BranchId IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND BranchId = ' + CAST(@BranchId AS NVARCHAR(10))
        
    IF @LineId IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND LineId = ' + CAST(@LineId AS NVARCHAR(10))
        
    IF @FarmerCode IS NOT NULL AND @FarmerCode != ''
        SET @WhereClause = @WhereClause + ' AND FarmerCode LIKE ''%' + @FarmerCode + '%'''
        
    IF @FarmerName IS NOT NULL AND @FarmerName != ''
        SET @WhereClause = @WhereClause + ' AND FarmerName LIKE ''%' + @FarmerName + '%'''
    
    -- Validate and build ORDER BY clause
    IF @SortBy IN ('FarmerId', 'FarmerName', 'FarmerCode', 'FarmerAddress', 'BranchId', 'LineId')
    BEGIN
        SET @OrderByClause = ' ORDER BY ' + @SortBy + ' ' + UPPER(@SortDirection)
    END
    ELSE
    BEGIN
        SET @OrderByClause = ' ORDER BY FarmerName ASC'
    END
    
    -- Get total count
    SET @SQL = 'SELECT @TotalCount = COUNT(*) FROM FarmerMaster' + @WhereClause
    EXEC sp_executesql @SQL, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT
    
    -- Get paged data
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize
    SET @SQL = 'SELECT * FROM FarmerMaster' + @WhereClause + @OrderByClause + 
               ' OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY'
    
    EXEC sp_executesql @SQL
END
