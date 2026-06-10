-- =============================================================================
-- Deploy_Auth_DualLogin.sql
-- Run this ONCE against your database to enable dual-login coexistence.
-- Safe to re-run (all steps are idempotent).
--
-- Steps:
--   1. Add UM_BCrypt_Hash column to USER_MASTER
--   2. Create ERP_GetUserForAuth stored procedure
-- =============================================================================

-- ─── STEP 1: Add BCrypt column ───────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'USER_MASTER' AND COLUMN_NAME = 'UM_BCrypt_Hash'
)
BEGIN
    ALTER TABLE USER_MASTER
    ADD UM_BCrypt_Hash NVARCHAR(255) NULL;

    PRINT 'Step 1: Column UM_BCrypt_Hash added to USER_MASTER.';
END
ELSE
BEGIN
    PRINT 'Step 1: Column UM_BCrypt_Hash already exists — skipping.';
END
GO

-- ─── STEP 2: Create ERP_GetUserForAuth SP ────────────────────────────────────

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserForAuth]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_GetUserForAuth]
GO

CREATE PROCEDURE [dbo].[ERP_GetUserForAuth]
    @Username          NVARCHAR(50),
    @CompanyId         INT,
    @FinancialYearCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        um.UM_CODE                   AS UserCode,
        um.UM_USERNAME               AS Username,
        um.UM_NAME                   AS DisplayName,
        um.UM_EMAIL                  AS UserEmail,
        um.UM_PASSWORD               AS PasswordHash,
        ISNULL(um.UM_BCrypt_Hash,'') AS BCryptHash,
        ISNULL(um.UM_IS_ADMIN, 0)    AS IsAdmin,
        um.UM_CM_ID                  AS CompanyId,
        cm.CM_CODE                   AS FinancialYearCode,
        cm.CM_NAME                   AS CompanyName,
        cm.CM_EMAILID                AS CompanyEmail,
        CONVERT(VARCHAR(10), cm.CM_OPENING_DATE, 103) AS OpeningDate,
        CONVERT(VARCHAR(10), cm.CM_CLOSING_DATE, 103) AS ClosingDate,
        ISNULL(cm.CM_ADDRESS1, '')   AS CompanyAddress
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

PRINT 'Step 2: ERP_GetUserForAuth created successfully.';
PRINT 'Deployment complete.';
GO
