CREATE PROCEDURE [dbo].[ERP_DeleteCustomerPoDetails]
    @PoCode INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        -- Hard delete all detail records for this PO (used during update)
        DELETE FROM CUSTPO_DETAIL
        WHERE CPOD_CPOM_CODE = @PoCode;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

