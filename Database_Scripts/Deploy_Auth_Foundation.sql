-- =============================================================================
-- Deploy_Auth_Foundation.sql
-- Auth Foundation — single deployment script for SSMS / sqlcmd
--
-- WHAT THIS DEPLOYS:
--   1. USER_REFRESH_TOKEN table      (new table — no legacy impact)
--   2. ERP_GetUserForAuth            (login: fetch user without password filter)
--   3. ERP_GetUserPermissions        (login: load USER_RIGHT bitmasks into JWT)
--   4. ERP_GetUserByCode             (refresh: reload user context for new JWT)
--   5. ERP_CreateRefreshToken        (login + refresh: store hashed token)
--   6. ERP_ValidateAndRotateRefreshToken  (refresh endpoint: token rotation)
--   7. ERP_RevokeUserRefreshTokens   (logout: revoke all sessions for a user)
--   8. ERP_UpdatePasswordHash        (background: lazy bcrypt rehash)
--
-- COEXISTENCE SAFETY:
--   - All ERP_* stored procedures are NEW — they do NOT replace any SP_* legacy SPs
--   - USER_REFRESH_TOKEN is a NEW table — legacy system is unaware of it
--   - ERP_GetUserPermissions reads USER_RIGHT as READ-ONLY — no schema changes
--   - ERP_UpdatePasswordHash writes to USER_MASTER.UM_PASSWORD — this is the ONLY
--     operation that touches a legacy table. After a user's password is rehashed to
--     bcrypt, the legacy SP_VerifyLogin will fail for that user (they must use the
--     modern system). This is intentional and irreversible during coexistence.
--
-- HOW TO RUN:
--   Option A — SSMS: Open this file, select your database from the dropdown,
--              press F5 to execute.
--   Option B — sqlcmd:
--              sqlcmd -S <server> -d <database> -i Deploy_Auth_Foundation.sql
--
-- Run once per environment (DEV, UAT, PROD).
-- Script is idempotent — safe to re-run (IF NOT EXISTS / DROP+CREATE pattern).
-- =============================================================================

-- !! IMPORTANT !! --
-- If running via sqlcmd, pass the database name as -d argument.
-- If running in SSMS, make sure the correct database is selected above.
-- Uncomment the line below and set the correct database name if needed:
-- USE [YourDatabaseNameHere];
-- GO

PRINT '============================================================';
PRINT 'Auth Foundation Deployment — Started';
PRINT 'Time: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '============================================================';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 1: USER_REFRESH_TOKEN table
-- Must run BEFORE any stored procedures that INSERT into this table.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 1: USER_REFRESH_TOKEN table';

IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[USER_REFRESH_TOKEN]')
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[USER_REFRESH_TOKEN] (
        URT_CODE        INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_USER_REFRESH_TOKEN PRIMARY KEY CLUSTERED,
        URT_UM_CODE     INT           NOT NULL,   -- FK to USER_MASTER.UM_CODE
        URT_TOKEN_HASH  NVARCHAR(500) NOT NULL,   -- SHA-256 hash of the raw token
        URT_EXPIRES_AT  DATETIME2     NOT NULL,
        URT_IS_REVOKED  BIT           NOT NULL CONSTRAINT DF_URT_IS_REVOKED DEFAULT 0,
        URT_CREATED_AT  DATETIME2     NOT NULL CONSTRAINT DF_URT_CREATED_AT DEFAULT GETUTCDATE(),
        URT_REPLACED_BY NVARCHAR(500) NULL,       -- token hash that replaced this (rotation audit)
        URT_IP_ADDRESS  NVARCHAR(50)  NULL,
        URT_USER_AGENT  NVARCHAR(500) NULL
    );

    -- Fast lookup by token hash (filtered: active tokens only)
    CREATE NONCLUSTERED INDEX IX_USER_REFRESH_TOKEN_HASH
        ON [dbo].[USER_REFRESH_TOKEN] (URT_TOKEN_HASH)
        WHERE URT_IS_REVOKED = 0;

    -- Efficient user-level queries (list sessions, revoke all on logout)
    CREATE NONCLUSTERED INDEX IX_USER_REFRESH_TOKEN_UM_CODE
        ON [dbo].[USER_REFRESH_TOKEN] (URT_UM_CODE);

    PRINT '  [OK] USER_REFRESH_TOKEN table created.';
END
ELSE
BEGIN
    PRINT '  [SKIP] USER_REFRESH_TOKEN already exists.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 2: ERP_GetUserForAuth
-- Fetches user row WITHOUT password filter so C# can handle both
-- legacy cipher and bcrypt comparison (lazy-rehash migration support).
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 2: ERP_GetUserForAuth';

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserForAuth]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_GetUserForAuth];
GO

CREATE PROCEDURE [dbo].[ERP_GetUserForAuth]
    @Username          NVARCHAR(50),
    @CompanyId         INT,
    @FinancialYearCode INT       -- maps to COMPANY_MASTER.CM_CODE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        um.UM_CODE                                       AS UserCode,
        um.UM_USERNAME                                   AS Username,
        um.UM_NAME                                       AS DisplayName,
        um.UM_EMAIL                                      AS UserEmail,
        um.UM_PASSWORD                                   AS PasswordHash,  -- legacy cipher or bcrypt
        ISNULL(um.UM_IS_ADMIN, 0)                        AS IsAdmin,
        um.UM_CM_ID                                      AS CompanyId,
        cm.CM_CODE                                       AS FinancialYearCode,
        cm.CM_NAME                                       AS CompanyName,
        cm.CM_EMAILID                                    AS CompanyEmail,
        CONVERT(VARCHAR(10), cm.CM_OPENING_DATE, 103)    AS OpeningDate,
        CONVERT(VARCHAR(10), cm.CM_CLOSING_DATE, 103)    AS ClosingDate,
        ISNULL(cm.CM_ADDRESS1, '')                       AS CompanyAddress
    FROM  USER_MASTER  um
    INNER JOIN COMPANY_MASTER cm ON um.UM_CM_ID = cm.CM_ID
    WHERE um.UM_USERNAME    = @Username
      AND um.UM_CM_ID       = @CompanyId
      AND cm.CM_CODE        = @FinancialYearCode
      AND um.IS_ACTIVE      = 1
      AND um.ES_DELETE      = 0
      AND cm.CM_ACTIVE_IND  = 1;
END
GO

PRINT '  [OK] ERP_GetUserForAuth created.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 3: ERP_GetUserPermissions
-- Reads USER_RIGHT bitmasks for embedding as perm_{ModuleCode} JWT claims.
-- READ-ONLY — no legacy table is modified.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 3: ERP_GetUserPermissions';

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserPermissions]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_GetUserPermissions];
GO

CREATE PROCEDURE [dbo].[ERP_GetUserPermissions]
    @UserCode INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Bitmask positions: 0=View 1=Add 2=Edit 3=Delete 4=Print 5=Approve 6=Export
    SELECT
        UR_SM_CODE  AS ModuleCode,
        UR_RIGHTS   AS RightsBitmask
    FROM  USER_RIGHT
    WHERE UR_UM_CODE   = @UserCode
      AND UR_IS_DELETE = 0
    ORDER BY UR_SM_CODE;
END
GO

PRINT '  [OK] ERP_GetUserPermissions created.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 4: ERP_GetUserByCode
-- Reloads user context during the token-refresh flow for new JWT generation.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 4: ERP_GetUserByCode';

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_GetUserByCode]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_GetUserByCode];
GO

CREATE PROCEDURE [dbo].[ERP_GetUserByCode]
    @UserCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        um.UM_CODE                                       AS UserCode,
        um.UM_USERNAME                                   AS Username,
        um.UM_NAME                                       AS DisplayName,
        um.UM_EMAIL                                      AS UserEmail,
        ISNULL(um.UM_IS_ADMIN, 0)                        AS IsAdmin,
        um.UM_CM_ID                                      AS CompanyId,
        cm.CM_CODE                                       AS FinancialYearCode,
        cm.CM_NAME                                       AS CompanyName,
        cm.CM_EMAILID                                    AS CompanyEmail,
        CONVERT(VARCHAR(10), cm.CM_OPENING_DATE, 103)    AS OpeningDate,
        CONVERT(VARCHAR(10), cm.CM_CLOSING_DATE, 103)    AS ClosingDate
    FROM  USER_MASTER  um
    INNER JOIN COMPANY_MASTER cm ON um.UM_CM_ID = cm.CM_ID
    WHERE um.UM_CODE      = @UserCode
      AND um.IS_ACTIVE    = 1
      AND um.ES_DELETE    = 0
      AND cm.CM_ACTIVE_IND = 1;
END
GO

PRINT '  [OK] ERP_GetUserByCode created.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 5: ERP_CreateRefreshToken
-- Inserts a hashed refresh token after login or token rotation.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 5: ERP_CreateRefreshToken';

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_CreateRefreshToken]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_CreateRefreshToken];
GO

CREATE PROCEDURE [dbo].[ERP_CreateRefreshToken]
    @UserCode   INT,
    @TokenHash  NVARCHAR(500),
    @ExpiresAt  DATETIME2,
    @IpAddress  NVARCHAR(50)  = NULL,
    @UserAgent  NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO USER_REFRESH_TOKEN (
        URT_UM_CODE,
        URT_TOKEN_HASH,
        URT_EXPIRES_AT,
        URT_IP_ADDRESS,
        URT_USER_AGENT
    )
    VALUES (
        @UserCode,
        @TokenHash,
        @ExpiresAt,
        @IpAddress,
        @UserAgent
    );

    SELECT SCOPE_IDENTITY() AS NewTokenCode;
END
GO

PRINT '  [OK] ERP_CreateRefreshToken created.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 6: ERP_ValidateAndRotateRefreshToken
-- Atomically validates an incoming token and marks it as rotated (used).
-- Returns UserCode on success, NULL row on failure.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 6: ERP_ValidateAndRotateRefreshToken';

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_ValidateAndRotateRefreshToken]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_ValidateAndRotateRefreshToken];
GO

CREATE PROCEDURE [dbo].[ERP_ValidateAndRotateRefreshToken]
    @TokenHash     NVARCHAR(500),
    @NewTokenHash  NVARCHAR(500)   -- hash of the incoming replacement token
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @UserCode  INT;
        DECLARE @TokenCode INT;

        -- Find a valid (non-revoked, non-expired) token
        SELECT
            @UserCode  = URT_UM_CODE,
            @TokenCode = URT_CODE
        FROM  USER_REFRESH_TOKEN
        WHERE URT_TOKEN_HASH = @TokenHash
          AND URT_IS_REVOKED = 0
          AND URT_EXPIRES_AT > GETUTCDATE();

        IF @UserCode IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT NULL AS UserCode;
            RETURN;
        END

        -- Mark old token as rotated
        UPDATE USER_REFRESH_TOKEN
        SET    URT_IS_REVOKED  = 1,
               URT_REPLACED_BY = @NewTokenHash
        WHERE  URT_CODE = @TokenCode;

        COMMIT TRANSACTION;

        SELECT @UserCode AS UserCode;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '  [OK] ERP_ValidateAndRotateRefreshToken created.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 7: ERP_RevokeUserRefreshTokens
-- Revokes ALL active sessions for a user (used by /logout endpoint).
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 7: ERP_RevokeUserRefreshTokens';

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_RevokeUserRefreshTokens]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_RevokeUserRefreshTokens];
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

PRINT '  [OK] ERP_RevokeUserRefreshTokens created.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 8: ERP_UpdatePasswordHash
-- Writes a bcrypt hash into USER_MASTER.UM_PASSWORD during lazy rehash.
--
-- !! COEXISTENCE WARNING !!
-- After this runs for a user, they can ONLY authenticate via the modern system.
-- Legacy SP_VerifyLogin will fail for rehashed users (bcrypt format mismatch).
-- This is intentional — each user is migrated silently on their next login.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '-- STEP 8: ERP_UpdatePasswordHash';

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_UpdatePasswordHash]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_UpdatePasswordHash];
GO

CREATE PROCEDURE [dbo].[ERP_UpdatePasswordHash]
    @UserCode   INT,
    @BcryptHash NVARCHAR(255)
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

PRINT '  [OK] ERP_UpdatePasswordHash created.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- VERIFICATION — confirm all objects exist
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '============================================================';
PRINT 'Verification — checking deployed objects:';
PRINT '============================================================';

SELECT
    CASE type WHEN 'U' THEN 'TABLE' ELSE 'PROCEDURE' END AS ObjectType,
    name                                                   AS ObjectName,
    create_date                                            AS CreatedAt,
    modify_date                                            AS LastModified
FROM sys.objects
WHERE name IN (
    'USER_REFRESH_TOKEN',
    'ERP_GetUserForAuth',
    'ERP_GetUserPermissions',
    'ERP_GetUserByCode',
    'ERP_CreateRefreshToken',
    'ERP_ValidateAndRotateRefreshToken',
    'ERP_RevokeUserRefreshTokens',
    'ERP_UpdatePasswordHash'
)
ORDER BY
    CASE type WHEN 'U' THEN 0 ELSE 1 END,
    name;

PRINT '';
PRINT '============================================================';
PRINT 'Auth Foundation Deployment — Completed';
PRINT 'Time: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '============================================================';
GO
