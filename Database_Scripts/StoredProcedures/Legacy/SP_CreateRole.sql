CREATE OR ALTER PROCEDURE [dbo].[SP_CreateRole]
    @RoleName NVARCHAR(100),
    @RoleDescription NVARCHAR(500) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ROLES (RoleName, RoleDescription, IsActive)
    VALUES (@RoleName, @RoleDescription, @IsActive);

    SELECT SCOPE_IDENTITY() AS RoleId;
END