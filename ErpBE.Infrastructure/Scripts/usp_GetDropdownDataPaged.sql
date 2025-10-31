IF OBJECT_ID(''dbo.usp_GetDropdownDataPaged'', ''P'') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetDropdownDataPaged;
GO

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
    DECLARE @Sql NVARCHAR(MAX) = '';
    DECLARE @Where NVARCHAR(MAX) = '';
    DECLARE @Order NVARCHAR(MAX) = '';
    DECLARE @SearchFilter NVARCHAR(252);

    IF @WhereClause IS NOT NULL AND LTRIM(RTRIM(@WhereClause)) <> ''
        SET @Where = '' WHERE '' + @WhereClause;

    IF @SearchText IS NOT NULL AND LTRIM(RTRIM(@SearchText)) <> ''
    BEGIN
        SET @SearchFilter = ''%'' + @SearchText + ''%'';
        IF LEN(@Where) > 0
            SET @Where = @Where + '' AND (['' + @DisplayColumn + ''] LIKE @SearchFilter OR CONVERT(VARCHAR(50),['' + @IdColumn + '']) LIKE @SearchFilter)'';
        ELSE
            SET @Where = '' WHERE (['' + @DisplayColumn + ''] LIKE @SearchFilter OR CONVERT(VARCHAR(50),['' + @IdColumn + '']) LIKE @SearchFilter)'';
    END
    ELSE
        SET @SearchFilter = NULL;

    IF @OrderBy IS NOT NULL AND LEN(@OrderBy) > 0
        SET @Order = '' ORDER BY '' + @OrderBy + '' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY'';
    ELSE
        SET @Order = '' ORDER BY ['' + @DisplayColumn + ''] ASC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY'';

    SET @Sql = ''SELECT ['' + @IdColumn + ''] AS Id, ['' + @DisplayColumn + ''] AS DisplayName FROM ['' + @Table + '']''
        + @Where + @Order + '';''
        + '' SELECT COUNT(1) AS TotalCount FROM ['' + @Table + '']'' + @Where + '';'';

    EXEC sp_executesql
        @Sql,
        N''@Skip INT, @Take INT, @SearchFilter NVARCHAR(252)'',
        @Skip = @Skip,
        @Take = @Take,
        @SearchFilter = @SearchFilter;
END
GO
