CREATE OR ALTER PROCEDURE [dbo].[ERP_GetAllDeliveryChallans]
    @CompanyCode  INT,
    @CustomerCode INT           = NULL,
    @DateFrom     SMALLDATETIME = NULL,
    @DateTo       SMALLDATETIME = NULL,
    @SearchText   NVARCHAR(100) = NULL,
    @PageNumber   INT           = 1,
    @PageSize     INT           = 20,
    @SortBy       NVARCHAR(50)  = 'ChallanDate',
    @SortDir      NVARCHAR(4)   = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        DCM_CODE          AS ChallanCode,
        DCM_CM_CODE       AS CompanyCode,
        DCM_P_CODE        AS CustomerCode,
        P_NAME            AS CustomerName,
        DCM_TYPE          AS Type,
        DCM_NO            AS ChallanNumber,
        DCM_DATE          AS ChallanDate,
        DCM_INV_NO        AS InvoiceNumber,
        DCM_THROUGH       AS Through,
        DCM_VEH_NO        AS VehicleNumber,
        DCM_LR_NO         AS LrNumber,
        DCM_ORDER_NO      AS OrderNumber,
        DCM_ORDER_DATE                   AS OrderDate,
        DELIVERY_CHALLAN_MASTER.ES_DELETE AS IsDeleted,
        DELIVERY_CHALLAN_MASTER.MODIFY    AS IsModifyLocked,
        DCM_MAT_TYPE                     AS MaterialType,
        DCM_IS_RETURNABLE                AS IsReturnable
    FROM DELIVERY_CHALLAN_MASTER
    LEFT JOIN PARTY_MASTER ON DCM_P_CODE = P_CODE
    WHERE DELIVERY_CHALLAN_MASTER.ES_DELETE = 0
      AND DCM_CM_CODE   = @CompanyCode
      AND DCM_TYPE      = 'DLC'
      AND (@CustomerCode IS NULL OR DCM_P_CODE   = @CustomerCode)
      AND (@DateFrom    IS NULL OR DCM_DATE      >= @DateFrom)
      AND (@DateTo      IS NULL OR DCM_DATE      <= @DateTo)
      AND (@SearchText  IS NULL
           OR CAST(DCM_NO AS NVARCHAR(50)) LIKE '%' + @SearchText + '%'
           OR UPPER(P_NAME)                LIKE UPPER('%' + @SearchText + '%')
           OR CONVERT(VARCHAR, DCM_DATE, 106) LIKE '%' + @SearchText + '%')
    ORDER BY DCM_CODE DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*)
    FROM DELIVERY_CHALLAN_MASTER
    JOIN PARTY_MASTER ON DCM_P_CODE = P_CODE
    WHERE DELIVERY_CHALLAN_MASTER.ES_DELETE = 0
      AND DCM_CM_CODE = @CompanyCode
      AND DCM_TYPE    = 'DLC'
      AND (@CustomerCode IS NULL OR DCM_P_CODE  = @CustomerCode)
      AND (@DateFrom    IS NULL OR DCM_DATE     >= @DateFrom)
      AND (@DateTo      IS NULL OR DCM_DATE     <= @DateTo)
      AND (@SearchText  IS NULL
           OR CAST(DCM_NO AS NVARCHAR(50)) LIKE '%' + @SearchText + '%'
           OR UPPER(P_NAME)                LIKE UPPER('%' + @SearchText + '%')
           OR CONVERT(VARCHAR, DCM_DATE, 106) LIKE '%' + @SearchText + '%');
END
GO
