CREATE OR ALTER PROCEDURE [dbo].[SP_GetRoleById]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        RoleId,
        RoleName,
        RoleDescription AS Description,
        IsActive
    FROM ROLES
    WHERE RoleId = @RoleId;
END