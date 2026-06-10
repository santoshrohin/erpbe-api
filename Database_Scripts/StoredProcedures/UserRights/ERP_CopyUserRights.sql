IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ERP_CopyUserRights]') AND type IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ERP_CopyUserRights]
GO

CREATE PROCEDURE [dbo].[ERP_CopyUserRights]
    @FromUserCode INT,
    @ToUserCode   INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Soft-delete existing rights for the target user
    UPDATE USER_RIGHT SET UR_IS_DELETE = 1
    WHERE UR_UM_CODE = @ToUserCode;

    -- Copy all active rights from source user to target user (upsert)
    MERGE USER_RIGHT AS target
    USING (
        SELECT @ToUserCode AS UR_UM_CODE, UR_SM_CODE, UR_RIGHTS
        FROM   USER_RIGHT
        WHERE  UR_UM_CODE   = @FromUserCode
          AND  UR_IS_DELETE  = 0
    ) AS source
    ON (target.UR_UM_CODE = source.UR_UM_CODE AND target.UR_SM_CODE = source.UR_SM_CODE)
    WHEN MATCHED THEN
        UPDATE SET target.UR_RIGHTS = source.UR_RIGHTS, target.UR_IS_DELETE = 0
    WHEN NOT MATCHED THEN
        INSERT (UR_UM_CODE, UR_SM_CODE, UR_RIGHTS, UR_IS_DELETE)
        VALUES (source.UR_UM_CODE, source.UR_SM_CODE, source.UR_RIGHTS, 0);

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO
