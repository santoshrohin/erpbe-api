-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-10-24
-- Description: Checks if an invoice is locked for editing
-- =============================================
CREATE PROCEDURE [dbo].[ERP_CheckInvoiceLock]
    @InvoiceCode INT,
    @IsLocked BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT @IsLocked = ISNULL(MODIFY, 0) 
        FROM INVOICE_MASTER 
        WHERE INM_CODE = @InvoiceCode;

        -- If invoice not found, return 0 (not locked)
        IF @IsLocked IS NULL
            SET @IsLocked = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

