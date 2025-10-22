USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Assign Roles to Existing Users>
-- =============================================

-- This script helps you assign roles to existing users
-- Modify the usernames and roles according to your requirements

-- 1. Assign Admin role to specific users
-- Replace 'admin_username' with actual admin usernames
INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
SELECT u.UserId, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME = 'admin_username' -- Replace with actual admin username
AND r.RoleName = 'Admin'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
);

-- 2. Assign SalesManager role to sales users
-- Replace 'sales_username1', 'sales_username2' with actual sales usernames
INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
SELECT u.UserId, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME IN ('sales_username1', 'sales_username2') -- Replace with actual sales usernames
AND r.RoleName = 'SalesManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
);

-- 3. Assign StoreManager role to store users
-- Replace 'store_username1', 'store_username2' with actual store usernames
INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
SELECT u.UserId, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME IN ('store_username1', 'store_username2') -- Replace with actual store usernames
AND r.RoleName = 'StoreManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
);

-- 4. Assign PurchaseManager role to purchase users
-- Replace 'purchase_username1', 'purchase_username2' with actual purchase usernames
INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
SELECT u.UserId, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME IN ('purchase_username1', 'purchase_username2') -- Replace with actual purchase usernames
AND r.RoleName = 'PurchaseManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
);

-- 5. Assign ReadOnlyManager role to read-only users
-- Replace 'readonly_username1', 'readonly_username2' with actual read-only usernames
INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
SELECT u.UserId, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME IN ('readonly_username1', 'readonly_username2') -- Replace with actual read-only usernames
AND r.RoleName = 'ReadOnlyManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
);

-- 6. Assign UtilityManager role to utility users
-- Replace 'utility_username1', 'utility_username2' with actual utility usernames
INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
SELECT u.UserId, r.RoleId, 'System'
FROM USER_MASTER u, Roles r
WHERE u.UM_USERNAME IN ('utility_username1', 'utility_username2') -- Replace with actual utility usernames
AND r.RoleName = 'UtilityManager'
AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
);

-- 7. Assign multiple roles to a user (e.g., Admin + SalesManager)
-- Example: If a user should have both Admin and SalesManager roles
-- INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
-- SELECT u.UserId, r.RoleId, 'System'
-- FROM USER_MASTER u, Roles r
-- WHERE u.UM_USERNAME = 'multi_role_user'
-- AND r.RoleName IN ('Admin', 'SalesManager')
-- AND NOT EXISTS (
--     SELECT 1 FROM UserRoles ur 
--     WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
-- );

-- 8. Query to check assigned roles
SELECT 
    u.UM_USERNAME,
    u.UM_CM_ID,
    r.RoleName,
    r.RoleDescription,
    ur.IsActive,
    ur.CreatedDate
FROM USER_MASTER u
INNER JOIN UserRoles ur ON u.UserId = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE u.ES_DELETE = 0
ORDER BY u.UM_USERNAME, r.RoleName;

PRINT 'Role assignment completed!';
PRINT 'Please review the assigned roles above and modify as needed.';
