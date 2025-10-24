-- =============================================
-- Delete Customer Master
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_DeleteCustomerMaster]
    @Id INT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT OFF;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if customer exists
        IF NOT EXISTS (SELECT 1 FROM PARTY_MASTER WHERE P_CODE = @Id AND P_CM_COMP_ID = @CompanyId AND P_TYPE = 1)
        BEGIN
            THROW 50001, 'Customer not found or access denied.', 1;
        END
        
        -- Check if customer is used in INVOICE_MASTER
        IF EXISTS (SELECT 1 FROM INVOICE_MASTER WHERE INM_P_CODE = @Id)
        BEGIN
            THROW 50002, 'Cannot delete customer because it is being used in invoices.', 1;
        END
        
        -- Delete the customer
        DELETE FROM PARTY_MASTER 
        WHERE P_CODE = @Id AND P_CM_COMP_ID = @CompanyId AND P_TYPE = 1;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        THROW;
    END CATCH
END

