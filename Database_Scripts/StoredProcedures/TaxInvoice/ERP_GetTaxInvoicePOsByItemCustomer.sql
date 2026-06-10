-- =============================================
-- Description: Get Customer POs for a specific item + customer with pending dispatch qty.
--   In MODIFY mode, also includes POs already dispatched for this invoice (InvoiceCode).
--   Returns rate, amort rate, pending qty, PO date, and first-item tax code.
-- Used in: Tax Invoice form - PO dropdown per line item
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoicePOsByItemCustomer]
    @ItemCode     INT,
    @CustomerCode INT,
    @CompanyCode  INT,
    @InvoiceCode  INT = NULL   -- NULL for INSERT mode; InvoiceCode for MODIFY mode
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CPM.CPOM_CODE                                                       AS PoCode,
        CPM.CPOM_PONO                                                       AS PoNumber,
        ISNULL(CPM.CPOM_DATE, CPM.CPOM_PO_DATE)                            AS PoDate,
        -- Pending qty = ordered - dispatched
        (CPD.CPOD_ORD_QTY - ISNULL(CPD.CPOD_DISPACH, 0))                  AS PendingQuantity,
        CPD.CPOD_ORD_QTY                                                    AS OrderedQuantity,
        ISNULL(CPD.CPOD_DISPACH, 0)                                        AS DispatchedQuantity,
        ISNULL(CPD.CPOD_RATE, 0)                                           AS Rate,
        ISNULL(CPD.CPOD_DISC_AMT, 0)                                       AS DiscountAmount,
        -- Net rate = rate - (discount / ordered qty)
        ISNULL(CPD.CPOD_RATE, 0)
            - ISNULL(CPD.CPOD_DISC_AMT / NULLIF(CPD.CPOD_ORD_QTY, 0), 0) AS NetRate,
        ISNULL(CPD.CPOD_AMORTRATE, 0)                                      AS AmortRate,
        ISNULL(CPD.CPOD_ST_CODE, 0)                                        AS TaxCode,
        CPD.CPOD_UOM_CODE                                                   AS UomCode
    FROM CUSTPO_MASTER CPM
    INNER JOIN CUSTPO_DETAIL CPD ON CPM.CPOM_CODE = CPD.CPOD_CPOM_CODE
    WHERE CPM.CPOM_P_CODE        = @CustomerCode
      AND CPD.CPOD_I_CODE        = @ItemCode
      AND CPM.CPOM_CM_COMP_ID    = @CompanyCode
      AND CPM.ES_DELETE          = 0
      AND (
          -- Normal: pending qty > 0
          (CPD.CPOD_ORD_QTY - ISNULL(CPD.CPOD_DISPACH, 0)) > 0
          OR
          -- MODIFY: include POs already dispatched for this invoice so user can re-select
          (@InvoiceCode IS NOT NULL AND EXISTS (
              SELECT 1 FROM INVOICE_DETAIL ID
              WHERE ID.IND_CPOM_CODE = CPM.CPOM_CODE
                AND ID.IND_I_CODE    = @ItemCode
                AND ID.IND_INM_CODE  = @InvoiceCode
                AND ISNULL(ID.ES_DELETE, 0) = 0
          ))
      )
    ORDER BY CPM.CPOM_DATE DESC;
END
GO
