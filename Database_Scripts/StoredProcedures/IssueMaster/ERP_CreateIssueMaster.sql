CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateIssueMaster]
    @CompanyCode    INT,
    @IssueDate      SMALLDATETIME = NULL,
    @IssueType      VARCHAR(10)   = NULL,
    @MaterialReqNo  DECIMAL(18,0) = NULL,
    @IssuedBy       VARCHAR(100)  = NULL,
    @RequestedBy    VARCHAR(100)  = NULL,
    @UserMasterCode INT           = NULL,
    @FromStore      INT           = -2147483647
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextNo DECIMAL(18,0);
    SELECT @NextNo = ISNULL(MAX(IM_NO), 0) + 1
    FROM ISSUE_MASTER
    WHERE IM_COMP_ID = @CompanyCode;

    INSERT INTO ISSUE_MASTER
        (IM_NO, IM_DATE, IM_TYPE, IM_MATERIAL_REQ, IM_ISSUEBY, IM_REQBY,
         IM_UM_CODE, IM_FROM_STORE, IM_COMP_ID, ES_DELETE)
    VALUES
        (@NextNo, @IssueDate, @IssueType, @MaterialReqNo, @IssuedBy, @RequestedBy,
         @UserMasterCode, @FromStore, @CompanyCode, 0);

    SELECT SCOPE_IDENTITY() AS IssueCode, @NextNo AS IssueNumber;
END
GO
