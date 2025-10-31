CREATE PROCEDURE [dbo].[usp_GetDropdownDataPaged]
    @Table NVARCHAR(100),
    @IdColumn NVARCHAR(50),
    @DisplayColumn NVARCHAR(50),
    @WhereClause NVARCHAR(1000) = NULL,
    @OrderBy NVARCHAR(100) = NULL,
    @SearchText NVARCHAR(250) = NULL,
    @Skip INT = 0,
    @Take INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @Where NVARCHAR(MAX) = '';
    DECLARE @Order NVARCHAR(MAX);
    -- Build dynamic WHERE clause
    IF @WhereClause IS NOT NULL AND LTRIM(RTRIM(@WhereClause)) <> ''
        SET @Where = ' WHERE ' + @WhereClause;
    -- Add search filter
    IF @SearchText IS NOT NULL AND LTRIM(RTRIM(@SearchText)) <> ''
    BEGIN
        IF LEN(@Where) > 0
            SET @Where = @Where + ' AND (';
        ELSE
            SET @Where = ' WHERE (';
        SET @Where = @Where + @DisplayColumn + ' LIKE @SearchFilter' + ' OR ' + @IdColumn + ' LIKE @SearchFilter';
        SET @Where = @Where + ')';
    END
    SET @Order = ' ORDER BY ' + CASE WHEN @OrderBy IS NOT NULL AND LEN(@OrderBy) > 0 THEN @OrderBy ELSE @DisplayColumn + ' ASC' END + ' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY';
    SET @Sql = 'SELECT ' + @IdColumn + ' AS Id, ' + @DisplayColumn + ' AS DisplayName FROM ' + @Table + @Where + @Order + '; ' +
        'SELECT COUNT(1) AS TotalCount FROM ' + @Table + @Where + ';';
    DECLARE @ParamDef NVARCHAR(MAX) = N'@Skip INT, @Take INT, @SearchFilter NVARCHAR(252)';
    EXEC sp_executesql @Sql, @ParamDef, @Skip = @Skip, @Take = @Take, @SearchFilter = '%' + @SearchText + '%';
END
GO
