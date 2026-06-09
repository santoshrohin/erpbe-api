IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ERP_DeleteCustomerPo')
    DROP PROCEDURE [dbo].[ERP_DeleteCustomerPo]
GO

CREATE PROCEDURE [dbo].[ERP_DeleteCustomerPo]
    @PoCode INT,
    @CompanyId INT
AS
BEGIN
    BEGIN TRY
        -- Block delete if PO is referenced by an Invoice (matches legacy: IND_CPOM_CODE check)
        IF EXISTS (
            SELECT 1 FROM INVOICE_DETAIL
            WHERE IND_CPOM_CODE = @PoCode
              AND EXISTS (
                  SELECT 1 FROM INVOICE_MASTER
                  WHERE INM_CODE = IND_INM_CODE AND ES_DELETE = 0
              )
        )
        BEGIN
            RAISERROR('Record cannot be deleted: it is referenced by one or more Tax Invoices.', 16, 1);
            RETURN;
        END

        -- Block delete if PO is referenced by an Inward of type IWIFP (matches legacy: IWM_TYPE='IWIFP' check)
        IF EXISTS (
            SELECT 1 FROM INWARD_DETAIL
            WHERE IWD_CPOM_CODE = @PoCode
              AND EXISTS (
                  SELECT 1 FROM INWARD_MASTER
                  WHERE IWM_CODE = IWD_IWM_CODE AND INWARD_MASTER.ES_DELETE = 0 AND IWM_TYPE = 'IWIFP'
              )
        )
        BEGIN
            RAISERROR('Record cannot be deleted: it is referenced by one or more Inward entries.', 16, 1);
            RETURN;
        END

        -- Soft delete
        UPDATE CUSTPO_MASTER
        SET ES_DELETE = 1
        WHERE
            CPOM_CODE = @PoCode
            AND CPOM_CM_COMP_ID = @CompanyId
            AND ES_DELETE = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
