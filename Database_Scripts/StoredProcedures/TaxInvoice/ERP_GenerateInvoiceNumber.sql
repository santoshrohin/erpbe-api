-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-01-24
-- Description: Generates next Invoice Number for a company
-- =============================================
CREATE PROCEDURE [dbo].[ERP_GenerateInvoiceNumber]
    @CompanyCode INT,
    @NewInvoiceNumber INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Get the max invoice number for this company and increment
        SELECT @NewInvoiceNumber = ISNULL(MAX(INM_NO), 0) + 1
        FROM INVOICE_MASTER
        WHERE INM_CM_CODE = @CompanyCode;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

