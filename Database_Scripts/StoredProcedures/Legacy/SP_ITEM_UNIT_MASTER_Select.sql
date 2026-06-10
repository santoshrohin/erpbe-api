CREATE OR ALTER PROCEDURE [dbo].[SP_ITEM_UNIT_MASTER_Select]


@I_UOM_CODE int,
@I_UOM_CM_COMP_ID int,
@I_UOM_NAME varchar(50),
@I_UOM_DESC varchar(50),
@ES_DELETE bit,
@MODIFY bit,
@TYPE VARCHAR(50)

AS
if @TYPE='CHECKUPDATE'
begin
Select 

I_UOM_CODE,
I_UOM_CM_COMP_ID,
I_UOM_NAME,
I_UOM_DESC,
ES_DELETE,
MODIFY

from ITEM_UNIT_MASTER 

where 
( @I_UOM_CODE is null or @I_UOM_CODE != I_UOM_CODE ) and
( @I_UOM_CM_COMP_ID is null or @I_UOM_CM_COMP_ID = I_UOM_CM_COMP_ID ) and
( @I_UOM_NAME is null or @I_UOM_NAME = I_UOM_NAME ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE
end
else
begin
Select 

I_UOM_CODE,
I_UOM_CM_COMP_ID,
I_UOM_NAME,
I_UOM_DESC,
ES_DELETE,
MODIFY

from ITEM_UNIT_MASTER 

where 
( @I_UOM_CODE is null or @I_UOM_CODE = I_UOM_CODE ) and
( @I_UOM_CM_COMP_ID is null or @I_UOM_CM_COMP_ID = I_UOM_CM_COMP_ID ) and
( @I_UOM_NAME is null or @I_UOM_NAME = I_UOM_NAME ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE order by I_UOM_NAME,I_UOM_DESC
end