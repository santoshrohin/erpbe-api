-- =============================================================================
-- ERP_StoreBcryptHash
-- Stores the bcrypt hash for a user in the DEDICATED new column UM_BCrypt_Hash.
-- This replaces the old ERP_UpdatePasswordHash which wrote to UM_PASSWORD.
--
-- Coexistence guarantee:
--   UM_PASSWORD is NEVER touched by the modern system.
--   The legacy SP_VerifyLogin continues to read UM_PASSWORD unchanged.
--   Users can log into BOTH systems simultaneously.
--
-- How it works:
--   Modern system login path:
--     1. Check UM_BCrypt_Hash (if present) → BCrypt.Verify → fast, secure
--     2. Fall back to UM_PASSWORD (legacy cipher) → LegacyEncryption.Verify
--     3. On successful fallback → call this SP to store BCrypt in UM_BCrypt_Hash
--        (UM_PASSWORD left unchanged so legacy login still works)
--
-- Run Deploy_Auth_Foundation.sql first to create the UM_BCrypt_Hash column.
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_StoreBcryptHash]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_StoreBcryptHash]
GO

CREATE PROCEDURE [dbo].[ERP_StoreBcryptHash]
    @UserCode    INT,
    @BcryptHash  NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE USER_MASTER
    SET    UM_BCrypt_Hash = @BcryptHash
    WHERE  UM_CODE        = @UserCode
      AND  ES_DELETE      = 0;

    SELECT @@ROWCOUNT AS UpdatedRows;
END
GO
