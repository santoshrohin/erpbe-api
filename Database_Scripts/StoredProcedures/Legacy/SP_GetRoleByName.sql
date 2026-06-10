CREATE OR ALTER PROCEDURE [dbo].[SP_GetRoleByName]
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RoleId as RoleId,
        [RoleName],
        RoleDescription,
        [IsActive]
    FROM [Roles]
    WHERE [RoleName] = @RoleName
END