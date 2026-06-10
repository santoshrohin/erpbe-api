CREATE OR ALTER PROCEDURE [dbo].[ERP_GetDeliveryChallanPrintData]
    @ChallanCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Challan Master with company and customer info
    SELECT
        D.DCM_CODE          AS ChallanCode,
        D.DCM_NO            AS ChallanNumber,
        D.DCM_DATE          AS ChallanDate,
        P.P_NAME            AS CustomerName,
        ISNULL(P.P_ADD1,'') + ISNULL(', ' + P.P_CITY,'') + ISNULL(' - ' + CAST(P.P_PIN_CODE AS NVARCHAR),'') AS CustomerAddress,
        D.DCM_INV_NO        AS InvoiceNumber,
        D.DCM_THROUGH       AS Through,
        D.DCM_VEH_NO        AS VehicleNumber,
        D.DCM_LR_NO         AS LrNumber,
        D.DCM_ORDER_NO      AS OrderNumber,
        D.DCM_ORDER_DATE    AS OrderDate,
        D.DCM_IS_RETURNABLE AS IsReturnable,
        CM.CM_NAME          AS CompanyName,
        ISNULL(CM.cm_address1, '') AS CompanyAddress,
        CM.CM_GST_NO        AS CompanyGstin
    FROM DELIVERY_CHALLAN_MASTER D
    LEFT JOIN PARTY_MASTER P   ON D.DCM_P_CODE = P.P_CODE
    LEFT JOIN COMPANY_MASTER CM ON D.DCM_CM_CODE = CM.CM_CODE
    WHERE D.DCM_CODE    = @ChallanCode
      AND D.DCM_CM_CODE = @CompanyCode
      AND D.ES_DELETE   = 0;

    -- Challan Line Items
    SELECT
        ROW_NUMBER() OVER (ORDER BY DD.DCD_I_CODE) AS SrNo,
        IM.I_NAME                                   AS ItemName,
        ISNULL(UM.I_UOM_NAME,'')                    AS Uom,
        DD.DCD_ORD_QTY                              AS OrderedQuantity,
        ISNULL(DD.DCD_BATCH_NO,'')                  AS BatchNumber,
        ISNULL(DD.DCD_NO_OF_PACKS,'')               AS NumberOfPacks,
        ISNULL(DD.DCD_REMARK,'')                    AS Remark
    FROM DELIVERY_CHALLAN_DETAIL DD
    LEFT JOIN ITEM_MASTER IM      ON DD.DCD_I_CODE  = IM.I_CODE
    LEFT JOIN ITEM_UNIT_MASTER UM ON DD.DCD_UM_CODE = UM.I_UOM_CODE
    WHERE DD.DCD_DCM_CODE = @ChallanCode
      AND DD.ES_DELETE    = 0
    ORDER BY DD.DCD_I_CODE;
END
GO
