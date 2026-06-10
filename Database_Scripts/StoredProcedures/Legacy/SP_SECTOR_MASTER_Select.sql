CREATE OR ALTER PROCEDURE [dbo].[SP_SECTOR_MASTER_Select]


@SCT_CODE int,
@SCT_CM_COMP_ID int,
@SCT_DESC varchar(50),
@ES_DELETE bit,
@MODIFY bit,
@TYPE VARCHAR(50)

AS

if @TYPE='CHECKUPDATE'
begin

Select 

SCT_CODE,
SCT_CM_COMP_ID,
SCT_DESC,
ES_DELETE,
MODIFY

from SECTOR_MASTER

where 
( @SCT_CODE is null or @SCT_CODE != SCT_CODE ) and
( @SCT_CM_COMP_ID is null or @SCT_CM_COMP_ID = SCT_CM_COMP_ID ) and
( @SCT_DESC is null or @SCT_DESC = SCT_DESC ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE

end
else
begin

Select 

SCT_CODE,
SCT_CM_COMP_ID,
SCT_DESC,
ES_DELETE,
MODIFY

from SECTOR_MASTER

where 
( @SCT_CODE is null or @SCT_CODE = SCT_CODE ) and
( @SCT_CM_COMP_ID is null or @SCT_CM_COMP_ID = SCT_CM_COMP_ID ) and
( @SCT_DESC is null or @SCT_DESC = SCT_DESC ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE ORDER BY SCT_DESC

end