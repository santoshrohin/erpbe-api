-- =============================================================================
-- ERP_UpdatePasswordHash
-- Updates UM_PASSWORD with a bcrypt hash after successful legacy-cipher login.
-- This is the lazy-rehash step — only called when user logs in with old cipher.
--
-- Coexistence note:
--   Writes to USER_MASTER.UM_PASSWORD — same column legacy system uses.
--   Legacy system will still work after rehash ONLY IF the legacy cipher
--   comparison fails gracefully (it won't find the bcrypt format).
--   After rehash, the user can ONLY log in via the modern system.
--   Plan: rehash is irreversible; use with care during coexistence period.
--   Recommendation: after full cutover, run a batch to rehash remaining users.
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_UpdatePasswordHash]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_UpdatePasswordHash]
GO

CREATE PROCEDURE [dbo].[ERP_UpdatePasswordHash]
    @UserCode    INT,
    @BcryptHash  NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE USER_MASTER
    SET    UM_PASSWORD = @BcryptHash
    WHERE  UM_CODE     = @UserCode
      AND  ES_DELETE   = 0;

    SELECT @@ROWCOUNT AS UpdatedRows;
END
GO
