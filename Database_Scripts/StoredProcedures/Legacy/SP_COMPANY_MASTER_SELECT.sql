CREATE OR ALTER PROCEDURE [dbo].[SP_COMPANY_MASTER_SELECT]


@CM_ID int
AS
Select *
from COMPANY_MASTER

where 
 @CM_ID is null or @CM_ID = CM_ID