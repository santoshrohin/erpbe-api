-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-10-24
-- Description: Gets basic user information (username and company ID)
-- =============================================
CREATE PROCEDURE [dbo].[ERP_GetUserBasicInfo]
    @UserId INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        SELECT 
            UM_USERNAME AS Username,
            UM_CM_ID AS CompanyId
        FROM USER_MASTER
        WHERE UM_CODE = @UserId;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

