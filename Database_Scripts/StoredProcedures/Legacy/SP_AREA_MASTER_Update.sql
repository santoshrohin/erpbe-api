CREATE OR ALTER PROCEDURE [dbo].[SP_AREA_MASTER_Update]


@A_CODE   int,
--@A_U_CODE    int,
--@A_U_DATE    varchar(50),
@A_CM_COMP_ID Varchar(50),
@A_NO Varchar(50),
@A_DESC Varchar(50)
AS

Update AREA_MASTER

Set
	
	A_CM_COMP_ID = @A_CM_COMP_ID,
	A_NO = @A_NO,
	A_DESC = @A_DESC
	
	
Where

A_CODE = @A_CODE