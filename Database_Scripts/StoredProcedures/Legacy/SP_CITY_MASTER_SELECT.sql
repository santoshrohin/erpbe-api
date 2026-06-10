CREATE OR ALTER PROCEDURE [dbo].[SP_CITY_MASTER_SELECT]

@UM_CODE int,
@UM_CM_ID int,
@UM_USERNAME varchar(50),
@UM_PASSWORD varchar(max),
@UM_LEVEL varchar(50),
@UM_LASTLOGIN_DATETIME datetime,
@UM_IP_ADDRESS varchar(50),
@IS_ACTIVE bit,
@UM_EMAIL_SEND bit,
@UM_LOGIN_FLAG bit,
@UM_IS_ADMIN bit,
@UM_NAME varchar(50),
@UM_EMAIL varchar(100),
@ES_DELETE bit,
@MODIFY bit,
@TYPE VARCHAR(50)

AS
if @TYPE='CHECKUPDATE'
begin
Select 
UM_CODE,
UM_CM_ID,
UM_USERNAME,
UM_PASSWORD,
UM_LEVEL,
UM_LASTLOGIN_DATETIME,
UM_IP_ADDRESS,
IS_ACTIVE,
UM_EMAIL_SEND,
UM_LOGIN_FLAG,
UM_IS_ADMIN,
UM_NAME,
UM_EMAIL,

ES_DELETE,
MODIFY

from USER_MASTER

where 
( @UM_CODE is null or @UM_CODE != UM_CODE ) and
( @UM_CM_ID is null or @UM_CM_ID != UM_CM_ID ) and
( @UM_USERNAME is null or @UM_USERNAME != UM_USERNAME ) and
( @UM_PASSWORD is null or @UM_PASSWORD != UM_PASSWORD ) and
( @UM_LEVEL is null or @UM_LEVEL != UM_LEVEL ) and
( @UM_LASTLOGIN_DATETIME is null or @UM_LASTLOGIN_DATETIME != UM_LASTLOGIN_DATETIME ) and
( @UM_IP_ADDRESS is null or @UM_IP_ADDRESS != UM_IP_ADDRESS ) and
( @IS_ACTIVE is null or @IS_ACTIVE != IS_ACTIVE ) and
( @UM_EMAIL_SEND is null or @UM_EMAIL_SEND != UM_EMAIL_SEND ) and
( @UM_LOGIN_FLAG is null or @UM_LOGIN_FLAG != UM_LOGIN_FLAG ) and
( @UM_IS_ADMIN is null or @UM_IS_ADMIN != UM_IS_ADMIN ) and
( @UM_NAME is null or @UM_NAME != UM_NAME ) and
( @UM_EMAIL is null or @UM_EMAIL != UM_EMAIL ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE
end
else
begin
Select 

UM_CODE,
UM_CM_ID,
UM_USERNAME,
UM_PASSWORD,
UM_LEVEL,
UM_LASTLOGIN_DATETIME,
UM_IP_ADDRESS,
IS_ACTIVE,
UM_EMAIL_SEND,
UM_LOGIN_FLAG,
UM_IS_ADMIN,
UM_NAME,
UM_EMAIL,

ES_DELETE,
MODIFY

from USER_MASTER


where 
( @UM_CODE is null or @UM_CODE = UM_CODE ) and
( @UM_CM_ID is null or @UM_CM_ID != UM_CM_ID ) and
( @UM_USERNAME is null or @UM_USERNAME != UM_USERNAME ) and
( @UM_PASSWORD is null or @UM_PASSWORD != UM_PASSWORD ) and
( @UM_LEVEL is null or @UM_LEVEL != UM_LEVEL ) and
( @UM_LASTLOGIN_DATETIME is null or @UM_LASTLOGIN_DATETIME != UM_LASTLOGIN_DATETIME ) and
( @UM_IP_ADDRESS is null or @UM_IP_ADDRESS != UM_IP_ADDRESS ) and
( @IS_ACTIVE is null or @IS_ACTIVE != IS_ACTIVE ) and
( @UM_EMAIL_SEND is null or @UM_EMAIL_SEND != UM_EMAIL_SEND ) and
( @UM_LOGIN_FLAG is null or @UM_LOGIN_FLAG != UM_LOGIN_FLAG ) and
( @UM_IS_ADMIN is null or @UM_IS_ADMIN != UM_IS_ADMIN ) and
( @UM_NAME is null or @UM_NAME != UM_NAME ) and
( @UM_EMAIL is null or @UM_EMAIL != UM_EMAIL ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE
end