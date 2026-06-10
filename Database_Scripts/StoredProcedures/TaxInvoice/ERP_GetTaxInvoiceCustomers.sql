-- =============================================
-- Description: Get customers who have active Customer POs with pending quantities
-- Used in: Tax Invoice form - Customer dropdown
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoiceCustomers]
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        PM.P_CODE       AS Id,
        PM.P_NAME       AS DisplayName,
        PM.P_ADD1       AS Address,
        PM.P_SM_CODE    AS StateCode,
        ISNULL(SM.SM_NAME, '')  AS StateName,
        PM.P_LBT_NO     AS GstNumber,
        PM.P_LBT_IND    AS GstApplicable
    FROM PARTY_MASTER PM
    INNER JOIN CUSTPO_MASTER CPM ON PM.P_CODE = CPM.CPOM_P_CODE
    LEFT  JOIN STATE_MASTER SM   ON PM.P_SM_CODE = SM.SM_CODE
    WHERE PM.ES_DELETE = 0
      AND PM.P_TYPE = 1
      AND ISNULL(PM.P_ACTIVE_IND, 0) = 1
      AND PM.P_CM_COMP_ID = @CompanyCode
      AND CPM.ES_DELETE = 0
      AND CPM.CPOM_CM_COMP_ID = @CompanyCode
      AND EXISTS (
          SELECT 1
          FROM CUSTPO_DETAIL CPD
          WHERE CPD.CPOD_CPOM_CODE = CPM.CPOM_CODE
            AND (CPD.CPOD_ORD_QTY - ISNULL(CPD.CPOD_DISPACH, 0)) > 0
      )
    ORDER BY PM.P_NAME;
END
GO
