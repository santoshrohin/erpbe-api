-- =============================================
-- Deploy All Customer Type Master Stored Procedures
-- =============================================
-- This script deploys all stored procedures for the CustomerTypeMaster module
-- Execute this script on the target database to create/update all procedures
-- =============================================

PRINT 'Starting deployment of Customer Type Master stored procedures...';
GO

-- 1. Create Customer Type Master
PRINT 'Deploying ERP_CreateCustomerTypeMaster...';
GO
:r .\ERP_CreateCustomerTypeMaster.sql
GO

-- 2. Update Customer Type Master
PRINT 'Deploying ERP_UpdateCustomerTypeMaster...';
GO
:r .\ERP_UpdateCustomerTypeMaster.sql
GO

-- 3. Delete Customer Type Master
PRINT 'Deploying ERP_DeleteCustomerTypeMaster...';
GO
:r .\ERP_DeleteCustomerTypeMaster.sql
GO

-- 4. Get Customer Type Master By ID
PRINT 'Deploying ERP_GetCustomerTypeMasterById...';
GO
:r .\ERP_GetCustomerTypeMasterById.sql
GO

-- 5. Get All Customer Type Masters (with pagination, filtering, sorting)
PRINT 'Deploying ERP_GetCustomerTypeMasters...';
GO
:r .\ERP_GetCustomerTypeMasters.sql
GO

-- 6. Get Customer Type Master By Type Code
PRINT 'Deploying ERP_GetCustomerTypeMasterByTypeCode...';
GO
:r .\ERP_GetCustomerTypeMasterByTypeCode.sql
GO

-- 7. Check Type Code Uniqueness
PRINT 'Deploying ERP_IsTypeCodeUnique...';
GO
:r .\ERP_IsTypeCodeUnique.sql
GO

-- 8. Check Customer Type Usage in Party Master
PRINT 'Deploying ERP_CheckCustomerTypeUsage...';
GO
:r .\ERP_CheckCustomerTypeUsage.sql
GO

-- 9. Check if Customer Type is Modified by Another User
PRINT 'Deploying ERP_IsCustomerTypeModified...';
GO
:r .\ERP_IsCustomerTypeModified.sql
GO

PRINT 'Customer Type Master stored procedures deployment completed successfully!';
GO

