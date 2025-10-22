USE [db_a2ea4b_farmerplacement]
GO

-- =============================================
-- Author:		<Santosh More>
-- Create date: <19-10-2025>
-- Description:	<Create Test Users for Each Role>
-- =============================================

-- Create test users for each role
-- Using the same password encryption pattern as your existing admin user

-- 1. Sales Manager User
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'SALESMGR')
BEGIN
    INSERT INTO USER_MASTER (
        UM_CODE, UM_CM_ID, UM_DM_CODE, UM_EM_CODE, UM_USERNAME, UM_PASSWORD, 
        UM_LEVEL, UM_LASTLOGIN_DATETIME, UM_IP_ADDRESS, IS_ACTIVE, UM_EMAIL_SEND, 
        UM_LOGIN_FLAG, ES_DELETE, MODIFY, UM_IS_ADMIN, UM_NAME, UM_EMAIL
    ) VALUES (
        -2147483647, 1, NULL, NULL, 'SALESMGR', '870-880-890-900-540-390-400-410-420',
        'Sales Manager', NULL, NULL, 1, 0, 0, 0, 0, 0, 'Sales Manager', 'sales@company.com'
    );
END
GO

-- 2. Store Manager User
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'STOREMGR')
BEGIN
    INSERT INTO USER_MASTER (
        UM_CODE, UM_CM_ID, UM_DM_CODE, UM_EM_CODE, UM_USERNAME, UM_PASSWORD, 
        UM_LEVEL, UM_LASTLOGIN_DATETIME, UM_IP_ADDRESS, IS_ACTIVE, UM_EMAIL_SEND, 
        UM_LOGIN_FLAG, ES_DELETE, MODIFY, UM_IS_ADMIN, UM_NAME, UM_EMAIL
    ) VALUES (
        -2147483646, 1, NULL, NULL, 'STOREMGR', '870-880-890-900-540-390-400-410-420',
        'Store Manager', NULL, NULL, 1, 0, 0, 0, 0, 0, 'Store Manager', 'store@company.com'
    );
END
GO

-- 3. Purchase Manager User
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'PURCHASEMGR')
BEGIN
    INSERT INTO USER_MASTER (
        UM_CODE, UM_CM_ID, UM_DM_CODE, UM_EM_CODE, UM_USERNAME, UM_PASSWORD, 
        UM_LEVEL, UM_LASTLOGIN_DATETIME, UM_IP_ADDRESS, IS_ACTIVE, UM_EMAIL_SEND, 
        UM_LOGIN_FLAG, ES_DELETE, MODIFY, UM_IS_ADMIN, UM_NAME, UM_EMAIL
    ) VALUES (
        -2147483645, 1, NULL, NULL, 'PURCHASEMGR', '870-880-890-900-540-390-400-410-420',
        'Purchase Manager', NULL, NULL, 1, 0, 0, 0, 0, 0, 'Purchase Manager', 'purchase@company.com'
    );
END
GO

-- 4. Read Only Manager User
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'READONLYMGR')
BEGIN
    INSERT INTO USER_MASTER (
        UM_CODE, UM_CM_ID, UM_DM_CODE, UM_EM_CODE, UM_USERNAME, UM_PASSWORD, 
        UM_LEVEL, UM_LASTLOGIN_DATETIME, UM_IP_ADDRESS, IS_ACTIVE, UM_EMAIL_SEND, 
        UM_LOGIN_FLAG, ES_DELETE, MODIFY, UM_IS_ADMIN, UM_NAME, UM_EMAIL
    ) VALUES (
        -2147483644, 1, NULL, NULL, 'READONLYMGR', '870-880-890-900-540-390-400-410-420',
        'Read Only Manager', NULL, NULL, 1, 0, 0, 0, 0, 0, 'Read Only Manager', 'readonly@company.com'
    );
END
GO

-- 5. Utility Manager User
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'UTILITYMGR')
BEGIN
    INSERT INTO USER_MASTER (
        UM_CODE, UM_CM_ID, UM_DM_CODE, UM_EM_CODE, UM_USERNAME, UM_PASSWORD, 
        UM_LEVEL, UM_LASTLOGIN_DATETIME, UM_IP_ADDRESS, IS_ACTIVE, UM_EMAIL_SEND, 
        UM_LOGIN_FLAG, ES_DELETE, MODIFY, UM_IS_ADMIN, UM_NAME, UM_EMAIL
    ) VALUES (
        -2147483643, 1, NULL, NULL, 'UTILITYMGR', '870-880-890-900-540-390-400-410-420',
        'Utility Manager', NULL, NULL, 1, 0, 0, 0, 0, 0, 'Utility Manager', 'utility@company.com'
    );
END
GO

-- 6. Additional Sales User
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'SALESUSER1')
BEGIN
    INSERT INTO USER_MASTER (
        UM_CODE, UM_CM_ID, UM_DM_CODE, UM_EM_CODE, UM_USERNAME, UM_PASSWORD, 
        UM_LEVEL, UM_LASTLOGIN_DATETIME, UM_IP_ADDRESS, IS_ACTIVE, UM_EMAIL_SEND, 
        UM_LOGIN_FLAG, ES_DELETE, MODIFY, UM_IS_ADMIN, UM_NAME, UM_EMAIL
    ) VALUES (
        -2147483642, 1, NULL, NULL, 'SALESUSER1', '870-880-890-900-540-390-400-410-420',
        'Sales User', NULL, NULL, 1, 0, 0, 0, 0, 0, 'Sales User 1', 'sales1@company.com'
    );
END
GO

-- 7. Additional Store User
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'STOREUSER1')
BEGIN
    INSERT INTO USER_MASTER (
        UM_CODE, UM_CM_ID, UM_DM_CODE, UM_EM_CODE, UM_USERNAME, UM_PASSWORD, 
        UM_LEVEL, UM_LASTLOGIN_DATETIME, UM_IP_ADDRESS, IS_ACTIVE, UM_EMAIL_SEND, 
        UM_LOGIN_FLAG, ES_DELETE, MODIFY, UM_IS_ADMIN, UM_NAME, UM_EMAIL
    ) VALUES (
        -2147483641, 1, NULL, NULL, 'STOREUSER1', '870-880-890-900-540-390-400-410-420',
        'Store User', NULL, NULL, 1, 0, 0, 0, 0, 0, 'Store User 1', 'store1@company.com'
    );
END
GO

PRINT 'Test users created successfully!';
PRINT 'Users created:';
PRINT '- ERPADMIN (existing admin)';
PRINT '- SALESMGR (Sales Manager)';
PRINT '- STOREMGR (Store Manager)';
PRINT '- PURCHASEMGR (Purchase Manager)';
PRINT '- READONLYMGR (Read Only Manager)';
PRINT '- UTILITYMGR (Utility Manager)';
PRINT '- SALESUSER1 (Sales User)';
PRINT '- STOREUSER1 (Store User)';
PRINT 'All users have the same password: admin (encrypted)';
PRINT 'Next step: Run the role assignment script.';
