CREATE OR ALTER PROCEDURE [dbo].[SP_DeleteRole] --20
    @RoleId INT
AS
BEGIN
    SET NOCOUNT OFF;

    DELETE FROM ROLES WHERE RoleId = @RoleId;

    SELECT @@ROWCOUNT AS RowsAffected;
END