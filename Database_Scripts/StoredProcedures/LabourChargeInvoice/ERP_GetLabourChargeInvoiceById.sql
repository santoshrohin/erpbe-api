-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-11
-- Description: Gets a Labour Charge Invoice by ID (INM_TYPE='OutJWINM') with all line items
-- =============================================
CREATE PROCEDURE [dbo].[ERP_GetLabourChargeInvoiceById]
    @InvoiceCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Return Invoice Master
    SELECT
        INM_CODE AS InvoiceCode,
        INM_CM_CODE AS CompanyCode,
        INM_NO AS InvoiceNumber,
        INM_DATE AS InvoiceDate,
        INM_INVOICE_TYPE AS InvoiceType,
        INM_P_CODE AS CustomerCode,
        P.P_NAME AS CustomerName,
        INM_CPOM_CODE AS CustomerPoCode,
        INM_NET_AMT AS NetAmount,
        INM_DISC AS DiscountPercentage,
        INM_DISC_AMT AS DiscountAmount,
        INM_S_TAX AS ServiceTaxPercentage,
        INM_S_TAX_AMT AS ServiceTaxAmount,
        INM_TAX_TCS AS TcsPercentage,
        INM_TAX_TCS_AMT AS TcsAmount,
        INM_PACK_AMT AS PackingAmount,
        INM_G_AMT AS GrossAmount,
        INM_T_CODE AS TaxCode,
        INM_VEH_NO AS VehicleNumber,
        INM_TRANSPORT AS TransportName,
        INM_ISSUE_DATE AS IssueDate,
        INM_REMOVAL_DATE AS RemovalDate,
        INM_REMARK AS Remarks,
        INM_LR_NO AS LrNumber,
        INM_LR_DATE AS LrDate,
        INM_TAXABLE_AMT AS TaxableAmount,
        INM_ROUNDING_AMT AS RoundingAmount,
        INM_OTHER_AMT AS OtherAmount,
        INM_FREIGHT AS FreightCharges,
        INM_INSURANCE AS InsuranceAmount,
        INM_TRANS_AMT AS TransportAmount,
        INM_OCTRI_AMT AS OctriAmount,
        INM_C_DAYS AS CreditDays,
        INM_HSN_CODE AS HsnCode,
        I.ES_DELETE AS IsDeleted,
        I.MODIFY AS IsModifyLocked
    FROM INVOICE_MASTER I
    LEFT JOIN PARTY_MASTER P ON I.INM_P_CODE = P.P_CODE
    WHERE I.INM_CODE = @InvoiceCode AND I.INM_CM_CODE = @CompanyCode AND I.INM_TYPE = 'OutJWINM';

    -- Return Invoice Details
    SELECT
        IND_INM_CODE AS InvoiceMasterCode,
        IND_I_CODE AS ItemCode,
        IM.I_NAME AS ItemName,
        IND_UOM_CODE AS UomCode,
        IND_CPOM_CODE AS CustomerPoCode,
        IND_INQTY AS InvoiceQuantity,
        IND_RATE AS Rate,
        IND_AMT AS Amount,
        IND_HSN_CODE AS HsnCode,
        IND_BACHNO AS BatchNumber,
        E_BASIC_CentralT AS CgstPercentage,
        E_EDU_CESS_State AS SgstPercentage,
        E_H_EDU_Integrated AS IgstPercentage,
        IND_NO_PACK AS NumberOfPackages,
        IND_REMARK AS Remarks,
        IND_SUBHEADING AS SubHeading,
        IND_EX_AMT AS ExciseAmount,
        IND_AMORT_RATE AS AmortRate,
        IND_AMORTAMT AS AmortAmount
    FROM INVOICE_DETAIL ID
    LEFT JOIN ITEM_MASTER IM ON ID.IND_I_CODE = IM.I_CODE
    WHERE ID.IND_INM_CODE = @InvoiceCode AND ID.ES_DELETE = 0
    ORDER BY ID.IND_I_CODE;

END
GO
