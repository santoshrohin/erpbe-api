IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetFinancialYearsByCompanyId]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ERP_GetFinancialYearsByCompanyId]
GO

CREATE PROCEDURE [dbo].[ERP_GetFinancialYearsByCompanyId]
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Query matches legacy: select distinct CM_CODE, 'From  ' +convert(varchar(10),CM_OPENING_DATE,103)+' To '+ convert(varchar(10),CM_CLOSING_DATE,103)as FINANCIAL from COMPANY_MASTER where CM_ID=<companyId> order by CM_CODE desc
    SELECT DISTINCT 
        CM_CODE AS Id,
        'From ' + CONVERT(VARCHAR(10), CM_OPENING_DATE, 103) + ' To ' + CONVERT(VARCHAR(10), CM_CLOSING_DATE, 103) AS DisplayName,
        CM_CODE AS FinancialYearCode,
        CONVERT(VARCHAR(10), CM_OPENING_DATE, 103) AS OpeningDate,
        CONVERT(VARCHAR(10), CM_CLOSING_DATE, 103) AS ClosingDate
    FROM COMPANY_MASTER 
    WHERE CM_ID = @CompanyId 
    ORDER BY CM_CODE DESC;
END
GO

