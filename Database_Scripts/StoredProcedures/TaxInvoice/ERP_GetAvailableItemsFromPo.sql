-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-01-24
-- Description: Gets available items from a Customer PO for invoicing
-- =============================================
CREATE PROCEDURE [dbo].[ERP_GetAvailableItemsFromPo]
    @CustomerPoCode INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT OFF;

    -- Get items from Customer PO with remaining quantity to invoice
    SELECT 
        NULL AS InvoiceMasterCode,
        POD.CPODT_I_CODE AS ItemCode,
        I.I_CODENO AS ItemCode_Display,
        I.I_NAME AS ItemName,
        POD.CPODT_UOM_CODE AS UomCode,
        @CustomerPoCode AS CustomerPoCode,
        POD.CPODT_QTY AS InvoiceQuantity,
        POD.CPODT_RATE AS Rate,
        I.I_QTY AS StockQuantity,
        (POD.CPODT_QTY - ISNULL((
            SELECT SUM(IND_INQTY) 
            FROM INVOICE_DETAIL 
            WHERE IND_CPOM_CODE = @CustomerPoCode 
            AND IND_I_CODE = POD.CPODT_I_CODE
            AND ES_DELETE = 0
        ), 0)) AS PendingQuantity
    FROM CUSTOMER_PO_DETAIL POD
    INNER JOIN ITEM_MASTER I ON POD.CPODT_I_CODE = I.I_CODE
    INNER JOIN CUSTOMER_PO_MASTER POM ON POD.CPODT_CPOM_CODE = POM.CPOM_CODE
    WHERE POD.CPODT_CPOM_CODE = @CustomerPoCode
    AND POM.CPOM_CM_CODE = @CompanyCode
    AND POD.ES_DELETE = 0
    ORDER BY POD.CPODT_I_CODE;

END
GO

