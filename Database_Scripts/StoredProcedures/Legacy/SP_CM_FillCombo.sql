CREATE OR ALTER PROCEDURE [dbo].[SP_CM_FillCombo] --'Item_Master','I_NAME','I_CODE','ES_DELETE=0'

@TbName varchar(100),
@fname varchar(100),
@code varchar(50),
@cond varchar(MAX)

AS
BEGIN
	--select @fname,@code from @TbName where @cond
	exec('select ' +' distinct '  + @code +' , '+ @fname +' from ' + @TbName + ' where ' +' '+ @cond +' ')
END