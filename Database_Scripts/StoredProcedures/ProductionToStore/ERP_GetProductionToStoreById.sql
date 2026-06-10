CREATE OR ALTER PROCEDURE [dbo].[ERP_GetProductionToStoreById]
    @ProductionCode INT,
    @CompanyCode    INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Master
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
    WHERE PS_CODE         = @ProductionCode
      AND PS_CM_COMP_CODE = @CompanyCode
      AND ES_DELETE       = 0;

    -- Details (with item name via JOIN)
    SELECT
        PSD_PS_CODE AS ProductionCode,
        PSD_I_CODE  AS ItemCode,
        IM.I_NAME   AS ItemName,
        PSD_QTY     AS Quantity,
        PSD_REMARK  AS Remark
    FROM PRODUCTION_TO_STORE_DETAIL d
    LEFT JOIN ITEM_MASTER IM ON IM.I_CODE = d.PSD_I_CODE
    WHERE d.PSD_PS_CODE = @ProductionCode;
END
GO
