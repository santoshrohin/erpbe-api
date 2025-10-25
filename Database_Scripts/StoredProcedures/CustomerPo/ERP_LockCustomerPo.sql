CREATE PROCEDURE [dbo].[ERP_LockCustomerPo]
    @PoCode INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        UPDATE CUSTPO_MASTER
        SET MODIFY = 1
        WHERE CPOM_CODE = @PoCode;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

