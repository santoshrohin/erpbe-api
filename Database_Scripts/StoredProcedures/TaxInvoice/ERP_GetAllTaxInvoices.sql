-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-01-24
-- Description: Gets all Tax Invoices with filtering, searching, sorting, and pagination
-- =============================================
CREATE PROCEDURE [dbo].[ERP_GetAllTaxInvoices]
    @CompanyId INT,
    @CustomerId INT = NULL,
    @CustomerPoCode INT = NULL,
    @InvoiceDateFrom DATETIME = NULL,
    @InvoiceDateTo DATETIME = NULL,
    @InvoiceNumber VARCHAR(50) = NULL,
    @InvoiceType TINYINT = NULL,
    @IsDeleted BIT = NULL,
    @IsSupplementary BIT = NULL,
    @ExportFlag BIT = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @SortBy NVARCHAR(50) = 'InvoiceDate',
    @SortOrder NVARCHAR(10) = 'desc',
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Build dynamic WHERE clause
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CountSQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = 'WHERE I.INM_CM_CODE = @CompanyId';

    IF @CustomerId IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.INM_P_CODE = @CustomerId';

    IF @CustomerPoCode IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.INM_CPOM_CODE = @CustomerPoCode';

    IF @InvoiceDateFrom IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.INM_DATE >= @InvoiceDateFrom';

    IF @InvoiceDateTo IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.INM_DATE <= @InvoiceDateTo';

    IF @InvoiceNumber IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND CAST(I.INM_NO AS VARCHAR) LIKE ''%'' + @InvoiceNumber + ''%''';

    IF @InvoiceType IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.INM_INVOICE_TYPE = @InvoiceType';

    IF @IsDeleted IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.ES_DELETE = @IsDeleted';
    ELSE
        SET @WhereClause = @WhereClause + ' AND I.ES_DELETE = 0'; -- Default: exclude deleted

    IF @IsSupplementary IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.INM_SUPPLEMENTORY = @IsSupplementary';

    IF @ExportFlag IS NOT NULL
        SET @WhereClause = @WhereClause + ' AND I.INM_EXPORT_FLAG = @ExportFlag';

    IF @SearchTerm IS NOT NULL AND @SearchTerm != ''
        SET @WhereClause = @WhereClause + ' AND (CAST(I.INM_NO AS VARCHAR) LIKE ''%'' + @SearchTerm + ''%'' OR P.P_NAME LIKE ''%'' + @SearchTerm + ''%'')';

    -- Build ORDER BY clause
    DECLARE @OrderByClause NVARCHAR(200) = 'ORDER BY ';
    IF @SortBy = 'InvoiceDate'
        SET @OrderByClause = @OrderByClause + 'I.INM_DATE';
    ELSE IF @SortBy = 'InvoiceNumber'
        SET @OrderByClause = @OrderByClause + 'I.INM_NO';
    ELSE IF @SortBy = 'CustomerName'
        SET @OrderByClause = @OrderByClause + 'P.P_NAME';
    ELSE IF @SortBy = 'GrossAmount'
        SET @OrderByClause = @OrderByClause + 'I.INM_G_AMT';
    ELSE
        SET @OrderByClause = @OrderByClause + 'I.INM_DATE';

    IF UPPER(@SortOrder) = 'DESC'
        SET @OrderByClause = @OrderByClause + ' DESC';
    ELSE
        SET @OrderByClause = @OrderByClause + ' ASC';

    -- Get total count
    SET @CountSQL = '
        SELECT @TotalCount = COUNT(*)
        FROM INVOICE_MASTER I
        LEFT JOIN PARTY_MASTER P ON I.INM_P_CODE = P.P_CODE
        ' + @WhereClause;

    EXEC sp_executesql @CountSQL, 
        N'@CompanyId INT, @CustomerId INT, @CustomerPoCode INT, @InvoiceDateFrom DATETIME, 
          @InvoiceDateTo DATETIME, @InvoiceNumber VARCHAR(50), @InvoiceType TINYINT, 
          @IsDeleted BIT, @IsSupplementary BIT, @ExportFlag BIT, @SearchTerm NVARCHAR(200), @TotalCount INT OUTPUT',
        @CompanyId, @CustomerId, @CustomerPoCode, @InvoiceDateFrom, @InvoiceDateTo, 
        @InvoiceNumber, @InvoiceType, @IsDeleted, @IsSupplementary, @ExportFlag, @SearchTerm, @TotalCount OUTPUT;

    -- Get paged data
    SET @SQL = '
        SELECT 
            I.INM_CODE AS InvoiceCode,
            I.INM_CM_CODE AS CompanyCode,
            I.INM_NO AS InvoiceNumber,
            I.INM_DATE AS InvoiceDate,
            I.INM_INVOICE_TYPE AS InvoiceType,
            I.INM_TYPE AS Type,
            I.INM_P_CODE AS CustomerCode,
            P.P_NAME AS CustomerName,
            I.INM_CPOM_CODE AS CustomerPoCode,
            I.INM_NET_AMT AS NetAmount,
            I.INM_TAXABLE_AMT AS TaxableAmount,
            I.INM_G_AMT AS GrossAmount,
            I.INM_BEXCISE AS BasicExcisePercentage,
            I.INM_BE_AMT AS BasicExciseAmount,
            I.INM_EDUC_CESS AS EducationCessPercentage,
            I.INM_EDUC_AMT AS EducationCessAmount,
            I.INM_H_EDUC_CESS AS HigherEducationCessPercentage,
            I.INM_H_EDUC_AMT AS HigherEducationCessAmount,
            I.INM_SUPPLEMENTORY AS IsSupplementary,
            I.INM_EXPORT_FLAG AS ExportFlag,
            I.INM_VEH_NO AS VehicleNumber,
            I.INM_TRANSPORT AS TransportName,
            I.INM_REMARK AS Remarks,
            I.ES_DELETE AS IsDeleted,
            I.MODIFY AS IsModifyLocked
        FROM INVOICE_MASTER I
        LEFT JOIN PARTY_MASTER P ON I.INM_P_CODE = P.P_CODE
        ' + @WhereClause + '
        ' + @OrderByClause + '
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @SQL, 
        N'@CompanyId INT, @CustomerId INT, @CustomerPoCode INT, @InvoiceDateFrom DATETIME, 
          @InvoiceDateTo DATETIME, @InvoiceNumber VARCHAR(50), @InvoiceType TINYINT, 
          @IsDeleted BIT, @IsSupplementary BIT, @ExportFlag BIT, @SearchTerm NVARCHAR(200), 
          @Offset INT, @PageSize INT',
        @CompanyId, @CustomerId, @CustomerPoCode, @InvoiceDateFrom, @InvoiceDateTo, 
        @InvoiceNumber, @InvoiceType, @IsDeleted, @IsSupplementary, @ExportFlag, @SearchTerm, 
        @Offset, @PageSize;

END
GO

