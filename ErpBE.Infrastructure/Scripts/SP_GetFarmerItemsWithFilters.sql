CREATE PROCEDURE SP_GetFarmerItemsWithFilters
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'ItemName',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(255) = NULL,
    @ItemType NVARCHAR(50) = NULL,
    @ItemName NVARCHAR(100) = NULL,
    @MinFeedPercent DECIMAL(5,2) = NULL,
    @MaxFeedPercent DECIMAL(5,2) = NULL,
    @MinOffsetDays INT = NULL,
    @MaxOffsetDays INT = NULL,
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
        SET @WhereClause = @WhereClause + ' AND (ItemType LIKE ''%' + @SearchTerm + '%'' OR ItemName LIKE ''%' + @SearchTerm + '%'')'
    END
    
    -- Add specific filter conditions
    IF @ItemType IS NOT NULL AND @ItemType != ''
        SET @WhereClause = @WhereClause + ' AND ItemType LIKE ''%' + @ItemType + '%'''
        
    IF @ItemName IS NOT NULL AND @ItemName != ''
        SET @WhereClause = @WhereClause + ' AND ItemName LIKE ''%' + @ItemName + '%'''
        
    IF @MinFeedPercent IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND FeedPercent >= ' + CAST(@MinFeedPercent AS NVARCHAR(10))
        
    IF @MaxFeedPercent IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND FeedPercent <= ' + CAST(@MaxFeedPercent AS NVARCHAR(10))
        
    IF @MinOffsetDays IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND OffsetDays >= ' + CAST(@MinOffsetDays AS NVARCHAR(10))
        
    IF @MaxOffsetDays IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND OffsetDays <= ' + CAST(@MaxOffsetDays AS NVARCHAR(10))
    
    -- Validate and build ORDER BY clause
    IF @SortBy IN ('Id', 'ItemType', 'ItemName', 'FeedPercent', 'OffsetDays', 'StandardBagSize')
    BEGIN
        SET @OrderByClause = ' ORDER BY ' + @SortBy + ' ' + UPPER(@SortDirection)
    END
    ELSE
    BEGIN
        SET @OrderByClause = ' ORDER BY ItemName ASC'
    END
    
    -- Get total count
    SET @SQL = 'SELECT @TotalCount = COUNT(*) FROM FarmerItemMaster' + @WhereClause
    EXEC sp_executesql @SQL, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT
    
    -- Get paged data
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize
    SET @SQL = 'SELECT * FROM FarmerItemMaster' + @WhereClause + @OrderByClause + 
               ' OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY'
    
    EXEC sp_executesql @SQL
END
