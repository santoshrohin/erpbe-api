-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Get SO Type Masters with pagination, filtering, and sorting
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_GetSoTypeMasters...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetSoTypeMasters]
    @PageNumber INT = 1,
    @PageSize INT = 15,
    @SortBy NVARCHAR(50) = 'SO_T_SHORT_NAME',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(200) = NULL,
    @CompanyId INT = NULL,
    @ShortName NVARCHAR(50) = NULL,
    @Description NVARCHAR(50) = NULL,
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
    FROM SO_TYPE_MASTER
    WHERE ES_DELETE = 0
        AND (@CompanyId IS NULL OR SO_T_COMP_ID = @CompanyId)
        AND (@ShortName IS NULL OR SO_T_SHORT_NAME LIKE '%' + @ShortName + '%')
        AND (@Description IS NULL OR SO_T_DESC LIKE '%' + @Description + '%')
        AND (@SearchTerm IS NULL OR 
             SO_T_SHORT_NAME LIKE '%' + @SearchTerm + '%' OR 
             SO_T_DESC LIKE '%' + @SearchTerm + '%' OR
             SO_T_FIRST_LETTER LIKE '%' + @SearchTerm + '%');
    
    -- Get paginated results with dynamic sorting
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = '
    SELECT 
        SO_T_CODE AS Id,
        SO_T_COMP_ID AS CompanyId,
        SO_T_SHORT_NAME AS ShortName,
        SO_T_DESC AS Description,
        SO_T_FIRST_LETTER AS FirstLetter,
        ES_DELETE AS IsDeleted,
        MODIFY AS IsModified
    FROM SO_TYPE_MASTER
    WHERE ES_DELETE = 0
        AND (@CompanyId IS NULL OR SO_T_COMP_ID = @CompanyId)
        AND (@ShortName IS NULL OR SO_T_SHORT_NAME LIKE ''%'' + @ShortName + ''%'')
        AND (@Description IS NULL OR SO_T_DESC LIKE ''%'' + @Description + ''%'')
        AND (@SearchTerm IS NULL OR 
             SO_T_SHORT_NAME LIKE ''%'' + @SearchTerm + ''%'' OR 
             SO_T_DESC LIKE ''%'' + @SearchTerm + ''%'' OR
             SO_T_FIRST_LETTER LIKE ''%'' + @SearchTerm + ''%'')
    ORDER BY ' + QUOTENAME(@SortBy) + ' ' + @SortDirection + '
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY';
    
    EXEC sp_executesql @SQL,
        N'@CompanyId INT, @ShortName NVARCHAR(50), @Description NVARCHAR(50), @SearchTerm NVARCHAR(200), @Offset INT, @PageSize INT',
        @CompanyId, @ShortName, @Description, @SearchTerm, @Offset, @PageSize;
END
GO

PRINT 'ERP_GetSoTypeMasters created successfully!';
GO

