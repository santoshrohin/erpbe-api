-- =============================================
-- Get All Customer Masters (Paged, Filtered, Sorted)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetCustomerMasters]
    @CompanyId INT,
    @IsActive BIT = NULL,
    @AreaCode INT = NULL,
    @CustomerType VARCHAR(20) = NULL,
    @StateCode INT = NULL,
    @CityCode INT = NULL,
    @CategoryCode INT = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @SortBy NVARCHAR(50) = 'PartyName',
    @SortDescending BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Build WHERE clause dynamically
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = N'
        SELECT 
            PM.P_CODE AS Id,
            PM.P_CM_COMP_ID AS CompanyId,
            PM.P_PARTY_CODE AS PartyCode,
            PM.P_NAME AS PartyName,
            PM.P_CONTACT AS ContactPerson,
            PM.P_ABBREVATION AS Abbreviation,
            PM.P_VEND_CODE AS VendorCode,
            PM.P_ADD1 AS Address,
            PM.P_PHONE AS Phone,
            PM.P_MOB AS Mobile,
            PM.P_EMAIL AS Email,
            PM.P_FAX AS FaxNo,
            PM.P_PIN_CODE AS PinCode,
            PM.P_A_CODE AS AreaCode,
            AM.A_DESC AS AreaName,
            PM.P_CUST_TYPE AS CustomerType,
            CTM.CTM_TYPE_DESC AS CustomerTypeName,
            PM.P_COUNTRY_CODE AS CountryCode,
            PM.P_SM_CODE AS StateCode,
            PM.P_CITY_CODE AS CityCode,
            PM.P_CATEGORY AS CategoryCode,
            PM.P_E_CODE AS EmployeeCode,
            PM.P_PAN AS PanNo,
            PM.P_CST AS CstNo,
            PM.P_VAT AS VatNo,
            PM.P_SER_TAX_NO AS ServiceTaxNo,
            PM.P_ECC_NO AS EccNo,
            PM.P_LBT_NO AS LbtNo,
            PM.P_EXC_RANGE AS ExciseRange,
            PM.P_EXC_DIV AS ExciseDivision,
            PM.P_EXC_COLLECTORATE AS ExciseCollectorate,
            PM.P_TALLY AS TallyName,
            PM.P_CREDITDAYS AS CreditDays,
            PM.P_TDS AS TdsPercentage,
            PM.P_ACTIVE_IND AS IsActive,
            PM.P_LBT_IND AS IsLbtApplicable
        FROM PARTY_MASTER PM
        LEFT JOIN AREA_MASTER AM ON PM.P_A_CODE = AM.A_CODE AND PM.P_CM_COMP_ID = AM.A_CM_COMP_ID
        LEFT JOIN CUSTOMER_TYPE_MASTER CTM ON PM.P_CUST_TYPE = CTM.CTM_TYPE_CODE AND PM.P_CM_COMP_ID = CTM.CTM_CM_COMP_ID
        WHERE PM.P_CM_COMP_ID = @CompanyId
          AND PM.P_TYPE = 1';
    
    -- Apply filters
    IF @IsActive IS NOT NULL
        SET @SQL = @SQL + N' AND PM.P_ACTIVE_IND = @IsActive';
        
    IF @AreaCode IS NOT NULL
        SET @SQL = @SQL + N' AND PM.P_A_CODE = @AreaCode';
        
    IF @CustomerType IS NOT NULL
        SET @SQL = @SQL + N' AND PM.P_CUST_TYPE = @CustomerType';
        
    IF @StateCode IS NOT NULL
        SET @SQL = @SQL + N' AND PM.P_SM_CODE = @StateCode';
        
    IF @CityCode IS NOT NULL
        SET @SQL = @SQL + N' AND PM.P_CITY_CODE = @CityCode';
        
    IF @CategoryCode IS NOT NULL
        SET @SQL = @SQL + N' AND PM.P_CATEGORY = @CategoryCode';
    
    -- Apply search
    IF @SearchTerm IS NOT NULL AND @SearchTerm <> ''
        SET @SQL = @SQL + N' AND (PM.P_NAME LIKE ''%'' + @SearchTerm + ''%'' 
                                OR PM.P_CONTACT LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_EMAIL LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_PHONE LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_MOB LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_ABBREVATION LIKE ''%'' + @SearchTerm + ''%'')';
    
    -- Get total count
    DECLARE @CountSQL NVARCHAR(MAX);
    SET @CountSQL = N'SELECT @TotalRecords = COUNT(*) FROM PARTY_MASTER PM 
        WHERE PM.P_CM_COMP_ID = @CompanyId AND PM.P_TYPE = 1';
        
    IF @IsActive IS NOT NULL
        SET @CountSQL = @CountSQL + N' AND PM.P_ACTIVE_IND = @IsActive';
    IF @AreaCode IS NOT NULL
        SET @CountSQL = @CountSQL + N' AND PM.P_A_CODE = @AreaCode';
    IF @CustomerType IS NOT NULL
        SET @CountSQL = @CountSQL + N' AND PM.P_CUST_TYPE = @CustomerType';
    IF @StateCode IS NOT NULL
        SET @CountSQL = @CountSQL + N' AND PM.P_SM_CODE = @StateCode';
    IF @CityCode IS NOT NULL
        SET @CountSQL = @CountSQL + N' AND PM.P_CITY_CODE = @CityCode';
    IF @CategoryCode IS NOT NULL
        SET @CountSQL = @CountSQL + N' AND PM.P_CATEGORY = @CategoryCode';
    IF @SearchTerm IS NOT NULL AND @SearchTerm <> ''
        SET @CountSQL = @CountSQL + N' AND (PM.P_NAME LIKE ''%'' + @SearchTerm + ''%'' 
                                OR PM.P_CONTACT LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_EMAIL LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_PHONE LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_MOB LIKE ''%'' + @SearchTerm + ''%''
                                OR PM.P_ABBREVATION LIKE ''%'' + @SearchTerm + ''%'')';
    
    EXEC sp_executesql @CountSQL, 
        N'@CompanyId INT, @IsActive BIT, @AreaCode INT, @CustomerType VARCHAR(20), @StateCode INT, @CityCode INT, @CategoryCode INT, @SearchTerm NVARCHAR(255), @TotalRecords INT OUTPUT',
        @CompanyId, @IsActive, @AreaCode, @CustomerType, @StateCode, @CityCode, @CategoryCode, @SearchTerm, @TotalRecords OUTPUT;
    
    -- Apply sorting
    SET @SQL = @SQL + N' ORDER BY ';
    
    IF @SortBy = 'PartyCode'
        SET @SQL = @SQL + N'PM.P_PARTY_CODE';
    ELSE IF @SortBy = 'AreaName'
        SET @SQL = @SQL + N'AM.A_DESC';
    ELSE IF @SortBy = 'CustomerTypeName'
        SET @SQL = @SQL + N'CTM.CTM_TYPE_DESC';
    ELSE
        SET @SQL = @SQL + N'PM.P_NAME';
    
    IF @SortDescending = 1
        SET @SQL = @SQL + N' DESC';
    ELSE
        SET @SQL = @SQL + N' ASC';
    
    -- Apply pagination
    SET @SQL = @SQL + N' OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY';
    
    -- Execute
    EXEC sp_executesql @SQL,
        N'@CompanyId INT, @IsActive BIT, @AreaCode INT, @CustomerType VARCHAR(20), @StateCode INT, @CityCode INT, @CategoryCode INT, @SearchTerm NVARCHAR(255), @Offset INT, @PageSize INT',
        @CompanyId, @IsActive, @AreaCode, @CustomerType, @StateCode, @CityCode, @CategoryCode, @SearchTerm, @Offset, @PageSize;
END

