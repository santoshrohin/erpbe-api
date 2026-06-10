-- ============================================================
-- ERP_ForceUnlock
-- Called by AdminLocksController when an admin force-unlocks a record.
-- Returns the number of rows unlocked (1 = success, 0 = not found/already unlocked).
--
-- To add a new module: add an ELSE IF block below.
-- No application code needs to change.
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_ForceUnlock]
    @Module   VARCHAR(50),
    @RecordId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Module = 'CustomerPO'
    BEGIN
        UPDATE CUSTPO_MASTER
        SET    MODIFY      = 0,
               MODIFY_TIME = NULL,
               MODIFY_BY   = NULL
        WHERE  CPOM_CODE = @RecordId;
    END

    ELSE IF @Module = 'DeliveryChallan'
    BEGIN
        UPDATE DELIVERY_CHALLAN_MASTER
        SET    MODIFY      = 0,
               MODIFY_TIME = NULL,
               MODIFY_BY   = NULL
        WHERE  DCM_CODE = @RecordId;
    END

    ELSE IF @Module = 'TaxInvoice'
    BEGIN
        UPDATE INVOICE_MASTER
        SET    MODIFY      = 0,
               MODIFY_TIME = NULL,
               MODIFY_BY   = NULL
        WHERE  INM_CODE  = @RecordId
          AND  INM_TYPE <> 'OutJWINM';
    END

    ELSE IF @Module = 'LabourChargeInvoice'
    BEGIN
        UPDATE INVOICE_MASTER
        SET    MODIFY      = 0,
               MODIFY_TIME = NULL,
               MODIFY_BY   = NULL
        WHERE  INM_CODE  = @RecordId
          AND  INM_TYPE  = 'OutJWINM';
    END

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
