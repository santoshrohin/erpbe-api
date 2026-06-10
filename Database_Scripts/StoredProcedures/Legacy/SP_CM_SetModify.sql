CREATE OR ALTER PROCEDURE [dbo].[SP_CM_SetModify]

--@TbName varchar(500),
--@fname varchar(50),
--@code varchar(50),
@TableName varchar(150),
@ModField varchar(50),
@CodeField varchar(50),
@codeVal int

AS
BEGIN
exec('update '+ @TableName +' set '+ @ModField + ' =1 where '+ @CodeField +' = '+ @codeVal + '')
	--exec(@Query)
END