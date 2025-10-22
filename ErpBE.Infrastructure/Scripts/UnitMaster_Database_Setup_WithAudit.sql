-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-21
-- Description: Unit Master Stored Procedures with Audit Trail Support
-- =============================================

USE [db_a2ea4b_sunv2]
GO

-- SP_GetUnitMasterById (With Audit Trail)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUnitMasterById')
    DROP PROCEDURE [dbo].[SP_GetUnitMasterById]
GO

CREATE PROCEDURE [dbo].[SP_GetUnitMasterById]
    @UnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        UOM.I_UOM_CODE AS Id,
        UOM.I_UOM_CM_COMP_ID AS CompanyId,
        UOM.I_UOM_NAME AS UnitName,
        UOM.I_UOM_DESC AS UnitDescription,
        CASE WHEN UOM.ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
        UOM.ES_DELETE AS IsDeleted,
        UOM.MODIFY AS IsModified,
        -- Audit fields from audit trail (latest record)
        ISNULL(AUDIT.CREATED_DATE, GETDATE()) AS CreatedDate,
        AUDIT.MODIFIED_DATE AS ModifiedDate,
        ISNULL(AUDIT.CREATED_BY, 'System') AS CreatedBy,
        AUDIT.MODIFIED_BY AS ModifiedBy
    FROM ITEM_UNIT_MASTER UOM
    LEFT JOIN (
        SELECT 
            RECORD_ID,
            CREATED_DATE,
            MODIFIED_DATE,
            CREATED_BY,
            MODIFIED_BY,
            ROW_NUMBER() OVER (PARTITION BY RECORD_ID ORDER BY CREATED_DATE DESC) as rn
        FROM AUDIT_TRAIL 
        WHERE TABLE_NAME = 'ITEM_UNIT_MASTER'
    ) AUDIT ON UOM.I_UOM_CODE = AUDIT.RECORD_ID AND AUDIT.rn = 1
    WHERE UOM.I_UOM_CODE = @UnitId AND UOM.ES_DELETE = 0;
END
GO

-- SP_GetUnitMasterByName (With Audit Trail)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUnitMasterByName')
    DROP PROCEDURE [dbo].[SP_GetUnitMasterByName]
GO

CREATE PROCEDURE [dbo].[SP_GetUnitMasterByName]
    @UnitName VARCHAR(10),
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        UOM.I_UOM_CODE AS Id,
        UOM.I_UOM_CM_COMP_ID AS CompanyId,
        UOM.I_UOM_NAME AS UnitName,
        UOM.I_UOM_DESC AS UnitDescription,
        CASE WHEN UOM.ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
        UOM.ES_DELETE AS IsDeleted,
        UOM.MODIFY AS IsModified,
        -- Audit fields from audit trail (latest record)
        ISNULL(AUDIT.CREATED_DATE, GETDATE()) AS CreatedDate,
        AUDIT.MODIFIED_DATE AS ModifiedDate,
        ISNULL(AUDIT.CREATED_BY, 'System') AS CreatedBy,
        AUDIT.MODIFIED_BY AS ModifiedBy
    FROM ITEM_UNIT_MASTER UOM
    LEFT JOIN (
        SELECT 
            RECORD_ID,
            CREATED_DATE,
            MODIFIED_DATE,
            CREATED_BY,
            MODIFIED_BY,
            ROW_NUMBER() OVER (PARTITION BY RECORD_ID ORDER BY CREATED_DATE DESC) as rn
        FROM AUDIT_TRAIL 
        WHERE TABLE_NAME = 'ITEM_UNIT_MASTER'
    ) AUDIT ON UOM.I_UOM_CODE = AUDIT.RECORD_ID AND AUDIT.rn = 1
    WHERE UOM.I_UOM_NAME = @UnitName 
    AND UOM.I_UOM_CM_COMP_ID = @CompanyId 
    AND UOM.ES_DELETE = 0;
END
GO

-- SP_GetUnitMasters (With Audit Trail and Server-Side Pagination)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUnitMasters')
    DROP PROCEDURE [dbo].[SP_GetUnitMasters]
GO

CREATE PROCEDURE [dbo].[SP_GetUnitMasters]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'I_UOM_NAME',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(255) = NULL,
    @CompanyId INT = NULL,
    @IsActive BIT = NULL,
    @UnitName VARCHAR(10) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE UOM.ES_DELETE = 0 ';

    -- Build WHERE clause based on filters
    IF @SearchTerm IS NOT NULL AND @SearchTerm <> ''
    BEGIN
        SET @WhereClause += ' AND (UOM.I_UOM_NAME LIKE ''%'' + @SearchTerm + ''%'' OR UOM.I_UOM_DESC LIKE ''%'' + @SearchTerm + ''%'') ';
    END;

    IF @CompanyId IS NOT NULL
    BEGIN
        SET @WhereClause += ' AND UOM.I_UOM_CM_COMP_ID = @CompanyId ';
    END;

    IF @IsActive IS NOT NULL
    BEGIN
        IF @IsActive = 1
            SET @WhereClause += ' AND UOM.ES_DELETE = 0 ';
        ELSE
            SET @WhereClause += ' AND UOM.ES_DELETE = 1 ';
    END;

    IF @UnitName IS NOT NULL AND @UnitName <> ''
    BEGIN
        SET @WhereClause += ' AND UOM.I_UOM_NAME LIKE ''%'' + @UnitName + ''%'' ';
    END;

    -- Get Total Count
    SET @Sql = 'SELECT @TotalCount = COUNT(UOM.I_UOM_CODE) FROM ITEM_UNIT_MASTER UOM ' + @WhereClause;
    EXEC sp_executesql @Sql,
                         N'@SearchTerm NVARCHAR(255), @CompanyId INT, @IsActive BIT, @UnitName VARCHAR(10), @TotalCount INT OUTPUT',
                         @SearchTerm, @CompanyId, @IsActive, @UnitName, @TotalCount OUTPUT;

    -- Get Paginated Data with Server-Side Sorting and Audit Trail
    SET @Sql = '
        SELECT
            UOM.I_UOM_CODE AS Id,
            UOM.I_UOM_CM_COMP_ID AS CompanyId,
            UOM.I_UOM_NAME AS UnitName,
            UOM.I_UOM_DESC AS UnitDescription,
            CASE WHEN UOM.ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
            UOM.ES_DELETE AS IsDeleted,
            UOM.MODIFY AS IsModified,
            -- Audit fields from audit trail (latest record)
            ISNULL(AUDIT.CREATED_DATE, GETDATE()) AS CreatedDate,
            AUDIT.MODIFIED_DATE AS ModifiedDate,
            ISNULL(AUDIT.CREATED_BY, ''System'') AS CreatedBy,
            AUDIT.MODIFIED_BY AS ModifiedBy
        FROM ITEM_UNIT_MASTER UOM
        LEFT JOIN (
            SELECT 
                RECORD_ID,
                CREATED_DATE,
                MODIFIED_DATE,
                CREATED_BY,
                MODIFIED_BY,
                ROW_NUMBER() OVER (PARTITION BY RECORD_ID ORDER BY CREATED_DATE DESC) as rn
            FROM AUDIT_TRAIL 
            WHERE TABLE_NAME = ''ITEM_UNIT_MASTER''
        ) AUDIT ON UOM.I_UOM_CODE = AUDIT.RECORD_ID AND AUDIT.rn = 1
        ' + @WhereClause + '
        ORDER BY ' + QUOTENAME(@SortBy) + ' ' + @SortDirection + '
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    ';

    EXEC sp_executesql @Sql,
                         N'@Offset INT, @PageSize INT, @SearchTerm NVARCHAR(255), @CompanyId INT, @IsActive BIT, @UnitName VARCHAR(10)',
                         @Offset, @PageSize, @SearchTerm, @CompanyId, @IsActive, @UnitName;
END;
GO

PRINT 'Unit Master stored procedures with audit trail support created successfully!';
PRINT 'Updated: SP_GetUnitMasterById, SP_GetUnitMasterByName, SP_GetUnitMasters';
PRINT 'Features: LEFT JOIN with AUDIT_TRAIL, handles existing records without audit data';
