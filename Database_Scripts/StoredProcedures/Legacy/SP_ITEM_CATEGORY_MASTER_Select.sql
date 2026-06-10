CREATE OR ALTER PROCEDURE [dbo].[SP_ITEM_CATEGORY_MASTER_Select]


@I_CAT_CODE int,
@I_CAT_CM_COMP_ID int,
@I_CAT_NAME varchar(50),
@ES_DELETE bit,
@MODIFY bit,
@Type varchar(50),
@I_CAT_SHORTCLOSE bit
AS


if @Type='CHECKUPDATE'
begin

Select 

I_CAT_CODE,
I_CAT_CM_COMP_ID,
I_CAT_NAME,
ES_DELETE,
MODIFY,I_CAT_SHORTCLOSE

from ITEM_CATEGORY_MASTER

where 
( @I_CAT_CODE is null or @I_CAT_CODE != I_CAT_CODE ) and
( @I_CAT_NAME is null or @I_CAT_NAME = I_CAT_NAME ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE ORDER BY I_CAT_NAME

end
else
begin

Select 

I_CAT_CODE,
I_CAT_CM_COMP_ID,
I_CAT_NAME,
ES_DELETE,
MODIFY,ISNULL(I_CAT_SHORTCLOSE,0) AS I_CAT_SHORTCLOSE

from ITEM_CATEGORY_MASTER
where 
( @I_CAT_CODE is null or @I_CAT_CODE = I_CAT_CODE ) and
( @I_CAT_CM_COMP_ID is null or @I_CAT_CM_COMP_ID = I_CAT_CM_COMP_ID ) and
( @I_CAT_NAME is null or @I_CAT_NAME = I_CAT_NAME ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE  ORDER BY I_CAT_NAME


end