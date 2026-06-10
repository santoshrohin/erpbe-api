IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ERP_UpsertUserRight]') AND type IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ERP_UpsertUserRight]
GO

CREATE PROCEDURE [dbo].[ERP_UpsertUserRight]
    @UserCode   INT,
    @ScreenCode INT,
    @Bitmask    NVARCHAR(7)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM USER_RIGHT
        WHERE UR_UM_CODE = @UserCode AND UR_SM_CODE = @ScreenCode
    )
    BEGIN
        UPDATE USER_RIGHT
        SET    UR_RIGHTS    = @Bitmask,
               UR_IS_DELETE = 0
        WHERE  UR_UM_CODE   = @UserCode
          AND  UR_SM_CODE   = @ScreenCode;
    END
    ELSE
    BEGIN
        INSERT INTO USER_RIGHT (UR_UM_CODE, UR_SM_CODE, UR_RIGHTS, UR_IS_DELETE)
        VALUES (@UserCode, @ScreenCode, @Bitmask, 0);
    END

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO
