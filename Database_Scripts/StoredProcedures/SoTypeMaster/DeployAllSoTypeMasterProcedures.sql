-- =============================================
-- Master Deployment Script for SO Type Master Stored Procedures
-- Author:      ERP Development Team
-- Create date: 2025-10-24
-- Description: Deploys all SO Type Master stored procedures with ERP_ prefix
-- =============================================

USE [db_a2ea4b_sunv2];
GO

PRINT '';
PRINT '========================================';
PRINT 'SO Type Master Stored Procedures Deployment';
PRINT 'Starting deployment at: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- 1. Create SO Type Master
:r .\ERP_CreateSoTypeMaster.sql

-- 2. Update SO Type Master
:r .\ERP_UpdateSoTypeMaster.sql

-- 3. Delete SO Type Master
:r .\ERP_DeleteSoTypeMaster.sql

-- 4. Get SO Type Master by ID
:r .\ERP_GetSoTypeMasterById.sql

-- 5. Get SO Type Masters (with pagination)
:r .\ERP_GetSoTypeMasters.sql

-- 6. Get SO Type Master by Short Name
:r .\ERP_GetSoTypeMasterByShortName.sql

-- 7. Check Short Name Uniqueness
:r .\ERP_IsSoTypeShortNameUnique.sql

-- 8. Check SO Type Usage
:r .\ERP_CheckSoTypeUsage.sql

-- 9. Check Fixed Record
:r .\ERP_IsSoTypeFixedRecord.sql

PRINT '';
PRINT '========================================';
PRINT 'Deployment completed at: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT 'All SO Type Master stored procedures deployed successfully!';
PRINT '========================================';
PRINT '';
GO

