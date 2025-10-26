-- =============================================
-- Script to deploy all corrected stored procedures
-- This fixes the SET NOCOUNT OFF issue that was causing
-- "no records found" errors in API endpoints
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT '========================================';
PRINT 'Deploying Fixed Stored Procedures';
PRINT 'Fixing SET NOCOUNT OFF -> SET NOCOUNT ON';
PRINT '========================================';
PRINT '';

-- List of critical stored procedures to deploy immediately
-- You can run each file individually or all together

-- TaxInvoice Critical SPs
PRINT 'Deploying TaxInvoice stored procedures...';
:r Database_Scripts\StoredProcedures\TaxInvoice\ERP_GetAllTaxInvoices.sql
:r Database_Scripts\StoredProcedures\TaxInvoice\ERP_GetTaxInvoiceById.sql
:r Database_Scripts\StoredProcedures\TaxInvoice\ERP_GetTaxInvoicePrintData_FINAL.sql
PRINT 'TaxInvoice SPs deployed.';
PRINT '';

-- CustomerPO Critical SPs
PRINT 'Deploying CustomerPO stored procedures...';
:r Database_Scripts\StoredProcedures\CustomerPo\ERP_GetCustomerPoById.sql
:r Database_Scripts\StoredProcedures\CustomerPo\ERP_GetAllCustomerPos.sql
PRINT 'CustomerPO SPs deployed.';
PRINT '';

-- CustomerMaster SPs  
PRINT 'Deploying CustomerMaster stored procedures...';
:r Database_Scripts\StoredProcedures\CustomerMaster\ERP_GetCustomerMasters.sql
:r Database_Scripts\StoredProcedures\CustomerMaster\ERP_GetCustomerMasterById.sql
PRINT 'CustomerMaster SPs deployed.';
PRINT '';

PRINT '========================================';
PRINT 'DEPLOYMENT COMPLETE';
PRINT 'All stored procedures now use SET NOCOUNT ON';
PRINT 'This should fix the "no records found" issue';
PRINT '========================================';
GO

