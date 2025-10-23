-- =============================================
-- Deploy New Stored Procedures
-- Date: 2025-01-23
-- Description: Deploy stored procedures created during CQRS refactoring
-- =============================================

USE [YOUR_DATABASE_NAME]; -- CHANGE THIS TO YOUR DATABASE NAME
GO

PRINT '=================================================';
PRINT 'Deploying New Stored Procedures...';
PRINT '=================================================';
GO

-- =============================================
-- 1. SP_GetLogs
-- =============================================
PRINT '';
PRINT 'Creating/Updating SP_GetLogs...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetLogs]
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @Level NVARCHAR(50) = NULL,
    @SearchTerm NVARCHAR(500) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure valid page number and size
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 50;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM Logs
    WHERE 
        (@Level IS NULL OR Level = @Level)
        AND (@SearchTerm IS NULL OR Message LIKE '%' + @SearchTerm + '%')
        AND (@StartDate IS NULL OR TimeStamp >= @StartDate)
        AND (@EndDate IS NULL OR TimeStamp <= @EndDate);

    -- Get paginated logs
    SELECT 
        Id,
        TimeStamp,
        ISNULL(Level, 'Information') as Level,
        ISNULL(Message, 'No message') as Message,
        ISNULL(Exception, '') as Exception,
        ISNULL(Properties, '') as Properties,
        ISNULL(UserId, '') as UserId,
        ISNULL(RequestId, '') as RequestId,
        ISNULL(ActionName, '') as ActionName
    FROM Logs
    WHERE 
        (@Level IS NULL OR Level = @Level)
        AND (@SearchTerm IS NULL OR Message LIKE '%' + @SearchTerm + '%')
        AND (@StartDate IS NULL OR TimeStamp >= @StartDate)
        AND (@EndDate IS NULL OR TimeStamp <= @EndDate)
    ORDER BY TimeStamp DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT 'SP_GetLogs created/updated successfully.';
GO

-- =============================================
-- 2. SP_GetUserRolesByUserId
-- =============================================
PRINT '';
PRINT 'Creating/Updating SP_GetUserRolesByUserId...';
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserRolesByUserId]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get username and company ID from USER_MASTER
    DECLARE @UserName NVARCHAR(50);
    DECLARE @CompanyId NVARCHAR(50);

    SELECT
        @UserName = UM_USERNAME,
        @CompanyId = CAST(UM_CM_ID AS NVARCHAR(50))
    FROM USER_MASTER
    WHERE UM_CODE = @UserId;

    -- If user info is found, call SP_GetUserRoles
    IF @UserName IS NOT NULL AND @CompanyId IS NOT NULL
    BEGIN
        EXEC SP_GetUserRoles @UserName, @CompanyId;
    END
    ELSE
    BEGIN
        -- Return an empty result set if user not found
        SELECT CAST(NULL AS NVARCHAR(50)) AS RoleName WHERE 1=0;
    END
END
GO

PRINT 'SP_GetUserRolesByUserId created/updated successfully.';
GO

-- =============================================
-- Summary
-- =============================================
PRINT '';
PRINT '=================================================';
PRINT 'Deployment Complete!';
PRINT '=================================================';
PRINT '';
PRINT 'Stored Procedures Created/Updated:';
PRINT '  1. SP_GetLogs';
PRINT '  2. SP_GetUserRolesByUserId';
PRINT '';
PRINT 'Please test the stored procedures before running tests.';
PRINT '=================================================';
GO

