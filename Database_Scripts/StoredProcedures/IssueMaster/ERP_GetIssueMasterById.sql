CREATE OR ALTER PROCEDURE [dbo].[ERP_GetIssueMasterById]
    @IssueCode   INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Header
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
    WHERE IM_CODE    = @IssueCode
      AND IM_COMP_ID = @CompanyCode
      AND ES_DELETE  = 0;

    -- Details
    SELECT
        IMD.IM_CODE       AS IssueCode,
        IMD.IMD_I_CODE    AS ItemCode,
        IM2.I_NAME        AS ItemName,
        IMD.IMD_UOM       AS UomCode,
        UM.UM_DESC        AS UomName,
        IMD.IMD_CURR_STOCK AS CurrentStock,
        IMD.IMD_REQ_QTY   AS RequestedQty,
        IMD.IMD_ISSUE_QTY AS IssuedQty,
        IMD.IMD_REMARK    AS Remark,
        IMD.IMD_RATE      AS Rate,
        IMD.IMD_AMOUNT    AS Amount
    FROM ISSUE_MASTER_DETAIL IMD
    LEFT JOIN ITEM_MASTER IM2 ON IM2.I_CODE = IMD.IMD_I_CODE
    LEFT JOIN UOM          UM  ON UM.UM_CODE  = IMD.IMD_UOM
    WHERE IMD.IM_CODE    = @IssueCode
      AND IMD.ES_DELETE  = 0;
END
GO
