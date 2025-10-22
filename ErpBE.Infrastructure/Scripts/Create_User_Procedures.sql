USE [db_a2ea4b_sunv2]
GO

-- Create SP_CreateUser stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CreateUser')
    DROP PROCEDURE [dbo].[SP_CreateUser]
GO

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
    SET NOCOUNT ON;

    INSERT INTO USER_MASTER (UM_USERNAME, UM_PASSWORD, UM_NAME, UM_EMAIL, UM_CM_ID, UM_LEVEL, IS_ACTIVE, UM_IS_ADMIN)
    VALUES (@Username, @Password, @Name, @Email, @CompanyId, 'User', @IsActive, @IsAdmin);

    SELECT SCOPE_IDENTITY() AS UserId;
END
GO

-- Create SP_UpdateUser stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_UpdateUser')
    DROP PROCEDURE [dbo].[SP_UpdateUser]
GO

CREATE PROCEDURE [dbo].[SP_UpdateUser]
    @UserId INT,
    @Name NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE USER_MASTER 
    SET UM_NAME = ISNULL(@Name, UM_NAME),
        UM_EMAIL = ISNULL(@Email, UM_EMAIL),
        IS_ACTIVE = ISNULL(@IsActive, IS_ACTIVE)
    WHERE UM_CODE = @UserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Create SP_DeleteUser stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_DeleteUser')
    DROP PROCEDURE [dbo].[SP_DeleteUser]
GO

CREATE PROCEDURE [dbo].[SP_DeleteUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE USER_MASTER 
    SET ES_DELETE = 1
    WHERE UM_CODE = @UserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Create SP_UserExists stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_UserExists')
    DROP PROCEDURE [dbo].[SP_UserExists]
GO

CREATE PROCEDURE [dbo].[SP_UserExists]
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = @Username) THEN 1 ELSE 0 END AS UserExists;
END
GO

PRINT 'User management stored procedures created successfully!';
