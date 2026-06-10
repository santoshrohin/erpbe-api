-- =============================================================================
-- ERP_CreateRefreshToken
-- Inserts a new refresh token record. Called after successful login or refresh.
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_CreateRefreshToken]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_CreateRefreshToken]
GO

CREATE PROCEDURE [dbo].[ERP_CreateRefreshToken]
    @UserCode    INT,
    @TokenHash   NVARCHAR(500),
    @ExpiresAt   DATETIME2,
    @IpAddress   NVARCHAR(50)  = NULL,
    @UserAgent   NVARCHAR(500) = NULL
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
