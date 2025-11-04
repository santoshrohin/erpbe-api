IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetActiveCompanies]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ERP_GetActiveCompanies]
GO

CREATE PROCEDURE [dbo].[ERP_GetActiveCompanies]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Query matches legacy: select distinct CM_ID,CM_NAME from COMPANY_MASTER where CM_ACTIVE_IND=1
    SELECT DISTINCT 
        CM_ID AS Id,
        CM_NAME AS DisplayName
    FROM COMPANY_MASTER 
    WHERE CM_ACTIVE_IND = 1 
    ORDER BY CM_NAME;
END
GO

