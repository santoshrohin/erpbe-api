CREATE PROCEDURE [dbo].[SP_CreateUser]
    @Username NVARCHAR(50),
    @Password NVARCHAR(255),
    @Name NVARCHAR(100),
    @Email NVARCHAR(100) = NULL,
    @CompanyId INT,
    @FinancialYearCode INT,
    @IsActive BIT = 1,
    @IsAdmin BIT = 0
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    INSERT INTO USER_MASTER (UM_USERNAME, UM_PASSWORD, UM_NAME, UM_EMAIL, UM_CM_ID, UM_LEVEL, IS_ACTIVE, UM_IS_ADMIN)
    VALUES (@Username, @Password, @Name, @Email, @CompanyId, 'User', @IsActive, @IsAdmin);

    SELECT SCOPE_IDENTITY() AS UserId;
END
