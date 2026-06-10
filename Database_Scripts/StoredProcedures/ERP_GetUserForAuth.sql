-- =============================================================================
-- ERP_GetUserForAuth
-- Fetches user record for authentication WITHOUT filtering by password.
-- Password comparison is done in C# to support dual-login coexistence.
--
-- Dual-login coexistence strategy:
--   - UM_PASSWORD  = legacy cipher (written by legacy system, NEVER overwritten
--                    by modern system)
--   - UM_BCrypt_Hash = BCrypt hash written by modern system on first new-system login
--
-- Modern system auth order:
--   1. If UM_BCrypt_Hash IS NOT NULL → BCrypt.Verify(plain, UM_BCrypt_Hash)
--   2. Else → LegacyEncryption.Verify(plain, UM_PASSWORD)
--   3. On step-2 success → store BCrypt in UM_BCrypt_Hash (NOT UM_PASSWORD)
--
-- Result: both systems can authenticate against the same USER_MASTER row
--         indefinitely, with no interference.
--
-- Coexistence note:
--   Legacy system continues using SP_VerifyLogin (reads UM_PASSWORD only).
--   This SP is additive — it does NOT replace SP_VerifyLogin.
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserForAuth]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_GetUserForAuth]
GO

CREATE PROCEDURE [dbo].[ERP_GetUserForAuth]
    @Username         NVARCHAR(50),
    @CompanyId        INT,
    @FinancialYearCode INT         -- maps to COMPANY_MASTER.CM_CODE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        um.UM_CODE          AS UserCode,
        um.UM_USERNAME      AS Username,
        um.UM_NAME          AS DisplayName,
        um.UM_EMAIL         AS UserEmail,
        um.UM_PASSWORD               AS PasswordHash,    -- legacy cipher (NEVER modified by modern system)
        ISNULL(um.UM_BCrypt_Hash,'') AS BCryptHash,      -- bcrypt written by modern system; '' until first new-system login
        ISNULL(um.UM_IS_ADMIN, 0) AS IsAdmin,
        um.UM_CM_ID         AS CompanyId,
        cm.CM_CODE          AS FinancialYearCode,
        cm.CM_NAME          AS CompanyName,
        cm.CM_EMAILID       AS CompanyEmail,
        CONVERT(VARCHAR(10), cm.CM_OPENING_DATE, 103) AS OpeningDate,
        CONVERT(VARCHAR(10), cm.CM_CLOSING_DATE, 103) AS ClosingDate,
        ISNULL(cm.CM_ADDRESS1, '') AS CompanyAddress
    FROM USER_MASTER um
    INNER JOIN COMPANY_MASTER cm
           ON um.UM_CM_ID = cm.CM_ID
    WHERE um.UM_USERNAME   = @Username
      AND um.UM_CM_ID      = @CompanyId
      AND cm.CM_CODE       = @FinancialYearCode
      AND um.IS_ACTIVE     = 1
      AND um.ES_DELETE     = 0
      AND cm.CM_ACTIVE_IND = 1;
END
GO
