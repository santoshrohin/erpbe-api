CREATE OR ALTER PROCEDURE [dbo].[ERP_GetAllProductionToStore]
    @CompanyCode  INT,
    @SearchText   NVARCHAR(100) = NULL,
    @DateFrom     SMALLDATETIME = NULL,
    @DateTo       SMALLDATETIME = NULL,
    @PageNumber   INT           = 1,
    @PageSize     INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        PS_CODE         AS ProductionCode,
        PS_GIN_NO       AS GinNumber,
        PS_GIN_DATE     AS GinDate,
        PS_TYPE         AS Type,
        PS_PERSON_NAME  AS PersonName,
        PS_MR_CODE      AS MrCode,
        PS_P_CODE       AS CustomerCode,
        PS_BATCH_NO     AS BatchNo,
        PS_CM_COMP_CODE AS CompanyCode,
        MODIFY          AS IsLocked,
        ES_DELETE       AS IsDeleted
    FROM PRODUCTION_TO_STORE_MASTER
    WHERE ES_DELETE       = 0
      AND PS_CM_COMP_CODE = @CompanyCode
      AND (@DateFrom      IS NULL OR PS_GIN_DATE >= @DateFrom)
      AND (@DateTo        IS NULL OR PS_GIN_DATE <= @DateTo)
      AND (@SearchText    IS NULL
           OR CAST(PS_GIN_NO AS NVARCHAR(50)) LIKE '%' + @SearchText + '%'
           OR PS_PERSON_NAME                  LIKE '%' + @SearchText + '%')
    ORDER BY PS_GIN_DATE DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*)
    FROM PRODUCTION_TO_STORE_MASTER
    WHERE ES_DELETE       = 0
      AND PS_CM_COMP_CODE = @CompanyCode
      AND (@DateFrom      IS NULL OR PS_GIN_DATE >= @DateFrom)
      AND (@DateTo        IS NULL OR PS_GIN_DATE <= @DateTo)
      AND (@SearchText    IS NULL
           OR CAST(PS_GIN_NO AS NVARCHAR(50)) LIKE '%' + @SearchText + '%'
           OR PS_PERSON_NAME                  LIKE '%' + @SearchText + '%');
END
GO
