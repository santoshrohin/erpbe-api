-- =============================================
-- Stored Procedure: ERP_DeleteCustomerMaster
-- Description: Soft deletes a Customer Master record
-- =============================================
CREATE OR ALTER PROCEDURE ERP_DeleteCustomerMaster
    @Id INT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT OFF;

    UPDATE PARTY_MASTER
    SET 
        ES_DELETE = 1,
        ES_MODIFY_DATE = GETDATE()
    WHERE P_CODE = @Id 
        AND P_C_CODE = @CompanyId
        AND P_TYPE = 1;
END
GO

