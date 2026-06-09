IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ERP_CheckPoUsedInWorkOrder')
    DROP PROCEDURE [dbo].[ERP_CheckPoUsedInWorkOrder]
GO

-- Returns 1 if the Customer PO is referenced by at least one live Work Order (ES_DELETE = 0).
-- Mirrors legacy ViewCustomerPO.aspx.cs: blocks MODIFY when a Work Order references the PO.
CREATE PROCEDURE [dbo].[ERP_CheckPoUsedInWorkOrder]
    @PoCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1 FROM WORK_ORDER_MASTER
            WHERE WO_CPOM_CODE = @PoCode
              AND ES_DELETE = 0
        ) THEN 1 ELSE 0 END
    AS BIT) AS IsUsed;
END
GO
