CREATE OR ALTER PROCEDURE [dbo].[SP_CheckRoleExists]
    @RoleName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM [Roles]
    WHERE [RoleName] = @RoleName
END