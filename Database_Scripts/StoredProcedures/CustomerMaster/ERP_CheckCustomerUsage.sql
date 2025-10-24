-- =============================================
-- Stored Procedure: ERP_CheckCustomerUsage
-- Description: Checks if a Customer Master record is used in any transactions
-- Returns: 1 if used, 0 if not used
-- Note: This checks common transaction tables (SO_MASTER, INVOICE_MASTER, etc.)
-- =============================================
CREATE OR ALTER PROCEDURE ERP_CheckCustomerUsage
    @Id INT
AS
BEGIN
    SET NOCOUNT OFF;

    DECLARE @Count INT = 0;

    -- Check if used in SO_MASTER (Sales Order)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SO_MASTER')
    BEGIN
        EXEC sp_executesql N'SELECT @Count = @Count + COUNT(*) FROM SO_MASTER WHERE SO_P_CODE = @Id', 
            N'@Id INT, @Count INT OUTPUT', @Id, @Count OUTPUT;
    END

    -- Check if used in INVOICE_MASTER (Invoice)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'INVOICE_MASTER')
    BEGIN
        EXEC sp_executesql N'SELECT @Count = @Count + COUNT(*) FROM INVOICE_MASTER WHERE INV_P_CODE = @Id', 
            N'@Id INT, @Count INT OUTPUT', @Id, @Count OUTPUT;
    END

    -- Check if used in CHALLAN_MASTER (Delivery Challan)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CHALLAN_MASTER')
    BEGIN
        EXEC sp_executesql N'SELECT @Count = @Count + COUNT(*) FROM CHALLAN_MASTER WHERE CH_P_CODE = @Id', 
            N'@Id INT, @Count INT OUTPUT', @Id, @Count OUTPUT;
    END

    -- Check if used in QUOTATION_MASTER (Quotation)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QUOTATION_MASTER')
    BEGIN
        EXEC sp_executesql N'SELECT @Count = @Count + COUNT(*) FROM QUOTATION_MASTER WHERE Q_P_CODE = @Id', 
            N'@Id INT, @Count INT OUTPUT', @Id, @Count OUTPUT;
    END

    IF @Count > 0
        SELECT 1 AS IsUsed; -- Used in transactions
    ELSE
        SELECT 0 AS IsUsed; -- Not used
END
GO

