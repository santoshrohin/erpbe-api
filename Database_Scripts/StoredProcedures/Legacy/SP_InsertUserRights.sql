CREATE OR ALTER PROCEDURE [dbo].[SP_InsertUserRights] 
@UR_UM_CODE int,
@UR_SM_CODE int,
@UR_RIGHTS varchar(50)

AS
BEGIN

INSERT INTO [dbo].[USER_RIGHT]
           ([UR_UM_CODE]
           ,[UR_SM_CODE]
           ,[UR_RIGHTS])
     VALUES
           (@UR_UM_CODE,@UR_SM_CODE,@UR_RIGHTS)

END