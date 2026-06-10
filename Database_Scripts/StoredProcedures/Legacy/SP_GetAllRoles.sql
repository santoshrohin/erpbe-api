CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllRoles]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RoleId as RoleId,
        [RoleName],
        RoleDescription,
        [IsActive]
    FROM [Roles]
    ORDER BY [RoleName]
END