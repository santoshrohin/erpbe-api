-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-10-24
-- Description: Gets available stock for an item by summing STOCK_LEDGER entries
-- Note: This uses STOCK_LEDGER (sum of all transactions) instead of ITEM_MASTER.I_QTY
-- =============================================
CREATE PROCEDURE [dbo].[ERP_GetItemStock]
    @ItemCode INT,
    @CompanyCode INT,
    @AvailableQuantity FLOAT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        -- Sum all stock movements for this item
        -- Positive = Stock IN (Purchases, Returns, etc.)
        -- Negative = Stock OUT (Sales, Issues, etc.)
        SELECT @AvailableQuantity = ISNULL(SUM(STL_DOC_QTY), 0)
        FROM STOCK_LEDGER
        WHERE STL_I_CODE = @ItemCode;

        -- If no stock ledger entries found, return 0
        IF @AvailableQuantity IS NULL
            SET @AvailableQuantity = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

