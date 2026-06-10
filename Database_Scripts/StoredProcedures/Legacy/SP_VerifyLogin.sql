-- =============================================
-- Author:		<Sharad Bhosale>
-- Create date: <03 September 2012>
-- Modified Date:<03 September 2012>
-- Description:	<For Login>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_VerifyLogin]

@CompanyId varchar(50),
@UserName varchar(50),
@Password varchar(50),
@CompCode varchar(50)

AS
BEGIN
	
     SELECT * FROM USER_MASTER USERM,COMPANY_MASTER COMPANYM WHERE  USERM.UM_USERNAME = @UserName AND USERM.UM_PASSWORD = @Password AND USERM.ES_DELETE=0 and USERM.UM_CM_ID=COMPANYM.CM_ID and COMPANYM.CM_ID=@CompanyId and COMPANYM.CM_CODE=@CompCode 

 
END