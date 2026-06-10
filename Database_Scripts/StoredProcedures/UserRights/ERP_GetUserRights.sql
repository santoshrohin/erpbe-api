IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserRights]') AND type IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ERP_GetUserRights]
GO

CREATE PROCEDURE [dbo].[ERP_GetUserRights]
    @UserCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Returns all screens joined with the user's current rights (NULL bitmask = no right assigned)
    SELECT
        @UserCode           AS UserCode,
        sm.SM_CODE          AS ScreenCode,
        sm.SM_NAME          AS ScreenName,
        sm.SM_MOD_CODE      AS ModuleCode,
        ISNULL(ur.UR_RIGHTS, '0000000') AS Bitmask
    FROM SCREEN_MASTER sm
    LEFT JOIN USER_RIGHT ur
           ON ur.UR_SM_CODE  = sm.SM_CODE
          AND ur.UR_UM_CODE  = @UserCode
          AND ur.UR_IS_DELETE = 0
    ORDER BY sm.SM_MOD_CODE, sm.SM_NAME;
END
GO
