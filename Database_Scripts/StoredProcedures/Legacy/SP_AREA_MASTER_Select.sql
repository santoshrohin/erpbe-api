CREATE OR ALTER PROCEDURE [dbo].[SP_AREA_MASTER_Select]


@A_CODE int,
@A_U_CODE int,
@A_U_DATE Date ,
@A_CM_COMP_ID int,
@A_NO varchar(10),
@A_DESC varchar(50),
@ES_DELETE bit,
@MODIFY bit,
@TYPE VARCHAR(50)

AS

if @TYPE='CHECKUPDATE'
begin

Select 

A_CODE,
A_U_CODE,
A_U_DATE,
A_CM_COMP_ID,
A_NO,
A_DESC,
ES_DELETE,
MODIFY

from AREA_MASTER

where 
( @A_CODE is null or @A_CODE != A_CODE ) and
( @A_CM_COMP_ID is null or @A_CM_COMP_ID = A_CM_COMP_ID ) and
( @A_NO is null or @A_NO = A_NO )and 
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE 

end

else
begin

Select 

A_CODE ,
A_CM_COMP_ID ,
A_NO,
A_DESC ,
ES_DELETE ,
MODIFY 



from AREA_MASTER

where 
( @A_CODE is null or @A_CODE = A_CODE ) and

( @A_CM_COMP_ID is null or @A_CM_COMP_ID = A_CM_COMP_ID ) and
( @A_NO is null or @A_NO = A_NO ) and
--( @A_DESC is null or @A_DESC = A_DESC ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE ORDER BY A_DESC

end