CREATE OR ALTER PROCEDURE [dbo].[ERP_GetCustomerTypeMasters]
    @CTM_CM_COMP_ID INT = NULL,
    @CTM_TYPE_CODE VARCHAR(50) = NULL,
    @CTM_TYPE_DESC VARCHAR(150) = NULL,
    @CTM_FIRST_LETTER VARCHAR(50) = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @SortBy VARCHAR(50) = 'TypeDescription',
    @SortDirection VARCHAR(4) = 'ASC',
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Build the WHERE clause dynamically
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CountSQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE ES_DELETE = 0';

    IF @CTM_CM_COMP_ID IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND CTM_CM_COMP_ID = @CTM_CM_COMP_ID';

    IF @CTM_TYPE_CODE IS NOT NULL AND @CTM_TYPE_CODE <> ''
        SET @WhereClause = @WhereClause + ' AND CTM_TYPE_CODE LIKE ''%'' + @CTM_TYPE_CODE + ''%''';

    IF @CTM_TYPE_DESC IS NOT NULL AND @CTM_TYPE_DESC <> ''
        SET @WhereClause = @WhereClause + ' AND CTM_TYPE_DESC LIKE ''%'' + @CTM_TYPE_DESC + ''%''';

    IF @CTM_FIRST_LETTER IS NOT NULL AND @CTM_FIRST_LETTER <> ''
        SET @WhereClause = @WhereClause + ' AND CTM_FIRST_LETTER LIKE ''%'' + @CTM_FIRST_LETTER + ''%''';

    IF @SearchTerm IS NOT NULL AND @SearchTerm <> ''
        SET @WhereClause = @WhereClause + ' AND (CTM_TYPE_CODE LIKE ''%'' + @SearchTerm + ''%'' 
                                                   OR CTM_TYPE_DESC LIKE ''%'' + @SearchTerm + ''%'' 
                                                   OR CTM_FIRST_LETTER LIKE ''%'' + @SearchTerm + ''%'')';

    -- Map SortBy to actual column names
    DECLARE @OrderByColumn VARCHAR(50);
    SET @OrderByColumn = CASE @SortBy
        WHEN 'TypeCode' THEN 'CTM_TYPE_CODE'
        WHEN 'TypeDescription' THEN 'CTM_TYPE_DESC'
        WHEN 'FirstLetter' THEN 'CTM_FIRST_LETTER'
        ELSE 'CTM_TYPE_DESC'
    END;

    -- Get total count
    SET @CountSQL = 'SELECT @TotalCount = COUNT(*) FROM CUSTOMER_TYPE_MASTER' + @WhereClause;

    EXEC sp_executesql @CountSQL,
        N'@CTM_CM_COMP_ID INT, @CTM_TYPE_CODE VARCHAR(50), @CTM_TYPE_DESC VARCHAR(150), 
          @CTM_FIRST_LETTER VARCHAR(50), @SearchTerm NVARCHAR(255), @TotalCount INT OUTPUT',
        @CTM_CM_COMP_ID, @CTM_TYPE_CODE, @CTM_TYPE_DESC, @CTM_FIRST_LETTER, @SearchTerm, @TotalCount OUTPUT;

    -- Get paginated results
    SET @SQL = 'SELECT 
                    CTM_CODE,
                    CTM_CM_COMP_ID,
                    CTM_TYPE_CODE,
                    CTM_TYPE_DESC,
                    CTM_FIRST_LETTER,
                    ES_DELETE,
                    MODIFY
                FROM CUSTOMER_TYPE_MASTER' + @WhereClause + '
                ORDER BY ' + @OrderByColumn + ' ' + @SortDirection + '
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @SQL,
        N'@CTM_CM_COMP_ID INT, @CTM_TYPE_CODE VARCHAR(50), @CTM_TYPE_DESC VARCHAR(150), 
          @CTM_FIRST_LETTER VARCHAR(50), @SearchTerm NVARCHAR(255), @Offset INT, @PageSize INT',
        @CTM_CM_COMP_ID, @CTM_TYPE_CODE, @CTM_TYPE_DESC, @CTM_FIRST_LETTER, @SearchTerm, @Offset, @PageSize;
END;
GO

