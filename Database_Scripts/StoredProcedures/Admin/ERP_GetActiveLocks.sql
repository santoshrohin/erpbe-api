-- ============================================================
-- ERP_GetActiveLocks
-- Returns all currently held locks across all modules for the
-- admin lock-management screen.
--
-- To add a new module: add one UNION ALL block below.
-- No application code needs to change.
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetActiveLocks]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        'CustomerPO'           AS Module,
        CPOM_CODE              AS RecordId,
        ISNULL(CPOM_PONO, '')  AS RecordLabel,
        MODIFY_BY              AS LockedByUserId,
        MODIFY_TIME            AS LockedAt,
        DATEDIFF(MINUTE, MODIFY_TIME, GETDATE()) AS MinutesHeld
    FROM  CUSTPO_MASTER
    WHERE MODIFY = 1

    UNION ALL

    SELECT
        'DeliveryChallan'              AS Module,
        DCM_CODE                       AS RecordId,
        ISNULL(CAST(DCM_NO AS VARCHAR(50)), '') AS RecordLabel,
        MODIFY_BY,
        MODIFY_TIME,
        DATEDIFF(MINUTE, MODIFY_TIME, GETDATE())
    FROM  DELIVERY_CHALLAN_MASTER
    WHERE MODIFY = 1

    UNION ALL

    SELECT
        'TaxInvoice'                   AS Module,
        INM_CODE                       AS RecordId,
        ISNULL(CAST(INM_NO AS VARCHAR(50)), '') AS RecordLabel,
        MODIFY_BY,
        MODIFY_TIME,
        DATEDIFF(MINUTE, MODIFY_TIME, GETDATE())
    FROM  INVOICE_MASTER
    WHERE MODIFY    = 1
      AND INM_TYPE <> 'OutJWINM'

    UNION ALL

    SELECT
        'LabourChargeInvoice'          AS Module,
        INM_CODE                       AS RecordId,
        ISNULL(CAST(INM_NO AS VARCHAR(50)), '') AS RecordLabel,
        MODIFY_BY,
        MODIFY_TIME,
        DATEDIFF(MINUTE, MODIFY_TIME, GETDATE())
    FROM  INVOICE_MASTER
    WHERE MODIFY   = 1
      AND INM_TYPE = 'OutJWINM'

    ORDER BY LockedAt ASC;
END
GO
