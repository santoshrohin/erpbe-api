CREATE OR ALTER PROCEDURE [dbo].[SP_STATE_MASTER_SELECT]


@SM_CODE int,
@SM_CM_COMP_ID int,
@SM_NAME varchar(50),
@SM_COUNTRY_CODE int,
@ES_DELETE bit,
@MODIFY bit,
@TYPE VARCHAR(50)

AS
if @TYPE='CHECKUPDATE'
begin
Select 

SM_CODE,
SM_CM_COMP_ID,
SM_NAME,
SM_COUNTRY_CODE,
ES_DELETE,
MODIFY,
SM_STATE_CODE

from STATE_MASTER

where 
( @SM_CODE is null or @SM_CODE != SM_CODE ) and
( @SM_CM_COMP_ID is null or @SM_CM_COMP_ID = SM_CM_COMP_ID ) and
( @SM_NAME is null or @SM_NAME = SM_NAME ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE
end
else
begin
Select 

SM_CODE,
SM_CM_COMP_ID,
SM_NAME,
SM_COUNTRY_CODE,
ES_DELETE,
MODIFY,
SM_STATE_CODE

from STATE_MASTER

where 
( @SM_CODE is null or @SM_CODE = SM_CODE ) and
( @SM_CM_COMP_ID is null or @SM_CM_COMP_ID = SM_CM_COMP_ID ) and
( @SM_NAME is null or @SM_NAME = SM_NAME ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE
end