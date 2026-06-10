-- =============================================================================
-- ERP_WriteActivityLog
-- Inserts an activity entry into LOG_MASTER, matching the legacy
-- CommonClasses.WriteLog() pattern exactly.
--
-- Called by the modern API whenever a user creates, updates, or deletes a record.
-- Provides cross-system audit trail — same LOG_MASTER table both systems use.
--
-- Legacy equivalent:
--   CommonClasses.WriteLog(FormName, Event, DocName, DocNo, DocCode,
--                          CompId, UserName, UserCode)
-- =============================================================================
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[ERP_WriteActivityLog]')
      AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[ERP_WriteActivityLog]
GO

CREATE PROCEDURE [dbo].[ERP_WriteActivityLog]
    @CompanyId   INT,
    @Source      NVARCHAR(100),   -- module/controller name (e.g. 'CustomerMaster')
    @Event       NVARCHAR(50),    -- 'INSERT', 'UPDATE', 'DELETE'
    @DocName     NVARCHAR(100),   -- document type label (e.g. 'Customer Master')
    @DocNo       NVARCHAR(50),    -- document number / code string
    @DocCode     INT,             -- primary key of the affected record
    @UserName    NVARCHAR(100),
    @UserCode    INT,
    @IpAddress   NVARCHAR(50)  = NULL,
    @CompanyCode INT           = NULL   -- optional; maps to LG_CM_CODE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LOG_MASTER
        (LG_CM_COMP_ID, LG_CM_CODE, LG_DATE,  LG_SOURCE, LG_EVENT,
         LG_COMP_NAME,  LG_DOC_NO,  LG_DOC_NAME, LG_DOC_CODE,
         LG_U_NAME,     LG_U_CODE,  LG_IP_ADDRESS)
    VALUES
        (@CompanyId, @CompanyCode, GETDATE(), @Source, @Event,
         HOST_NAME(),   @DocNo,     @DocName,    @DocCode,
         @UserName,     @UserCode,  @IpAddress);
END
GO
