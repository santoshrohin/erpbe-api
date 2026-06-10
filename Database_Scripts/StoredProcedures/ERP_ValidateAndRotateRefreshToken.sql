-- =============================================================================
-- ERP_ValidateAndRotateRefreshToken
-- Validates an incoming refresh token hash and atomically rotates it.
-- Returns the user code if valid, nothing if expired/revoked/not found.
--
-- Rotation: marks old token as replaced, does NOT issue new one here.
-- New token is created by C# calling ERP_CreateRefreshToken.
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_ValidateAndRotateRefreshToken]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_ValidateAndRotateRefreshToken]
GO

CREATE PROCEDURE [dbo].[ERP_ValidateAndRotateRefreshToken]
    @TokenHash     NVARCHAR(500),
    @NewTokenHash  NVARCHAR(500)   -- hash of the replacement token
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @UserCode   INT;
        DECLARE @TokenCode  INT;

        -- Find valid (non-revoked, non-expired) token
        SELECT
            @UserCode  = URT_UM_CODE,
            @TokenCode = URT_CODE
        FROM USER_REFRESH_TOKEN
        WHERE URT_TOKEN_HASH = @TokenHash
          AND URT_IS_REVOKED  = 0
          AND URT_EXPIRES_AT  > GETUTCDATE();

        IF @UserCode IS NULL
        BEGIN
            -- Token not found, expired, or revoked
            ROLLBACK TRANSACTION;
            SELECT NULL AS UserCode;
            RETURN;
        END

        -- Mark old token as used/rotated
        UPDATE USER_REFRESH_TOKEN
        SET    URT_IS_REVOKED  = 1,
               URT_REPLACED_BY = @NewTokenHash
        WHERE  URT_CODE = @TokenCode;

        COMMIT TRANSACTION;

        -- Return user code for JWT re-generation
        SELECT @UserCode AS UserCode;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
