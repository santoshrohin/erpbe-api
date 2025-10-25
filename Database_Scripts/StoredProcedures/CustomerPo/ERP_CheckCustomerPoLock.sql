CREATE PROCEDURE [dbo].[ERP_CheckCustomerPoLock]
    @PoCode INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        SELECT ISNULL(MODIFY, 0) AS IsLocked
        FROM CUSTPO_MASTER
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

