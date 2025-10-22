-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-19
-- Description: Create Unit Master stored procedures with server-side pagination, filtering, and searching
-- Database: db_a2ea4b_farmerplacement
-- =============================================

USE [db_a2ea4b_sunv2]
GO

-- =============================================
-- Unit Master Stored Procedures
-- =============================================

-- SP_CreateUnitMaster
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CreateUnitMaster')
    DROP PROCEDURE [dbo].[SP_CreateUnitMaster]
GO

CREATE PROCEDURE [dbo].[SP_CreateUnitMaster]
    @UnitName VARCHAR(10),
    @UnitDescription VARCHAR(100) = NULL,
    @CompanyId INT,
    @IsActive BIT = 1,
    @UnitId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if unit name already exists for this company
    IF EXISTS (SELECT 1 FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = @UnitName AND I_UOM_CM_COMP_ID = @CompanyId AND ES_DELETE = 0)
    BEGIN
        RAISERROR('Unit with this name already exists for this company', 16, 1);
        RETURN;
    END
    
    -- Insert new unit
    INSERT INTO ITEM_UNIT_MASTER (
        I_UOM_CM_COMP_ID,
        I_UOM_NAME,
        I_UOM_DESC,
        ES_DELETE,
        MODIFY
    )
    VALUES (
        @CompanyId,
        @UnitName,
        @UnitDescription,
        0, -- ES_DELETE
        0  -- MODIFY
    );
    
    SET @UnitId = SCOPE_IDENTITY();
    SELECT @UnitId AS UnitId;
END
GO

-- SP_UpdateUnitMaster
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_UpdateUnitMaster')
    DROP PROCEDURE [dbo].[SP_UpdateUnitMaster]
GO

CREATE PROCEDURE [dbo].[SP_UpdateUnitMaster]
    @UnitId INT,
    @UnitName VARCHAR(10),
    @UnitDescription VARCHAR(100) = NULL,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if unit exists
    IF NOT EXISTS (SELECT 1 FROM ITEM_UNIT_MASTER WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0)
    BEGIN
        RAISERROR('Unit not found', 16, 1);
        RETURN;
    END
    
    -- Check if unit name already exists for another unit in the same company
    IF EXISTS (
        SELECT 1 FROM ITEM_UNIT_MASTER 
        WHERE I_UOM_NAME = @UnitName 
        AND I_UOM_CM_COMP_ID = (SELECT I_UOM_CM_COMP_ID FROM ITEM_UNIT_MASTER WHERE I_UOM_CODE = @UnitId)
        AND I_UOM_CODE != @UnitId 
        AND ES_DELETE = 0
    )
    BEGIN
        RAISERROR('Unit with this name already exists for this company', 16, 1);
        RETURN;
    END
    
    -- Update unit
    UPDATE ITEM_UNIT_MASTER
    SET
        I_UOM_NAME = @UnitName,
        I_UOM_DESC = @UnitDescription,
        MODIFY = 1
    WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0;
    
    SELECT @@ROWCOUNT;
END
GO

-- SP_DeleteUnitMaster (Soft Delete)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_DeleteUnitMaster')
    DROP PROCEDURE [dbo].[SP_DeleteUnitMaster]
GO

CREATE PROCEDURE [dbo].[SP_DeleteUnitMaster]
    @UnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Soft delete unit
    UPDATE ITEM_UNIT_MASTER
    SET
        ES_DELETE = 1,
        MODIFY = 1
    WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0;
    
    SELECT @@ROWCOUNT;
END
GO

-- SP_GetUnitMasterById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_GetUnitMasterById')
    DROP PROCEDURE [dbo].[SP_GetUnitMasterById]
GO

CREATE PROCEDURE [dbo].[SP_GetUnitMasterById]
    @UnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        I_UOM_CODE AS Id,
        I_UOM_CM_COMP_ID AS CompanyId,
        I_UOM_NAME AS UnitName,
        I_UOM_DESC AS UnitDescription,
        CASE WHEN ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
        ES_DELETE AS IsDeleted,
        MODIFY AS IsModified,
        GETDATE() AS CreatedDate,
        NULL AS ModifiedDate,
        'System' AS CreatedBy,
        NULL AS ModifiedBy
    FROM ITEM_UNIT_MASTER
    WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0;
END
GO

-- SP_GetUnitMasterByName
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
        I_UOM_CODE AS Id,
        I_UOM_CM_COMP_ID AS CompanyId,
        I_UOM_NAME AS UnitName,
        I_UOM_DESC AS UnitDescription,
        CASE WHEN ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
        ES_DELETE AS IsDeleted,
        MODIFY AS IsModified,
        GETDATE() AS CreatedDate,
        NULL AS ModifiedDate,
        'System' AS CreatedBy,
        NULL AS ModifiedBy
    FROM ITEM_UNIT_MASTER
    WHERE I_UOM_NAME = @UnitName 
    AND I_UOM_CM_COMP_ID = @CompanyId 
    AND ES_DELETE = 0;
END
GO

-- SP_GetUnitMasters (With Server-Side Pagination, Filtering, and Searching)
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

    -- Get Paginated Data with Server-Side Sorting
    SET @Sql = '
        SELECT
            UOM.I_UOM_CODE AS Id,
            UOM.I_UOM_CM_COMP_ID AS CompanyId,
            UOM.I_UOM_NAME AS UnitName,
            UOM.I_UOM_DESC AS UnitDescription,
            CASE WHEN UOM.ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
            UOM.ES_DELETE AS IsDeleted,
            UOM.MODIFY AS IsModified,
            GETDATE() AS CreatedDate,
            NULL AS ModifiedDate,
            ''System'' AS CreatedBy,
            NULL AS ModifiedBy
        FROM ITEM_UNIT_MASTER UOM
        ' + @WhereClause + '
        ORDER BY ' + QUOTENAME(@SortBy) + ' ' + @SortDirection + '
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    ';

    EXEC sp_executesql @Sql,
                       N'@Offset INT, @PageSize INT, @SearchTerm NVARCHAR(255), @CompanyId INT, @IsActive BIT, @UnitName VARCHAR(10)',
                       @Offset, @PageSize, @SearchTerm, @CompanyId, @IsActive, @UnitName;
END
GO

-- SP_IsUnitNameUnique
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_IsUnitNameUnique')
    DROP PROCEDURE [dbo].[SP_IsUnitNameUnique]
GO

CREATE PROCEDURE [dbo].[SP_IsUnitNameUnique]
    @UnitName VARCHAR(10),
    @CompanyId INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM ITEM_UNIT_MASTER
    WHERE I_UOM_NAME = @UnitName
    AND I_UOM_CM_COMP_ID = @CompanyId
    AND ES_DELETE = 0
    AND (@ExcludeId IS NULL OR I_UOM_CODE != @ExcludeId);
END
GO

-- SP_SetUnitMasterActiveStatus
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_SetUnitMasterActiveStatus')
    DROP PROCEDURE [dbo].[SP_SetUnitMasterActiveStatus]
GO

CREATE PROCEDURE [dbo].[SP_SetUnitMasterActiveStatus]
    @UnitId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ITEM_UNIT_MASTER
    SET
        ES_DELETE = CASE WHEN @IsActive = 1 THEN 0 ELSE 1 END,
        MODIFY = 1
    WHERE I_UOM_CODE = @UnitId;
    
    SELECT @@ROWCOUNT;
END
GO

PRINT 'Unit Master stored procedures created successfully!'
PRINT 'Created procedures: SP_CreateUnitMaster, SP_UpdateUnitMaster, SP_DeleteUnitMaster, SP_GetUnitMasterById, SP_GetUnitMasterByName'
PRINT 'Created procedures: SP_GetUnitMasters (with server-side pagination, filtering, searching), SP_IsUnitNameUnique, SP_SetUnitMasterActiveStatus'
