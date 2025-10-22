CREATE PROCEDURE SP_GetPlacementsWithFilters
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'PlacementDate',
    @SortDirection NVARCHAR(4) = 'DESC',
    @SearchTerm NVARCHAR(255) = NULL,
    @FarmerId INT = NULL,
    @BranchId INT = NULL,
    @LineId INT = NULL,
    @FarmerName NVARCHAR(100) = NULL,
    @FarmerCode NVARCHAR(50) = NULL,
    @PlacementDateFrom DATETIME = NULL,
    @PlacementDateTo DATETIME = NULL,
    @MinPlacementQty DECIMAL(10,2) = NULL,
    @MaxPlacementQty DECIMAL(10,2) = NULL,
    @Status BIT = NULL,
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
        SET @WhereClause = @WhereClause + ' AND (FarmerName LIKE ''%' + @SearchTerm + '%'' OR FarmerCode LIKE ''%' + @SearchTerm + '%'' OR FarmerAddress LIKE ''%' + @SearchTerm + '%'')'
    END
    
    -- Add specific filter conditions
    IF @FarmerId IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND FarmerId = ' + CAST(@FarmerId AS NVARCHAR(10))
        
    IF @BranchId IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND BranchId = ' + CAST(@BranchId AS NVARCHAR(10))
        
    IF @LineId IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND LineId = ' + CAST(@LineId AS NVARCHAR(10))
        
    IF @FarmerName IS NOT NULL AND @FarmerName != ''
        SET @WhereClause = @WhereClause + ' AND FarmerName LIKE ''%' + @FarmerName + '%'''
        
    IF @FarmerCode IS NOT NULL AND @FarmerCode != ''
        SET @WhereClause = @WhereClause + ' AND FarmerCode LIKE ''%' + @FarmerCode + '%'''
        
    IF @PlacementDateFrom IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND PlacementDate >= ''' + CONVERT(NVARCHAR(19), @PlacementDateFrom, 120) + ''''
        
    IF @PlacementDateTo IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND PlacementDate <= ''' + CONVERT(NVARCHAR(19), @PlacementDateTo, 120) + ''''
        
    IF @MinPlacementQty IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND PlacementQty >= ' + CAST(@MinPlacementQty AS NVARCHAR(20))
        
    IF @MaxPlacementQty IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND PlacementQty <= ' + CAST(@MaxPlacementQty AS NVARCHAR(20))
        
    IF @Status IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND Status = ' + CAST(@Status AS NVARCHAR(1))
    
    -- Validate and build ORDER BY clause
    IF @SortBy IN ('PlacementId', 'EntryDate', 'FarmerId', 'FarmerName', 'FarmerCode', 'BranchId', 'LineId', 'PlacementQty', 'PlacementDate', 'Status')
    BEGIN
        SET @OrderByClause = ' ORDER BY ' + @SortBy + ' ' + UPPER(@SortDirection)
    END
    ELSE
    BEGIN
        SET @OrderByClause = ' ORDER BY PlacementDate DESC'
    END
    
    -- Get total count
    SET @SQL = 'SELECT @TotalCount = COUNT(*) FROM Placement' + @WhereClause
    EXEC sp_executesql @SQL, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT
    
    -- Get paged data
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize
    SET @SQL = 'SELECT * FROM Placement' + @WhereClause + @OrderByClause + 
               ' OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY'
    
    EXEC sp_executesql @SQL
END
