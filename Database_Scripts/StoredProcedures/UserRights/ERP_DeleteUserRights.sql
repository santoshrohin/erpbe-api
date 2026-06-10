IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ERP_DeleteUserRights]') AND type IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ERP_DeleteUserRights]
GO

CREATE PROCEDURE [dbo].[ERP_DeleteUserRights]
    @UserCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE USER_RIGHT
    SET    UR_IS_DELETE = 1
    WHERE  UR_UM_CODE   = @UserCode;

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO
