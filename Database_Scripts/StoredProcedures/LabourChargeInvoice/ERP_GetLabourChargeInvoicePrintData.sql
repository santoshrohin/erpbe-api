CREATE OR ALTER PROCEDURE [dbo].[ERP_GetLabourChargeInvoicePrintData]
    @InvoiceCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Invoice Master with company and customer info
    SELECT
        I.INM_CODE          AS InvoiceCode,
        I.INM_NO            AS InvoiceNumber,
        I.INM_DATE          AS InvoiceDate,
        I.INM_INVOICE_TYPE  AS InvoiceType,
        P.P_NAME            AS CustomerName,
        ISNULL(P.P_ADD1,'') + ISNULL(', ' + P.P_CITY,'') + ISNULL(' - ' + CAST(P.P_PIN_CODE AS NVARCHAR),'') AS CustomerAddress,
        P.P_GST_NO          AS CustomerGstin,
        I.INM_VEH_NO        AS VehicleNumber,
        I.INM_TRANSPORT     AS TransportName,
        I.INM_LR_NO         AS LrNumber,
        I.INM_REMARK        AS Remarks,
        I.INM_DISC          AS DiscountPercentage,
        I.INM_DISC_AMT      AS DiscountAmount,
        I.INM_PACK_AMT      AS PackingAmount,
        I.INM_FREIGHT       AS FreightCharges,
        I.INM_OTHER_AMT     AS OtherAmount,
        I.INM_TAX_TCS       AS TcsPercentage,
        I.INM_TAX_TCS_AMT   AS TcsAmount,
        I.INM_TAXABLE_AMT   AS TaxableAmount,
        I.INM_G_AMT         AS GrossAmount,
        CM.CM_NAME          AS CompanyName,
        ISNULL(CM.cm_address1, '') AS CompanyAddress,
        CM.CM_GST_NO        AS CompanyGstin
    FROM INVOICE_MASTER I
    LEFT JOIN PARTY_MASTER P  ON I.INM_P_CODE = P.P_CODE
    LEFT JOIN COMPANY_MASTER CM ON I.INM_CM_CODE = CM.CM_ID
    WHERE I.INM_CODE = @InvoiceCode
      AND I.INM_CM_CODE = @CompanyCode
      AND I.INM_TYPE = 'OutJWINM'
      AND I.ES_DELETE = 0;

    -- Invoice Line Items
    SELECT
        ROW_NUMBER() OVER (ORDER BY ID.IND_I_CODE)  AS SrNo,
        IM.I_NAME                                    AS ItemName,
        ID.IND_HSN_CODE                              AS HsnCode,
        ISNULL(UM.I_UOM_NAME,'')                     AS Uom,
        ID.IND_INQTY                                 AS Qty,
        ISNULL(ID.IND_RATE, 0)                       AS Rate,
        ISNULL(ID.IND_AMT, 0)                        AS Amount,
        ISNULL(ID.E_BASIC_CentralT, 0)               AS CgstPercentage,
        ISNULL(ID.E_EDU_CESS_State, 0)               AS SgstPercentage,
        ISNULL(ID.E_H_EDU_Integrated, 0)             AS IgstPercentage
    FROM INVOICE_DETAIL ID
    LEFT JOIN ITEM_MASTER IM        ON ID.IND_I_CODE  = IM.I_CODE
    LEFT JOIN ITEM_UNIT_MASTER UM   ON ID.IND_UOM_CODE = UM.I_UOM_CODE
    WHERE ID.IND_INM_CODE = @InvoiceCode
      AND ID.ES_DELETE = 0
    ORDER BY ID.IND_I_CODE;
END
GO
