CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateRemoveModifyLock] 
@Code int,
@TableCode varchar(50),
@TableName varchar(max),
@TableMod varchar(50)

AS
BEGIN
exec('update '+ @TableName +' set '+ @TableMod + ' =0 where '+ @TableCode +' = '+ @Code + '')

END