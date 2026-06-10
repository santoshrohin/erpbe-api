-- =============================================================================
-- ERP_RevokeUserRefreshTokens
-- Revokes all active refresh tokens for a user (logout / security event).
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_RevokeUserRefreshTokens]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_RevokeUserRefreshTokens]
GO

CREATE PROCEDURE [dbo].[ERP_RevokeUserRefreshTokens]
    @UserCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE USER_REFRESH_TOKEN
    SET    URT_IS_REVOKED = 1
    WHERE  URT_UM_CODE   = @UserCode
      AND  URT_IS_REVOKED = 0;

    SELECT @@ROWCOUNT AS RevokedCount;
END
GO
