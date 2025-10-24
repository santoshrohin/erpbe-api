-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Get SO Type Master by Short Name
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_GetSoTypeMasterByShortName...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetSoTypeMasterByShortName]
    @SO_T_SHORT_NAME VARCHAR(50),
    @SO_T_COMP_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SO_T_CODE AS Id,
        SO_T_COMP_ID AS CompanyId,
        SO_T_SHORT_NAME AS ShortName,
        SO_T_DESC AS Description,
        SO_T_FIRST_LETTER AS FirstLetter,
        ES_DELETE AS IsDeleted,
        MODIFY AS IsModified
    FROM SO_TYPE_MASTER
    WHERE LOWER(SO_T_SHORT_NAME) = LOWER(@SO_T_SHORT_NAME)
        AND SO_T_COMP_ID = @SO_T_COMP_ID
        AND ES_DELETE = 0;
END
GO

PRINT 'ERP_GetSoTypeMasterByShortName created successfully!';
GO

