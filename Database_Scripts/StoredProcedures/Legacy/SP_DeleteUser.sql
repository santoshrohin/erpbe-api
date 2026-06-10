CREATE OR ALTER PROCEDURE [dbo].[SP_DeleteUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT off;

    UPDATE USER_MASTER 
    SET ES_DELETE = 1
    WHERE UM_CODE = @UserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END