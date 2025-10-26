-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Create a new SO Type Master record
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_CreateSoTypeMaster...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateSoTypeMaster]
    @SO_T_COMP_ID INT,
    @SO_T_SHORT_NAME VARCHAR(50),
    @SO_T_DESC VARCHAR(50),
    @SO_T_FIRST_LETTER VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO SO_TYPE_MASTER (
        SO_T_COMP_ID,
        SO_T_SHORT_NAME,
        SO_T_DESC,
        SO_T_FIRST_LETTER,
        ES_DELETE,
        MODIFY
    )
    VALUES (
        @SO_T_COMP_ID,
        @SO_T_SHORT_NAME,
        @SO_T_DESC,
        @SO_T_FIRST_LETTER,
        0, -- ES_DELETE = false
        0  -- MODIFY = false
    );
    
    SELECT SCOPE_IDENTITY() AS Id;
END
GO

PRINT 'ERP_CreateSoTypeMaster created successfully!';
GO

