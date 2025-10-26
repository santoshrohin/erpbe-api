-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Update an existing SO Type Master record
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_UpdateSoTypeMaster...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_UpdateSoTypeMaster]
    @SO_T_CODE INT,
    @SO_T_SHORT_NAME VARCHAR(50),
    @SO_T_DESC VARCHAR(50),
    @SO_T_FIRST_LETTER VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE SO_TYPE_MASTER
    SET 
        SO_T_SHORT_NAME = @SO_T_SHORT_NAME,
        SO_T_DESC = @SO_T_DESC,
        SO_T_FIRST_LETTER = @SO_T_FIRST_LETTER
    WHERE SO_T_CODE = @SO_T_CODE;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

PRINT 'ERP_UpdateSoTypeMaster created successfully!';
GO

