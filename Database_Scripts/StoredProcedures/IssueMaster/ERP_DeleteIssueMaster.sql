CREATE OR ALTER PROCEDURE [dbo].[ERP_DeleteIssueMaster]
    @IssueCode   INT,
    @CompanyCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ISSUE_MASTER
    SET    ES_DELETE = 1
    WHERE  IM_CODE    = @IssueCode
      AND  IM_COMP_ID = @CompanyCode
      AND  ES_DELETE  = 0;

    UPDATE ISSUE_MASTER_DETAIL
    SET    ES_DELETE = 1
    WHERE  IM_CODE = @IssueCode;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
