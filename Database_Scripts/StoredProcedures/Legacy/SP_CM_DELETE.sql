CREATE OR ALTER PROCEDURE [dbo].[SP_CM_DELETE] --'-2147483647','IWM_CODE',1,'ES_DELETE','INWARD_MASTER'

@PK_CODE int,
@PK_Field varchar(30),
@ES_DELETE int,
@DELETE VARCHAR(10),
@TABLE_NAME varchar(50)


AS
BEGIN
	--select @fname,@code from @TbName where @cond
	exec('Update ' +''  + @TABLE_NAME +' set  '+ @DELETE +' = ' + @ES_DELETE + ' where ' +' '+ @PK_Field +' = ' +@PK_CODE +'')
END