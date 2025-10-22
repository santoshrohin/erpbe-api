USE [db_a2ea4b_sunv2]
GO

-- Create usp_GetDropdownData stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_GetDropdownData')
    DROP PROCEDURE [dbo].[usp_GetDropdownData]
GO

CREATE PROCEDURE [dbo].[usp_GetDropdownData]
    @Table NVARCHAR(100),
    @IdColumn NVARCHAR(50),
    @DisplayColumn NVARCHAR(50),
    @WhereClause NVARCHAR(500) = NULL,
    @OrderBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Sql NVARCHAR(MAX);
    
    -- Build the SQL query
    SET @Sql = 'SELECT ' + @IdColumn + ' AS Id, ' + @DisplayColumn + ' AS DisplayName FROM ' + @Table;
    
    -- Add WHERE clause if provided
    IF @WhereClause IS NOT NULL AND @WhereClause <> ''
    BEGIN
        SET @Sql = @Sql + ' WHERE ' + @WhereClause;
    END;
    
    -- Add ORDER BY clause if provided
    IF @OrderBy IS NOT NULL AND @OrderBy <> ''
    BEGIN
        SET @Sql = @Sql + ' ORDER BY ' + @OrderBy;
    END;
    
    -- Execute the dynamic SQL
    EXEC sp_executesql @Sql;
END
GO

PRINT 'usp_GetDropdownData stored procedure created successfully!';
