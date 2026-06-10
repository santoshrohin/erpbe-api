CREATE OR ALTER PROCEDURE [dbo].[ERP_GetAllIssueMaster]
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
        IM_CODE     AS IssueCode,
        IM_NO       AS IssueNumber,
        IM_DATE     AS IssueDate,
        IM_TYPE     AS IssueType,
        IM_ISSUEBY  AS IssuedBy,
        IM_REQBY    AS RequestedBy,
        IM_COMP_ID  AS CompanyCode,
        ES_DELETE   AS IsDeleted
    FROM ISSUE_MASTER
    WHERE IM_COMP_ID  = @CompanyCode
      AND ES_DELETE   = 0
      AND (@DateFrom   IS NULL OR IM_DATE >= @DateFrom)
      AND (@DateTo     IS NULL OR IM_DATE <= @DateTo)
      AND (@SearchText IS NULL
           OR CAST(IM_NO AS NVARCHAR(50)) LIKE '%' + @SearchText + '%'
           OR IM_ISSUEBY                  LIKE '%' + @SearchText + '%')
    ORDER BY IM_DATE DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*)
    FROM ISSUE_MASTER
    WHERE IM_COMP_ID  = @CompanyCode
      AND ES_DELETE   = 0
      AND (@DateFrom   IS NULL OR IM_DATE >= @DateFrom)
      AND (@DateTo     IS NULL OR IM_DATE <= @DateTo)
      AND (@SearchText IS NULL
           OR CAST(IM_NO AS NVARCHAR(50)) LIKE '%' + @SearchText + '%'
           OR IM_ISSUEBY                  LIKE '%' + @SearchText + '%');
END
GO
