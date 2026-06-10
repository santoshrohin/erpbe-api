CREATE OR ALTER PROCEDURE [dbo].[SP_ADMIN_MASTER_INSERT]

@A_CM_COMP_ID int,
@A_NO varchar(10),
@A_DESC varchar(50)


AS

Insert into AREA_MASTER
(
A_CM_COMP_ID,
A_NO,
A_DESC

)
values
(
@A_CM_COMP_ID,
@A_NO,
@A_DESC

)