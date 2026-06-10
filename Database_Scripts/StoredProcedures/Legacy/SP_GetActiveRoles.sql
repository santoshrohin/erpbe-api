CREATE OR ALTER PROCEDURE [dbo].[SP_GetActiveRoles]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RoleId as RoleId,
        [RoleName],
        RoleDescription,
        [IsActive]
    FROM [Roles]
    WHERE [IsActive] = 1
    ORDER BY [RoleName]
END