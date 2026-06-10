CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateRole]
    @RoleId INT,
    @RoleName NVARCHAR(100),
    @RoleDescription NVARCHAR(500) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT OFF;

    UPDATE ROLES 
    SET RoleName = @RoleName,
        RoleDescription = @RoleDescription,
        IsActive = @IsActive
    WHERE RoleId = @RoleId;

    SELECT @@ROWCOUNT AS RowsAffected;
END