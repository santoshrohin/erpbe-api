-- Seed test data
USE ErpBE_Test;
GO

-- Insert test roles
INSERT INTO ROLES (RoleName, Description, IsActive) VALUES 
    ('Admin', 'Administrator', 1),
    ('SalesManager', 'Sales Manager', 1),
    ('StoreManager', 'Store Manager', 1),
    ('PurchaseManager', 'Purchase Manager', 1),
    ('ReadOnlyManager', 'Read Only Manager', 1),
    ('UtilityManager', 'Utility Manager', 1);

-- Insert test users
INSERT INTO USER_MASTER (UM_USERNAME, UM_PASSWORD, UM_NAME, UM_EMAIL, UM_CM_ID, UM_LEVEL, IS_ACTIVE, UM_IS_ADMIN) VALUES 
    ('testadmin', 'password', 'Test Admin', 'admin@test.com', 1, 'Admin', 1, 1),
    ('testuser', 'password', 'Test User', 'user@test.com', 1, 'User', 1, 0),
    ('mohan', '1234', 'Mohan Test', 'mohan@test.com', 1, 'Admin', 1, 1);

-- Insert user roles
INSERT INTO UserRoles (UserId, RoleId, IsActive) VALUES 
    (1, 1, 1), -- testadmin -> Admin
    (1, 2, 1), -- testadmin -> SalesManager
    (1, 3, 1), -- testadmin -> StoreManager
    (2, 2, 1), -- testuser -> SalesManager
    (3, 1, 1), -- mohan -> Admin
    (3, 2, 1), -- mohan -> SalesManager
    (3, 3, 1); -- mohan -> StoreManager

-- Insert test units
INSERT INTO ITEM_UNIT_MASTER (I_UOM_CM_COMP_ID, I_UOM_NAME, I_UOM_DESC, ES_DELETE, MODIFY) VALUES 
    (1, 'KG', 'Kilogram', 0, 0),
    (1, 'LTR', 'Liter', 0, 0),
    (1, 'PCS', 'Pieces', 0, 0),
    (1, 'MTR', 'Meter', 0, 0),
    (1, 'BOX', 'Box', 0, 0);

-- Insert audit trail entries for existing units
INSERT INTO AUDIT_TRAIL (TABLE_NAME, RECORD_ID, ACTION_TYPE, NEW_VALUES, CREATED_BY, CREATED_DATE) VALUES 
    ('ITEM_UNIT_MASTER', 1, 'INSERT', '{"UnitName":"KG","UnitDescription":"Kilogram","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
    ('ITEM_UNIT_MASTER', 2, 'INSERT', '{"UnitName":"LTR","UnitDescription":"Liter","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
    ('ITEM_UNIT_MASTER', 3, 'INSERT', '{"UnitName":"PCS","UnitDescription":"Pieces","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
    ('ITEM_UNIT_MASTER', 4, 'INSERT', '{"UnitName":"MTR","UnitDescription":"Meter","CompanyId":1,"IsActive":true}', 'System', GETDATE()),
    ('ITEM_UNIT_MASTER', 5, 'INSERT', '{"UnitName":"BOX","UnitDescription":"Box","CompanyId":1,"IsActive":true}', 'System', GETDATE());

PRINT 'Test data seeded successfully!';
