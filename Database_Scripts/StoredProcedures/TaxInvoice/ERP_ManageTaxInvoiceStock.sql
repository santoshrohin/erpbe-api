-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-10-24
-- Description: Manages stock for Tax Invoice (INSERT/UPDATE/DELETE operations)
-- =============================================
CREATE PROCEDURE [dbo].[ERP_ManageTaxInvoiceStock]
    @Operation VARCHAR(10), -- 'INSERT', 'UPDATE', 'DELETE'
    @InvoiceCode INT,
    @InvoiceDate DATETIME,
    @ItemCode INT,
    @Quantity FLOAT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        IF @Operation = 'INSERT'
        BEGIN
            -- Insert stock OUT entry (negative quantity for sales)
            INSERT INTO STOCK_LEDGER (
                STL_I_CODE, 
                STL_DOC_NO, 
                STL_DOC_TYPE, 
                STL_DOC_DATE, 
                STL_DOC_QTY
            )
            VALUES (
                @ItemCode,
                @InvoiceCode,
                'TAXINV',
                @InvoiceDate,
                -@Quantity  -- Negative for stock OUT
            );
        END
        ELSE IF @Operation = 'DELETE'
        BEGIN
            -- Reverse stock (delete the stock ledger entry)
            DELETE FROM STOCK_LEDGER
            WHERE STL_DOC_NO = @InvoiceCode 
            AND STL_DOC_TYPE = 'TAXINV'
            AND STL_I_CODE = @ItemCode;
        END
        ELSE IF @Operation = 'UPDATE'
        BEGIN
            -- For update: Delete old entry and insert new one
            -- This is called from UpdateTaxInvoice after deleting old details
            DELETE FROM STOCK_LEDGER
            WHERE STL_DOC_NO = @InvoiceCode 
            AND STL_DOC_TYPE = 'TAXINV'
            AND STL_I_CODE = @ItemCode;
            
            -- Insert new entry
            INSERT INTO STOCK_LEDGER (
                STL_I_CODE, 
                STL_DOC_NO, 
                STL_DOC_TYPE, 
                STL_DOC_DATE, 
                STL_DOC_QTY
            )
            VALUES (
                @ItemCode,
                @InvoiceCode,
                'TAXINV',
                @InvoiceDate,
                -@Quantity  -- Negative for stock OUT
            );
        END

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

