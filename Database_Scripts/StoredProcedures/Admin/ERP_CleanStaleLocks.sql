-- ============================================================
-- ERP_CleanStaleLocks
-- Called by StaleLockCleanupService every 15 minutes.
-- Clears MODIFY = 1 rows whose MODIFY_TIME has exceeded the
-- timeout window (or whose MODIFY_TIME is NULL from before the
-- timestamp migration).
--
-- To add a new module: add one UPDATE block below following the
-- same pattern. No application code needs to change.
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_CleanStaleLocks]
    @TimeoutMinutes INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Cutoff DATETIME = DATEADD(MINUTE, -@TimeoutMinutes, GETDATE());

    -- Customer PO
    UPDATE CUSTPO_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  MODIFY = 1
      AND  (MODIFY_TIME IS NULL OR MODIFY_TIME < @Cutoff);

    -- Delivery Challan
    UPDATE DELIVERY_CHALLAN_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  MODIFY = 1
      AND  (MODIFY_TIME IS NULL OR MODIFY_TIME < @Cutoff);

    -- Tax Invoice  (INVOICE_MASTER rows that are NOT LCI)
    UPDATE INVOICE_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  MODIFY    = 1
      AND  INM_TYPE <> 'OutJWINM'
      AND  (MODIFY_TIME IS NULL OR MODIFY_TIME < @Cutoff);

    -- Labour Charge Invoice  (INVOICE_MASTER rows that ARE LCI)
    UPDATE INVOICE_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  MODIFY   = 1
      AND  INM_TYPE = 'OutJWINM'
      AND  (MODIFY_TIME IS NULL OR MODIFY_TIME < @Cutoff);
END
GO
