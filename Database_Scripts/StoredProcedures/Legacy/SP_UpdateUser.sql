CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateUser]
    @UserId INT,
    @Name NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT OFF;

    UPDATE USER_MASTER 
    SET UM_NAME = ISNULL(@Name, UM_NAME),
        UM_EMAIL = ISNULL(@Email, UM_EMAIL),
        IS_ACTIVE = ISNULL(@IsActive, IS_ACTIVE)
    WHERE UM_CODE = @UserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END