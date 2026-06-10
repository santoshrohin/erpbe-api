CREATE OR ALTER PROCEDURE [dbo].[SP_CM_Execute]

@Query varchar(MAX)

AS
BEGIN
	exec(@Query)
END