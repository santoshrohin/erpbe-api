CREATE OR ALTER PROCEDURE [dbo].[ERP_UpdateIssueMaster]
    @IssueCode      INT,
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

    UPDATE ISSUE_MASTER
    SET    IM_DATE         = @IssueDate,
           IM_TYPE         = @IssueType,
           IM_MATERIAL_REQ = @MaterialReqNo,
           IM_ISSUEBY      = @IssuedBy,
           IM_REQBY        = @RequestedBy,
           IM_UM_CODE      = @UserMasterCode,
           IM_FROM_STORE   = @FromStore
    WHERE  IM_CODE    = @IssueCode
      AND  IM_COMP_ID = @CompanyCode
      AND  ES_DELETE  = 0;

    -- Soft-delete old details; caller re-inserts via ERP_CreateIssueMasterDetail
    UPDATE ISSUE_MASTER_DETAIL
    SET    ES_DELETE = 1
    WHERE  IM_CODE = @IssueCode;
END
GO
