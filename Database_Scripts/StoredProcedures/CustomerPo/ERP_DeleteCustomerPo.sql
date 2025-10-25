CREATE PROCEDURE [dbo].[ERP_DeleteCustomerPo]
    @PoCode INT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        -- Soft delete master (details don't have ES_DELETE flag, just orphaned)
        UPDATE CUSTPO_MASTER
        SET ES_DELETE = 1
        WHERE 
            CPOM_CODE = @PoCode 
            AND CPOM_CM_COMP_ID = @CompanyId
            AND ES_DELETE = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

