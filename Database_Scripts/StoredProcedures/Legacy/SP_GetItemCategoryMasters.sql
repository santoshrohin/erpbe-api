CREATE OR ALTER PROCEDURE [dbo].[SP_GetItemCategoryMasters]
    @PageNumber INT = 1,
    @PageSize INT = 15,
    @SortBy NVARCHAR(50) = 'I_CAT_NAME',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(200) = NULL,
    @CompanyId INT = NULL,
    @IsActive BIT = NULL,
    @IsAutoShortClose BIT = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate and set defaults
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 15;
    IF @PageSize > 100 SET @PageSize = 100;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM ITEM_CATEGORY_MASTER
    WHERE 
        (@CompanyId IS NULL OR I_CAT_CM_COMP_ID = @CompanyId)
        AND (@IsActive IS NULL OR (CASE WHEN ES_DELETE = 0 THEN 1 ELSE 0 END) = @IsActive)
        AND (@IsAutoShortClose IS NULL OR I_CAT_SHORTCLOSE = @IsAutoShortClose)
        AND (@SearchTerm IS NULL OR I_CAT_NAME LIKE '%' + @SearchTerm + '%');
    
    -- Get paginated results with dynamic sorting
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = '
    SELECT 
        I_CAT_CODE AS CategoryId,
        I_CAT_NAME AS CategoryName,
        I_CAT_CM_COMP_ID AS CompanyId,
        I_CAT_SHORTCLOSE AS IsAutoShortClose,
        CASE WHEN ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive
    FROM ITEM_CATEGORY_MASTER
    WHERE 
        (@CompanyId IS NULL OR I_CAT_CM_COMP_ID = @CompanyId)
        AND (@IsActive IS NULL OR (CASE WHEN ES_DELETE = 0 THEN 1 ELSE 0 END) = @IsActive)
        AND (@IsAutoShortClose IS NULL OR I_CAT_SHORTCLOSE = @IsAutoShortClose)
        AND (@SearchTerm IS NULL OR I_CAT_NAME LIKE ''%'' + @SearchTerm + ''%'')
    ORDER BY ' + QUOTENAME(@SortBy) + ' ' + @SortDirection + '
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY';
    
    EXEC sp_executesql @SQL,
        N'@CompanyId INT, @IsActive BIT, @IsAutoShortClose BIT, @SearchTerm NVARCHAR(200), @Offset INT, @PageSize INT',
        @CompanyId, @IsActive, @IsAutoShortClose, @SearchTerm, @Offset, @PageSize;
END