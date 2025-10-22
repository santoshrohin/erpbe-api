USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Assign Roles to Users - Actual Table Structure>
-- =============================================

-- Assign roles to users based on their UM_CODE
-- Using the actual USER_MASTER table structure

-- 1. Assign Admin role to ERPADMIN (existing admin user)
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'ERPADMIN'
AND r.RoleName = 'Admin'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 2. Assign SalesManager role to SALESMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'SALESMGR'
AND r.RoleName = 'SalesManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 3. Assign SalesManager role to SALESUSER1 (additional sales user)
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'SALESUSER1'
AND r.RoleName = 'SalesManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 4. Assign StoreManager role to STOREMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'STOREMGR'
AND r.RoleName = 'StoreManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 5. Assign StoreManager role to STOREUSER1 (additional store user)
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'STOREUSER1'
AND r.RoleName = 'StoreManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 6. Assign PurchaseManager role to PURCHASEMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'PURCHASEMGR'
AND r.RoleName = 'PurchaseManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 7. Assign ReadOnlyManager role to READONLYMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'READONLYMGR'
AND r.RoleName = 'ReadOnlyManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 8. Assign UtilityManager role to UTILITYMGR
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'UTILITYMGR'
AND r.RoleName = 'UtilityManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 9. Assign multiple roles to ERPADMIN (Admin + SalesManager for testing)
INSERT INTO UserRoles (UserCode, RoleId, CreatedBy)
SELECT u.UM_CODE, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'ERPADMIN'
AND r.RoleName = 'SalesManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserCode = u.UM_CODE AND ur.RoleId = r.RoleId
);

-- 10. Query to check assigned roles
SELECT 
    u.UM_USERNAME,
    u.UM_NAME,
    u.UM_EMAIL,
    u.UM_CM_ID,
    r.RoleName,
    r.RoleDescription,
    ur.IsActive,
    ur.CreatedDate
FROM USER_MASTER u
INNER JOIN UserRoles ur ON u.UM_CODE = ur.UserCode
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE u.ES_DELETE = 0
ORDER BY u.UM_USERNAME, r.RoleName;

-- 11. Summary of user roles
PRINT '=== ROLE ASSIGNMENT SUMMARY ===';
PRINT 'ERPADMIN: Admin, SalesManager';
PRINT 'SALESMGR: SalesManager';
PRINT 'SALESUSER1: SalesManager';
PRINT 'STOREMGR: StoreManager';
PRINT 'STOREUSER1: StoreManager';
PRINT 'PURCHASEMGR: PurchaseManager';
PRINT 'READONLYMGR: ReadOnlyManager';
PRINT 'UTILITYMGR: UtilityManager';
PRINT '===============================';
PRINT 'All users can login with password: admin';
PRINT 'Role assignment completed successfully!';
