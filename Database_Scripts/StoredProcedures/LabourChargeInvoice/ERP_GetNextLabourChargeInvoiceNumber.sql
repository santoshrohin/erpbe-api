CREATE OR ALTER PROCEDURE [dbo].[ERP_GetNextLabourChargeInvoiceNumber]
    @CompanyCode       INT,
    @NewInvoiceNumber  INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @NewInvoiceNumber = ISNULL(MAX(INM_NO), 0) + 1
    FROM INVOICE_MASTER
    WHERE INM_TYPE    = 'OutJWINM'
      AND INM_CM_CODE = @CompanyCode;
END
GO
