CREATE OR ALTER PROCEDURE [dbo].[ERP_GetDeliveryChallanById]
    @ChallanCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Master
    SELECT
        DCM_CODE                         AS ChallanCode,
        DCM_CM_CODE                      AS CompanyCode,
        DCM_P_CODE                       AS CustomerCode,
        P_NAME                           AS CustomerName,
        DCM_TYPE                         AS Type,
        DCM_NO                           AS ChallanNumber,
        DCM_DATE                         AS ChallanDate,
        DCM_INV_NO                       AS InvoiceNumber,
        DCM_THROUGH                      AS Through,
        DCM_VEH_NO                       AS VehicleNumber,
        DCM_LR_NO                        AS LrNumber,
        DCM_ORDER_NO                     AS OrderNumber,
        DCM_ORDER_DATE                   AS OrderDate,
        DELIVERY_CHALLAN_MASTER.ES_DELETE AS IsDeleted,
        DELIVERY_CHALLAN_MASTER.MODIFY    AS IsModifyLocked,
        DCM_MAT_TYPE                     AS MaterialType,
        DCM_IS_RETURNABLE                AS IsReturnable
    FROM DELIVERY_CHALLAN_MASTER
    LEFT JOIN PARTY_MASTER ON DCM_P_CODE = P_CODE
    WHERE DCM_CODE    = @ChallanCode
      AND DCM_CM_CODE = @CompanyCode
      AND DELIVERY_CHALLAN_MASTER.ES_DELETE = 0;

    -- Details
    SELECT
        DCD_DCM_CODE    AS ChallanCode,
        DCD_I_CODE      AS ItemCode,
        DCD_ORD_QTY     AS OrderedQuantity,
        DCD_BATCH_NO    AS BatchNumber,
        DCD_NO_OF_PACKS AS NumberOfPacks,
        DCD_UM_CODE     AS UomCode,
        ES_DELETE       AS IsDeleted,
        DCD_REMARK      AS Remark,
        DCD_RET_QTY     AS ReturnedQuantity
    FROM DELIVERY_CHALLAN_DETAIL
    WHERE DCD_DCM_CODE = @ChallanCode
      AND ES_DELETE    = 0;
END
GO
