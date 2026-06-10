CREATE OR ALTER PROCEDURE [dbo].[SP_RemoveAllRolesFromUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT OFF;
    
    BEGIN TRY
        DELETE FROM [UserRoles] WHERE [UserId] = @UserId
        
        SELECT @@ROWCOUNT as AffectedRows
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END