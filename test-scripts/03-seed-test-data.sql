-- Seed test data
USE ErpBE_Test;
GO

-- Insert test roles (if not exists)
IF NOT EXISTS (SELECT 1 FROM ROLES WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO ROLES (RoleName, RoleDescription, IsActive) VALUES 
        ('Admin', 'Administrator', 1),
        ('SalesManager', 'Sales Manager', 1),
        ('StoreManager', 'Store Manager', 1),
        ('PurchaseManager', 'Purchase Manager', 1),
        ('ReadOnlyManager', 'Read Only Manager', 1),
        ('UtilityManager', 'Utility Manager', 1);
    PRINT 'Test roles inserted';
END
GO

-- Insert test user 'Mohan' with encrypted password
-- Password: 1234 (will be encrypted by legacy encryption)
IF NOT EXISTS (SELECT 1 FROM USER_MASTER WHERE UM_USERNAME = 'Mohan')
BEGIN
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
    ) VALUES (
        'Mohan',
        'MTIzNA==', -- Base64 encoded '1234' (legacy encryption format)
        'Mohan',
        'mohan@test.com',
        1,
        -2147483641, -- FinancialYearCode
        1,
        1,
        0,
        0
    );
    PRINT 'Test user Mohan inserted';
END
GO

-- Get the UserId for Mohan (it will be an IDENTITY value)
DECLARE @MohanUserId INT;
SELECT @MohanUserId = UM_CODE FROM USER_MASTER WHERE UM_USERNAME = 'Mohan';

-- Insert user roles for Mohan (Admin, SalesManager, StoreManager)
IF @MohanUserId IS NOT NULL
BEGIN
    -- Admin role
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @MohanUserId AND RoleId = (SELECT RoleId FROM ROLES WHERE RoleName = 'Admin'))
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, IsActive) 
        SELECT @MohanUserId, RoleId, 1 FROM ROLES WHERE RoleName = 'Admin';
    END
    
    -- SalesManager role
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @MohanUserId AND RoleId = (SELECT RoleId FROM ROLES WHERE RoleName = 'SalesManager'))
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, IsActive) 
        SELECT @MohanUserId, RoleId, 1 FROM ROLES WHERE RoleName = 'SalesManager';
    END
    
    -- StoreManager role
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @MohanUserId AND RoleId = (SELECT RoleId FROM ROLES WHERE RoleName = 'StoreManager'))
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, IsActive) 
        SELECT @MohanUserId, RoleId, 1 FROM ROLES WHERE RoleName = 'StoreManager';
    END
    
    PRINT 'Test user roles assigned to Mohan';
END
GO

-- Insert test units (if not already exist)
IF NOT EXISTS (SELECT 1 FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = 'KG' AND I_UOM_CM_COMP_ID = 1)
BEGIN
    INSERT INTO ITEM_UNIT_MASTER (I_UOM_CM_COMP_ID, I_UOM_NAME, I_UOM_DESC, ES_DELETE, MODIFY) VALUES 
        (1, 'KG', 'Kilogram', 0, 0),
        (1, 'LTR', 'Liter', 0, 0),
        (1, 'PCS', 'Pieces', 0, 0),
        (1, 'MTR', 'Meter', 0, 0),
        (1, 'BOX', 'Box', 0, 0);
    PRINT 'Test units inserted';
END
GO

-- Insert audit trail entries for existing units
DECLARE @UnitId1 INT, @UnitId2 INT, @UnitId3 INT, @UnitId4 INT, @UnitId5 INT;
SELECT @UnitId1 = I_UOM_CODE FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = 'KG' AND I_UOM_CM_COMP_ID = 1;
SELECT @UnitId2 = I_UOM_CODE FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = 'LTR' AND I_UOM_CM_COMP_ID = 1;
SELECT @UnitId3 = I_UOM_CODE FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = 'PCS' AND I_UOM_CM_COMP_ID = 1;
SELECT @UnitId4 = I_UOM_CODE FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = 'MTR' AND I_UOM_CM_COMP_ID = 1;
SELECT @UnitId5 = I_UOM_CODE FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = 'BOX' AND I_UOM_CM_COMP_ID = 1;

IF @UnitId1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM AUDIT_TRAIL WHERE TABLE_NAME = 'ITEM_UNIT_MASTER' AND RECORD_ID = @UnitId1)
BEGIN
    INSERT INTO AUDIT_TRAIL (TABLE_NAME, RECORD_ID, ACTION_TYPE, NEW_VALUES, CREATED_BY, CREATED_DATE) VALUES 
        ('ITEM_UNIT_MASTER', @UnitId1, 'INSERT', '{"UnitName":"KG","UnitDescription":"Kilogram","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
        ('ITEM_UNIT_MASTER', @UnitId2, 'INSERT', '{"UnitName":"LTR","UnitDescription":"Liter","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
        ('ITEM_UNIT_MASTER', @UnitId3, 'INSERT', '{"UnitName":"PCS","UnitDescription":"Pieces","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
        ('ITEM_UNIT_MASTER', @UnitId4, 'INSERT', '{"UnitName":"MTR","UnitDescription":"Meter","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
        ('ITEM_UNIT_MASTER', @UnitId5, 'INSERT', '{"UnitName":"BOX","UnitDescription":"Box","CompanyId":1,"IsActive":true}', 'System', GETDATE());
    PRINT 'Audit trail entries inserted';
END
GO

PRINT 'Test data seeded successfully!';
