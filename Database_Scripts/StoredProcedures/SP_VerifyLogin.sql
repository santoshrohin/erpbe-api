IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_VerifyLogin]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_VerifyLogin]
GO

CREATE PROCEDURE [dbo].[SP_VerifyLogin]
    @UserName NVARCHAR(50),
    @Password NVARCHAR(255),
    @CompanyId NVARCHAR(10),
    @CompCode NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        um.UM_CM_ID,
        cm.CM_CODE,
        cm.CM_NAME,
        cm.CM_EMAILID,
        um.UM_USERNAME,
        um.UM_NAME,
        um.UM_EMAIL
    FROM USER_MASTER um
    INNER JOIN COMPANY_MASTER cm ON um.UM_CM_ID = cm.CM_ID
    WHERE um.UM_USERNAME = @UserName
      AND um.UM_PASSWORD = @Password
      AND um.UM_CM_ID = CAST(@CompanyId AS INT)
      AND cm.CM_CODE = CAST(@CompCode AS INT)
      AND um.IS_ACTIVE = 1
      AND um.ES_DELETE = 0
      AND cm.CM_ACTIVE_IND = 1;
END
GO

