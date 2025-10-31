IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ERP_GetAllCustomerPos')
    DROP PROCEDURE [dbo].[ERP_GetAllCustomerPos]
GO

CREATE PROCEDURE [dbo].[ERP_GetAllCustomerPos]
    @CompanyId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @IsActive BIT = 1,
    @SearchTerm NVARCHAR(100) = NULL,
    @PoNumber VARCHAR(100) = NULL,
    @CustomerCode INT = NULL,
    @CustomerName NVARCHAR(100) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @WorkOrderNumber VARCHAR(50) = NULL,
    @CustomerItemCode NVARCHAR(100) = NULL,
    @PoType INT = NULL,
    @ProjectCode INT = NULL,
    @InvoiceGenerated BIT = NULL,
    @HasAmendment BIT = NULL,
    @IsVerbalOrder BIT = NULL,
    @SortBy NVARCHAR(50) = 'PoCode',
    @SortOrder NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

        -- Build WHERE clause conditions
        DECLARE @SQL NVARCHAR(MAX);
        DECLARE @CountSQL NVARCHAR(MAX);
        DECLARE @WHERE NVARCHAR(MAX) = ' WHERE m.CPOM_CM_COMP_ID = @CompanyId ';

        IF @IsActive = 1
            SET @WHERE = @WHERE + ' AND m.ES_DELETE = 0 ';
        ELSE
            SET @WHERE = @WHERE + ' AND m.ES_DELETE = 1 ';

        IF @SearchTerm IS NOT NULL AND @SearchTerm != ''
            SET @WHERE = @WHERE + ' AND (m.CPOM_PONO LIKE ''%'' + @SearchTerm + ''%'' 
                OR p.P_NAME LIKE ''%'' + @SearchTerm + ''%'' 
                OR m.CPOM_WORK_ODR_NO LIKE ''%'' + @SearchTerm + ''%'') ';

        IF @PoNumber IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_PONO LIKE ''%'' + @PoNumber + ''%'' ';

        IF @CustomerCode IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_P_CODE = @CustomerCode ';

        IF @CustomerName IS NOT NULL
            SET @WHERE = @WHERE + ' AND p.P_NAME LIKE ''%'' + @CustomerName + ''%'' ';

        IF @FromDate IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_DATE >= @FromDate ';

        IF @ToDate IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_DATE <= @ToDate ';

        IF @WorkOrderNumber IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_WORK_ODR_NO LIKE ''%'' + @WorkOrderNumber + ''%'' ';

        IF @CustomerItemCode IS NOT NULL
            SET @WHERE = @WHERE + ' AND EXISTS (SELECT 1 FROM CUSTPO_DETAIL d WHERE d.CPOD_CPOM_CODE = m.CPOM_CODE AND d.CPOD_CUST_I_CODE LIKE ''%'' + @CustomerItemCode + ''%'') ';

        IF @PoType IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_TYPE = @PoType ';

        IF @ProjectCode IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_PROJECT_CODE = @ProjectCode ';

        IF @InvoiceGenerated IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_INV_FLAG = @InvoiceGenerated ';

        IF @HasAmendment IS NOT NULL
        BEGIN
            IF @HasAmendment = 1
                SET @WHERE = @WHERE + ' AND m.CPOM_AM_COUNT > 0 ';
            ELSE
                SET @WHERE = @WHERE + ' AND m.CPOM_AM_COUNT = 0 ';
        END

        IF @IsVerbalOrder IS NOT NULL
            SET @WHERE = @WHERE + ' AND m.CPOM_IS_VERBAL = @IsVerbalOrder ';

        -- Build ORDER BY clause
        DECLARE @ORDERBY NVARCHAR(200) = ' ORDER BY ';
        IF @SortBy = 'PoCode'
            SET @ORDERBY = @ORDERBY + 'm.CPOM_CODE ';
        ELSE IF @SortBy = 'PoNumber'
            SET @ORDERBY = @ORDERBY + 'm.CPOM_PONO ';
        ELSE IF @SortBy = 'PoDate'
            SET @ORDERBY = @ORDERBY + 'm.CPOM_DATE ';
        ELSE IF @SortBy = 'CustomerName'
            SET @ORDERBY = @ORDERBY + 'p.P_NAME ';
        ELSE IF @SortBy = 'GrandTotal'
            SET @ORDERBY = @ORDERBY + 'm.CPOM_GRAND_TOT ';
        ELSE IF @SortBy = 'WorkOrderNumber'
            SET @ORDERBY = @ORDERBY + 'm.CPOM_WORK_ODR_NO ';
        ELSE IF @SortBy = 'AmendmentCount'
            SET @ORDERBY = @ORDERBY + 'm.CPOM_AM_COUNT ';
        ELSE IF @SortBy = 'CustomerPartNo'
            -- Sort by first detail's customer item code using subquery
            SET @ORDERBY = @ORDERBY + '(SELECT TOP 1 d.CPOD_CUST_I_CODE FROM CUSTPO_DETAIL d WHERE d.CPOD_CPOM_CODE = m.CPOM_CODE ORDER BY d.CPOD_CPOM_CODE) ';
        ELSE
            SET @ORDERBY = @ORDERBY + 'm.CPOM_CODE ';

        SET @ORDERBY = @ORDERBY + @SortOrder;

        -- Get paginated data
        SET @SQL = N'
        SELECT 
            m.CPOM_CODE AS PoCode,
            m.CPOM_P_CODE AS CustomerCode,
            p.P_NAME AS CustomerName,
            m.CPOM_PONO AS PoNumber,
            m.CPOM_DOC_NO AS DocumentNumber,
            m.CPOM_TYPE AS PoType,
            m.CPOM_DATE AS PoDate,
            m.CPOM_CR_DAYS AS CreditDays,
            m.CPOM_CM_COMP_ID AS CompanyId,
            m.CPOM_WORK_ODR_NO AS WorkOrderNumber,
            m.MODIFY AS IsLocked,
            m.ES_DELETE AS IsDeleted,
            m.CPOM_GRAND_TOT AS GrandTotal,
            m.CPOM_INV_FLAG AS InvoiceGenerated,
            m.CPOM_AM_COUNT AS AmendmentCount,
            m.CPOM_IS_VERBAL AS IsVerbalOrder,
            m.CPOM_PROJECT_CODE AS ProjectCode,
            m.CPOM_PROJECT_NAME AS ProjectName
        FROM CUSTPO_MASTER m
        INNER JOIN PARTY_MASTER p ON m.CPOM_P_CODE = p.P_CODE ' + @WHERE + @ORDERBY + '
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;';

        -- Get total count
        SET @CountSQL = N'
        SELECT COUNT(*)
        FROM CUSTPO_MASTER m
        INNER JOIN PARTY_MASTER p ON m.CPOM_P_CODE = p.P_CODE ' + @WHERE;

        -- Execute data query
        EXEC sp_executesql @SQL, 
            N'@CompanyId INT, @Offset INT, @PageSize INT, @SearchTerm NVARCHAR(100), 
              @PoNumber VARCHAR(100), @CustomerCode INT, @CustomerName NVARCHAR(100), 
              @FromDate DATETIME, @ToDate DATETIME, @WorkOrderNumber VARCHAR(50), 
              @CustomerItemCode NVARCHAR(100), @PoType INT, @ProjectCode INT, 
              @InvoiceGenerated BIT, @IsVerbalOrder BIT',
            @CompanyId, @Offset, @PageSize, @SearchTerm, 
            @PoNumber, @CustomerCode, @CustomerName, 
            @FromDate, @ToDate, @WorkOrderNumber, 
            @CustomerItemCode, @PoType, @ProjectCode, 
            @InvoiceGenerated, @IsVerbalOrder;

        -- Execute count query
        EXEC sp_executesql @CountSQL, 
            N'@CompanyId INT, @SearchTerm NVARCHAR(100), 
              @PoNumber VARCHAR(100), @CustomerCode INT, @CustomerName NVARCHAR(100), 
              @FromDate DATETIME, @ToDate DATETIME, @WorkOrderNumber VARCHAR(50), 
              @CustomerItemCode NVARCHAR(100), @PoType INT, @ProjectCode INT, 
              @InvoiceGenerated BIT, @IsVerbalOrder BIT',
            @CompanyId, @SearchTerm, 
            @PoNumber, @CustomerCode, @CustomerName, 
            @FromDate, @ToDate, @WorkOrderNumber, 
            @CustomerItemCode, @PoType, @ProjectCode, 
            @InvoiceGenerated, @IsVerbalOrder;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

