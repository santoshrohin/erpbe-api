CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateUserRights] 
@UR_UM_CODE int,
@UR_SM_CODE int,
@UR_RIGHTS varchar(50)

AS
BEGIN

if exists(Select * From USER_RIGHT Where [UR_UM_CODE] = @UR_UM_CODE and [UR_SM_CODE] = @UR_SM_CODE)
Begin

UPDATE [USER_RIGHT]
   SET [UR_UM_CODE] = @UR_UM_CODE
      ,[UR_SM_CODE] = @UR_SM_CODE
      ,[UR_RIGHTS] = @UR_RIGHTS
 WHERE [UR_UM_CODE] = @UR_UM_CODE and [UR_SM_CODE] = @UR_SM_CODE
End

Else

Begin
INSERT INTO [USER_RIGHT]
           ([UR_UM_CODE]
           ,[UR_SM_CODE]
           ,[UR_RIGHTS])
     VALUES
           (@UR_UM_CODE,@UR_SM_CODE,@UR_RIGHTS)
End

END