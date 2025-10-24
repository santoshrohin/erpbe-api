-- =============================================
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Soft delete an SO Type Master record
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT 'Creating ERP_DeleteSoTypeMaster...';
GO

CREATE OR ALTER PROCEDURE [dbo].[ERP_DeleteSoTypeMaster]
    @SO_T_CODE INT
AS
BEGIN
    SET NOCOUNT OFF;
    
    UPDATE SO_TYPE_MASTER
    SET ES_DELETE = 1
    WHERE SO_T_CODE = @SO_T_CODE;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

PRINT 'ERP_DeleteSoTypeMaster created successfully!';
GO

