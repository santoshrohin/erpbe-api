-- =============================================================================
-- ERP_GetUserPermissions
-- Loads all module permission bitmasks for a user from USER_RIGHT table.
-- Returns legacy module codes + 7-character bitmask strings exactly as stored.
--
-- Bitmask position meanings (ACTUAL legacy definition, verified from UserRights_BL.cs):
--   0 = MENU (menu-visibility; not enforced by API endpoints)
--   1 = VIEW
--   2 = EDIT  (legacy label: "Update")
--   3 = ADD
--   4 = DELETE
--   5 = PRINT
--   6 = BACK DATE (back-dated entry permission)
--
-- These are embedded as JWT claims: perm_{ModuleCode} = RightsBitmask
-- Example: perm_75 = "1111000"  (Sales: view+add+edit+delete, no print/approve/export)
--
-- Coexistence note:
--   Reads directly from USER_RIGHT — same table used by legacy system.
--   No schema change. Read-only.
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserPermissions]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_GetUserPermissions]
GO

CREATE PROCEDURE [dbo].[ERP_GetUserPermissions]
    @UserCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UR_SM_CODE   AS ModuleCode,
        UR_RIGHTS    AS RightsBitmask
    FROM USER_RIGHT
    WHERE UR_UM_CODE  = @UserCode
      AND UR_IS_DELETE = 0
    ORDER BY UR_SM_CODE;
END
GO
