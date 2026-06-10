IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetScreenMasters]') AND type IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ERP_GetScreenMasters]
GO

CREATE PROCEDURE [dbo].[ERP_GetScreenMasters]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SM_CODE  AS ScreenCode,
        SM_NAME  AS ScreenName,
        SM_MOD_CODE AS ModuleCode
    FROM SCREEN_MASTER
    ORDER BY SM_MOD_CODE, SM_NAME;
END
GO
