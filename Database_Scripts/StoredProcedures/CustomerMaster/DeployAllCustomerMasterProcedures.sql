-- =============================================
-- Deploy All Customer Master Stored Procedures
-- =============================================
-- This script deploys all Customer Master related stored procedures

:r .\ERP_CreateCustomerMaster.sql
:r .\ERP_UpdateCustomerMaster.sql
:r .\ERP_DeleteCustomerMaster.sql
:r .\ERP_GetCustomerMasterById.sql
:r .\ERP_GetCustomerMasters.sql
:r .\ERP_IsPartyNameUnique.sql
:r .\ERP_IsAbbreviationUnique.sql
:r .\ERP_IsCustomerModified.sql
:r .\ERP_CheckCustomerUsage.sql

PRINT 'All Customer Master stored procedures deployed successfully!';
GO

