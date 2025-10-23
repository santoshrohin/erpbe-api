/*
    Setup Test User for Integration Tests
    This script creates a dedicated test user with Admin role
    Run this script once before running tests
*/

-- Step 1: Check if test user already exists and delete if needed
DECLARE @TestUserId INT;

SELECT @TestUserId = UM_CODE 
FROM USER_MASTER 
WHERE UM_USERNAME = 'TestUser';

IF @TestUserId IS NOT NULL
BEGIN
    -- Delete user roles
    DELETE FROM UserRoles WHERE UserId = @TestUserId;
    
    -- Delete user
    DELETE FROM USER_MASTER WHERE UM_CODE = @TestUserId;
    
    PRINT 'Existing TestUser deleted.';
END

-- Step 2: Create test user
-- Password is "Test@123" encrypted using legacy encryption
INSERT INTO USER_MASTER (
    UM_USERNAME,
    UM_PASSWORD,
    UM_NAME,
    UM_EMAIL,
    UM_CM_ID,
    UM_LEVEL,
    IS_ACTIVE,
    UM_IS_ADMIN,
    ES_DELETE,
    MODIFY
)
VALUES (
    'TestUser',
    '740-910-1050-1060-540-390-400-410', -- Legacy encrypted "Test@123"
    'Test User for Integration Tests',
    'testuser@test.com',
    1, -- Company ID
    '-2147483641', -- Financial Year Code (as varchar, same as production)
    1, -- Active
    1, -- Is Admin
    0, -- Not deleted
    0 -- Not modified
);

-- Get the newly created user ID
SET @TestUserId = SCOPE_IDENTITY();

-- Step 3: Assign Admin role to test user
DECLARE @AdminRoleId INT;

SELECT @AdminRoleId = RoleId 
FROM ROLES 
WHERE RoleName = 'Admin';

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, IsActive)
    VALUES (@TestUserId, @AdminRoleId, 1);
    
    PRINT 'Admin role assigned to TestUser.';
END
ELSE
BEGIN
    PRINT 'ERROR: Admin role not found! Please create roles first.';
END

-- Step 4: Verify test user setup
SELECT 
    UM.UM_CODE AS UserId,
    UM.UM_USERNAME AS Username,
    UM.UM_NAME AS Name,
    UM.UM_EMAIL AS Email,
    R.RoleName AS Role,
    UM.IS_ACTIVE AS IsActive
FROM USER_MASTER UM
LEFT JOIN UserRoles UR ON UM.UM_CODE = UR.UserId
LEFT JOIN ROLES R ON UR.RoleId = R.RoleId
WHERE UM.UM_USERNAME = 'TestUser';

PRINT '';
PRINT '=================================================';
PRINT 'Test User Created Successfully!';
PRINT '=================================================';
PRINT 'Username: TestUser';
PRINT 'Password: Test@123';
PRINT 'Company ID: 1';
PRINT 'Financial Year Code: -2147483641';
PRINT '=================================================';

