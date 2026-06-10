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
        DCM_CODE        AS ChallanCode,
        DCM_CM_CODE     AS CompanyCode,
        DCM_P_CODE      AS CustomerCode,
        DCM_TYPE        AS Type,
        DCM_NO          AS ChallanNumber,
        DCM_DATE        AS ChallanDate,
        DCM_INV_NO      AS InvoiceNumber,
        DCM_THROUGH     AS Through,
        DCM_VEH_NO      AS VehicleNumber,
        DCM_LR_NO       AS LrNumber,
        DCM_ORDER_NO    AS OrderNumber,
        DCM_ORDER_DATE  AS OrderDate,
        ES_DELETE       AS IsDeleted,
        MODIFY          AS IsModifyLocked,
        DCM_MAT_TYPE    AS MaterialType,
        DCM_IS_RETURNABLE AS IsReturnable
    FROM DELIVERY_CHALLAN_MASTER
    WHERE ES_DELETE     = 0
      AND DCM_CM_CODE   = @CompanyCode
      AND (@CustomerCode IS NULL OR DCM_P_CODE   = @CustomerCode)
      AND (@DateFrom    IS NULL OR DCM_DATE      >= @DateFrom)
      AND (@DateTo      IS NULL OR DCM_DATE      <= @DateTo)
      AND (@SearchText  IS NULL OR DCM_INV_NO    LIKE '%' + @SearchText + '%'
                                OR DCM_ORDER_NO  LIKE '%' + @SearchText + '%')
    ORDER BY DCM_DATE DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*)
    FROM DELIVERY_CHALLAN_MASTER
    WHERE ES_DELETE   = 0
      AND DCM_CM_CODE = @CompanyCode
      AND (@CustomerCode IS NULL OR DCM_P_CODE  = @CustomerCode)
      AND (@DateFrom    IS NULL OR DCM_DATE     >= @DateFrom)
      AND (@DateTo      IS NULL OR DCM_DATE     <= @DateTo)
      AND (@SearchText  IS NULL OR DCM_INV_NO   LIKE '%' + @SearchText + '%'
                                OR DCM_ORDER_NO LIKE '%' + @SearchText + '%');
END
GO
