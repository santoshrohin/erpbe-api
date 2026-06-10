-- =============================================================================
-- Migrate_Add_BCrypt_Hash_Column.sql
-- Adds the UM_BCrypt_Hash column to USER_MASTER for dual-login coexistence.
--
-- Background:
--   Previously the modern system wrote BCrypt hashes to UM_PASSWORD, which broke
--   legacy SP_VerifyLogin. This migration adds a SEPARATE column so:
--     - UM_PASSWORD remains untouched (legacy login always works)
--     - UM_BCrypt_Hash stores the modern system's BCrypt hash
--
-- Safe to run multiple times (checks column existence first).
-- Run BEFORE deploying the updated ERP_GetUserForAuth and ERP_StoreBcryptHash SPs.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'USER_MASTER' AND COLUMN_NAME = 'UM_BCrypt_Hash'
)
BEGIN
    ALTER TABLE USER_MASTER
    ADD UM_BCrypt_Hash NVARCHAR(255) NULL;

    PRINT 'Column UM_BCrypt_Hash added to USER_MASTER.';
END
ELSE
BEGIN
    PRINT 'Column UM_BCrypt_Hash already exists — skipping.';
END
GO

-- If UM_PASSWORD already contains BCrypt hashes (from the old rehash strategy),
-- copy them to UM_BCrypt_Hash and clear UM_PASSWORD back to the legacy cipher.
-- IMPORTANT: Only do this if you know which rows have BCrypt in UM_PASSWORD.
-- Run a SELECT first to inspect: SELECT UM_CODE, LEFT(UM_PASSWORD,4) AS HashPrefix FROM USER_MASTER
-- BCrypt hashes start with $2a$ or $2b$.
--
-- Uncomment and run manually after verifying:
-- UPDATE USER_MASTER
-- SET    UM_BCrypt_Hash = UM_PASSWORD,
--        UM_PASSWORD    = NULL   -- or restore original legacy cipher if known
-- WHERE  UM_PASSWORD LIKE '$2a$%' OR UM_PASSWORD LIKE '$2b$%';
--
-- If you don't have the original legacy ciphers, users with BCrypt in UM_PASSWORD
-- will need to reset their password OR you can simply leave UM_BCrypt_Hash NULL
-- for those users — ERP_GetUserForAuth returns both columns and LoginRepository
-- will try BCrypt first (via UM_BCrypt_Hash or UM_PASSWORD detection).

PRINT 'Migration complete. Deploy ERP_GetUserForAuth and ERP_StoreBcryptHash next.';
GO
