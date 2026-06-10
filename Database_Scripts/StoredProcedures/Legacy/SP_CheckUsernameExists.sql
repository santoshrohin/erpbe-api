CREATE OR ALTER PROCEDURE [dbo].[SP_CheckUsernameExists]
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM [USER_MASTER]
    WHERE [UM_USERNAME] = @Username AND [ES_DELETE] = 0
END