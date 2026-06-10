CREATE OR ALTER PROCEDURE [dbo].[SP_AREA_MASTER_Delete]

@A_CODE int

AS

Update 
AREA_MASTER

set ES_DELETE = 1

where A_CODE=@A_CODE