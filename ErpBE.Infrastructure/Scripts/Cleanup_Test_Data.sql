/*
    Cleanup Test Data
    This script removes all test data created during integration tests
    Run this after tests complete or when cleaning up
*/

PRINT 'Starting Test Data Cleanup...';
PRINT '';

-- Step 1: Get TestUser ID
DECLARE @TestUserId INT;

SELECT @TestUserId = UM_CODE 
FROM USER_MASTER 
WHERE UM_USERNAME = 'TestUser';

IF @TestUserId IS NULL
BEGIN
    PRINT 'TestUser not found. No cleanup needed.';
    RETURN;
END

PRINT 'Found TestUser with ID: ' + CAST(@TestUserId AS VARCHAR(10));
PRINT '';

-- Step 2: Delete UnitMaster test data
-- Delete units created by TestUser or with test names
DECLARE @DeletedUnits INT = 0;

DELETE FROM ITEM_UNIT_MASTER 
WHERE I_UOM_CR_USER = @TestUserId 
   OR I_UOM_NAME LIKE 'TEST_%'
   OR I_UOM_NAME LIKE '%_TEST'
   OR I_UOM_DESC LIKE '%Test%';

SET @DeletedUnits = @@ROWCOUNT;
PRINT 'Deleted ' + CAST(@DeletedUnits AS VARCHAR(10)) + ' test unit records.';

-- Step 3: Delete Audit Trail entries for test data
DECLARE @DeletedAudits INT = 0;

DELETE FROM AUDIT_TRAIL 
WHERE CREATED_BY = 'TestUser'
   OR (TABLE_NAME = 'ITEM_UNIT_MASTER' AND RECORD_ID NOT IN (SELECT I_UOM_CODE FROM ITEM_UNIT_MASTER));

SET @DeletedAudits = @@ROWCOUNT;
PRINT 'Deleted ' + CAST(@DeletedAudits AS VARCHAR(10)) + ' audit trail records.';

-- Step 4: Summary
PRINT '';
PRINT '=================================================';
PRINT 'Test Data Cleanup Summary';
PRINT '=================================================';
PRINT 'Unit Masters Deleted: ' + CAST(@DeletedUnits AS VARCHAR(10));
PRINT 'Audit Records Deleted: ' + CAST(@DeletedAudits AS VARCHAR(10));
PRINT '=================================================';
PRINT '';
PRINT 'NOTE: TestUser account is NOT deleted.';
PRINT 'To delete TestUser, run: DELETE FROM UserRoles WHERE UserId = ' + CAST(@TestUserId AS VARCHAR(10)) + '; DELETE FROM USER_MASTER WHERE UM_CODE = ' + CAST(@TestUserId AS VARCHAR(10)) + ';';
PRINT '';

