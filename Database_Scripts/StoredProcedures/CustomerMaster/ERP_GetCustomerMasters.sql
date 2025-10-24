-- =============================================
-- Stored Procedure: ERP_GetCustomerMasters
-- Description: Retrieves a paginated, filtered, searched, and sorted list of Customer Master records
-- =============================================
CREATE OR ALTER PROCEDURE ERP_GetCustomerMasters
    @CompanyId INT,
    @IsActive BIT = NULL,
    @AreaCode INT = NULL,
    @CustomerType INT = NULL,
    @CountryCode INT = NULL,
    @StateCode INT = NULL,
    @CityCode INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL,
    @SortBy NVARCHAR(50) = 'PartyName',
    @SortDescending BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;

    -- Build the WHERE clause dynamically
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @WhereClauses NVARCHAR(MAX) = '';
    DECLARE @OrderByClause NVARCHAR(MAX) = '';
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Base WHERE clause
    SET @WhereClauses = 'WHERE pm.P_C_CODE = @CompanyId AND pm.P_TYPE = 1 AND pm.ES_DELETE = 0';

    -- Filter by IsActive
    IF @IsActive IS NOT NULL
        SET @WhereClauses = @WhereClauses + ' AND pm.ES_ACTIVE = @IsActive';

    -- Filter by AreaCode
    IF @AreaCode IS NOT NULL
        SET @WhereClauses = @WhereClauses + ' AND pm.P_A_CODE = @AreaCode';

    -- Filter by CustomerType
    IF @CustomerType IS NOT NULL
        SET @WhereClauses = @WhereClauses + ' AND pm.P_CUST_TYPE = @CustomerType';

    -- Filter by CountryCode
    IF @CountryCode IS NOT NULL
        SET @WhereClauses = @WhereClauses + ' AND pm.P_COUNTRY_CODE = @CountryCode';

    -- Filter by StateCode
    IF @StateCode IS NOT NULL
        SET @WhereClauses = @WhereClauses + ' AND pm.P_STATE_CODE = @StateCode';

    -- Filter by CityCode
    IF @CityCode IS NOT NULL
        SET @WhereClauses = @WhereClauses + ' AND pm.P_CITY_CODE = @CityCode';

    -- Search Term
    IF @SearchTerm IS NOT NULL AND @SearchTerm <> ''
        SET @WhereClauses = @WhereClauses + ' AND (pm.P_NAME LIKE ''%'' + @SearchTerm + ''%'' OR pm.P_PARTY_CODE LIKE ''%'' + @SearchTerm + ''%'' OR pm.P_CONTACT LIKE ''%'' + @SearchTerm + ''%'' OR pm.P_MOB LIKE ''%'' + @SearchTerm + ''%'' OR pm.P_EMAIL LIKE ''%'' + @SearchTerm + ''%'')';

    -- Sorting
    SET @OrderByClause = 'ORDER BY ' + 
        CASE 
            WHEN @SortBy = 'PartyName' THEN 'pm.P_NAME'
            WHEN @SortBy = 'PartyCode' THEN 'pm.P_PARTY_CODE'
            WHEN @SortBy = 'ContactPerson' THEN 'pm.P_CONTACT'
            WHEN @SortBy = 'AreaName' THEN 'am.A_NAME'
            WHEN @SortBy = 'CustomerTypeName' THEN 'ctm.CTM_TYPE_DESCRIPTION'
            WHEN @SortBy = 'CreatedDate' THEN 'pm.ES_CREATE_DATE'
            ELSE 'pm.P_NAME'
        END +
        CASE 
            WHEN @SortDescending = 1 THEN ' DESC'
            ELSE ' ASC'
        END;

    -- Get total count
    SET @SQL = 'SELECT @TotalCount = COUNT(*) FROM PARTY_MASTER pm ' + @WhereClauses;
    
    EXEC sp_executesql @SQL, 
        N'@CompanyId INT, @IsActive BIT, @AreaCode INT, @CustomerType INT, @CountryCode INT, @StateCode INT, @CityCode INT, @SearchTerm NVARCHAR(100), @TotalCount INT OUTPUT',
        @CompanyId, @IsActive, @AreaCode, @CustomerType, @CountryCode, @StateCode, @CityCode, @SearchTerm, @TotalCount OUTPUT;

    -- Get paginated data
    SET @SQL = '
    SELECT 
        pm.P_CODE AS Id,
        pm.P_C_CODE AS CompanyId,
        pm.P_PARTY_CODE AS PartyCode,
        pm.P_NAME AS PartyName,
        pm.P_CONTACT AS ContactPerson,
        pm.P_ABBREVATION AS Abbreviation,
        pm.P_ADDRESS AS Address,
        pm.P_PHONE AS Phone,
        pm.P_MOB AS Mobile,
        pm.P_EMAIL AS Email,
        pm.P_WEBSITE AS Website,
        pm.P_FAX_NO AS FaxNo,
        pm.P_A_CODE AS AreaCode,
        am.A_NAME AS AreaName,
        pm.P_CUST_TYPE AS CustomerType,
        ctm.CTM_TYPE_DESCRIPTION AS CustomerTypeName,
        pm.P_COUNTRY_CODE AS CountryCode,
        pm.P_STATE_CODE AS StateCode,
        pm.P_CITY_CODE AS CityCode,
        pm.P_PIN_CODE AS PinCode,
        pm.P_VAT_TIN_NO AS VatTinNo,
        pm.P_CST_NO AS CstNo,
        pm.P_GST_NO AS GstNo,
        pm.P_PAN AS PanNo,
        pm.P_SERVICE_TAX_NO AS ServiceTaxNo,
        pm.P_TALLY_NAME AS TallyName,
        pm.P_OP_BAL AS OpeningBalance,
        pm.P_OP_BAL_TYPE AS OpeningBalanceType,
        pm.P_CREDIT_LIMIT AS CreditLimit,
        pm.P_CREDIT_DAYS AS CreditDays,
        pm.P_BANK_NAME AS BankName,
        pm.P_BANK_AC_NO AS BankAccountNo,
        pm.P_BANK_BRANCH AS BankBranchName,
        pm.P_BANK_IFSC_CODE AS BankIfscCode,
        pm.P_LBT_APPLICABLE AS IsLbtApplicable,
        pm.P_SEZ_CUSTOMER AS IsSezCustomer,
        pm.P_COMPOSITE_DEALER AS IsCompositeDealer,
        pm.P_REMARK AS Remark,
        pm.ES_ACTIVE AS IsActive,
        pm.ES_CREATE_DATE AS CreatedDate,
        pm.ES_MODIFY_DATE AS ModifiedDate
    FROM PARTY_MASTER pm
    LEFT JOIN AREA_MASTER am ON pm.P_A_CODE = am.A_CODE
    LEFT JOIN CUSTOMER_TYPE_MASTER ctm ON pm.P_CUST_TYPE = ctm.CTM_CODE ' +
    @WhereClauses + ' ' +
    @OrderByClause + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @SQL, 
        N'@CompanyId INT, @IsActive BIT, @AreaCode INT, @CustomerType INT, @CountryCode INT, @StateCode INT, @CityCode INT, @SearchTerm NVARCHAR(100), @Offset INT, @PageSize INT',
        @CompanyId, @IsActive, @AreaCode, @CustomerType, @CountryCode, @StateCode, @CityCode, @SearchTerm, @Offset, @PageSize;
END
GO

