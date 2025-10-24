-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Get SO Type Master by ID
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_GetSoTypeMasterById...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_GetSoTypeMasterById]
    @SO_T_CODE INT
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
    WHERE SO_T_CODE = @SO_T_CODE;
END
GO

PRINT 'ERP_GetSoTypeMasterById created successfully!';
GO

