-- =============================================================================
-- USER_REFRESH_TOKEN table
-- Stores hashed refresh tokens for JWT silent-refresh strategy.
-- Access token lives in React memory (short-lived).
-- Refresh token lives in httpOnly cookie (long-lived, rotated on each use).
--
-- Coexistence note:
--   New table — no impact on legacy system whatsoever.
--   Legacy system is unaware of this table.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[USER_REFRESH_TOKEN]')
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[USER_REFRESH_TOKEN] (
        URT_CODE        INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_USER_REFRESH_TOKEN PRIMARY KEY CLUSTERED,
        URT_UM_CODE     INT          NOT NULL,   -- FK to USER_MASTER.UM_CODE
        URT_TOKEN_HASH  NVARCHAR(500) NOT NULL,  -- SHA-256 hash of the raw token
        URT_EXPIRES_AT  DATETIME2    NOT NULL,
        URT_IS_REVOKED  BIT          NOT NULL CONSTRAINT DF_URT_IS_REVOKED DEFAULT 0,
        URT_CREATED_AT  DATETIME2    NOT NULL CONSTRAINT DF_URT_CREATED_AT DEFAULT GETUTCDATE(),
        URT_REPLACED_BY NVARCHAR(500) NULL,      -- token hash that replaced this one (rotation audit)
        URT_IP_ADDRESS  NVARCHAR(50)  NULL,
        URT_USER_AGENT  NVARCHAR(500) NULL
    );

    -- Index for fast token lookup
    CREATE NONCLUSTERED INDEX IX_USER_REFRESH_TOKEN_HASH
        ON [dbo].[USER_REFRESH_TOKEN] (URT_TOKEN_HASH)
        WHERE URT_IS_REVOKED = 0;

    -- Index for user-based queries (list sessions, revoke all)
    CREATE NONCLUSTERED INDEX IX_USER_REFRESH_TOKEN_UM_CODE
        ON [dbo].[USER_REFRESH_TOKEN] (URT_UM_CODE);

    PRINT 'USER_REFRESH_TOKEN table created.';
END
ELSE
BEGIN
    PRINT 'USER_REFRESH_TOKEN table already exists. Skipping.';
END
GO
