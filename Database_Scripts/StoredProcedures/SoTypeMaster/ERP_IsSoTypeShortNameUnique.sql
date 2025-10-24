-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Check if SO Type Short Name is unique (case-insensitive)
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_IsSoTypeShortNameUnique...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_IsSoTypeShortNameUnique]
    @SO_T_SHORT_NAME VARCHAR(50),
    @SO_T_COMP_ID INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS DuplicateCount
    FROM SO_TYPE_MASTER
    WHERE LOWER(SO_T_SHORT_NAME) = LOWER(@SO_T_SHORT_NAME)
        AND SO_T_COMP_ID = @SO_T_COMP_ID
        AND ES_DELETE = 0
        AND (@ExcludeId IS NULL OR SO_T_CODE != @ExcludeId);
END
GO

PRINT 'ERP_IsSoTypeShortNameUnique created successfully!';
GO

