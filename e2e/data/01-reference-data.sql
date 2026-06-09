-- ============================================================
-- Reference / master data seed for E2E test environment
-- Safe to re-run: all statements are IF NOT EXISTS guarded
-- ============================================================

-- COMPANY_MASTER (CompanyId=1 is the test company)
IF NOT EXISTS (SELECT 1 FROM [dbo].[COMPANY_MASTER] WHERE CM_ID = 1)
BEGIN
    INSERT INTO [dbo].[COMPANY_MASTER]
        (CM_CODE, CM_ID, CM_NAME, CM_ADDRESS1, CM_EMAILID,
         CM_PHONENO1, CM_GSTIN, CM_OPENING_DATE, CM_CLOSING_DATE, CM_ACTIVE_IND)
    VALUES
        (-2147483641, 1, 'Sun Electro (Test)', '123 Test Street, Mumbai',
         'test@sunelectro.com', '022-12345678', '27AABCS1429B1ZB',
         '2024-04-01', '2025-03-31', 1);
END
GO

-- ROLES
IF NOT EXISTS (SELECT 1 FROM [dbo].[ROLES] WHERE RoleName = 'Admin')
BEGIN
    SET IDENTITY_INSERT [dbo].[ROLES] ON;
    INSERT INTO [dbo].[ROLES] (RoleId, RoleName, IsActive) VALUES (1, 'Admin', 1);
    SET IDENTITY_INSERT [dbo].[ROLES] OFF;
END
IF NOT EXISTS (SELECT 1 FROM [dbo].[ROLES] WHERE RoleName = 'SalesManager')
BEGIN
    INSERT INTO [dbo].[ROLES] (RoleName, IsActive) VALUES ('SalesManager', 1);
END
IF NOT EXISTS (SELECT 1 FROM [dbo].[ROLES] WHERE RoleName = 'StoreManager')
BEGIN
    INSERT INTO [dbo].[ROLES] (RoleName, IsActive) VALUES ('StoreManager', 1);
END
GO

-- USER_MASTER (TestUser — password = Test@123 in LegacyEncryption format)
-- Encryption: each char → (ascii * 20 / 2 - 100), joined with '-'
-- T=84→740 e=101→910 s=115→1050 t=116→1060 @=64→540 1=49→390 2=50→400 3=51→410
DECLARE @encPass VARCHAR(MAX) = '740-910-1050-1060-540-390-400-410';
IF NOT EXISTS (SELECT 1 FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser')
BEGIN
    INSERT INTO [dbo].[USER_MASTER]
        (UM_USERNAME, UM_PASSWORD, UM_NAME, UM_EMAIL, UM_CM_ID, IS_ACTIVE, UM_IS_ADMIN, ES_DELETE)
    VALUES ('TestUser', @encPass, 'Test Admin', 'test@test.com', 1, 1, 1, 0);
END
ELSE
BEGIN
    UPDATE [dbo].[USER_MASTER]
    SET UM_PASSWORD = @encPass, IS_ACTIVE = 1, UM_IS_ADMIN = 1, ES_DELETE = 0
    WHERE UM_USERNAME = 'TestUser';
END
GO

-- Assign Admin role to TestUser
DECLARE @userId INT;
DECLARE @adminRoleId INT;
SELECT @userId = UM_CODE FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser';
SELECT @adminRoleId = RoleId FROM [dbo].[ROLES] WHERE RoleName = 'Admin';
IF @userId IS NOT NULL AND @adminRoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[UserRoles] WHERE UserId = @userId AND RoleId = @adminRoleId)
        INSERT INTO [dbo].[UserRoles] (UserId, RoleId, IsActive) VALUES (@userId, @adminRoleId, 1);
END
GO

-- USER_RIGHT — full permissions (bitmask '1111111') for all relevant modules
DECLARE @uid INT;
SELECT @uid = UM_CODE FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser';
IF @uid IS NOT NULL
BEGIN
    DELETE FROM [dbo].[USER_RIGHT] WHERE UR_UM_CODE = @uid;
    INSERT INTO [dbo].[USER_RIGHT] (UR_UM_CODE, UR_SM_CODE, UR_RIGHTS, UR_IS_DELETE)
    VALUES
        (@uid, 72,  '1111111', 0),  -- CustomerPO
        (@uid, 73,  '1111111', 0),  -- TaxInvoice
        (@uid, 74,  '1111111', 0),  -- DeliveryChallan
        (@uid, 75,  '1111111', 0),  -- LabourChargeInvoice
        (@uid, 76,  '1111111', 0),  -- IssueMaster
        (@uid, 77,  '1111111', 0),  -- ProductionToStore
        (@uid, 99,  '1111111', 0),  -- CustomerMaster
        (@uid, 106, '1111111', 0);  -- Admin
END
GO

-- SO_TYPE_MASTER (PO Types required for CustomerPO form dropdown)
IF NOT EXISTS (SELECT 1 FROM [dbo].[SO_TYPE_MASTER] WHERE SO_T_COMP_ID = 1 AND ES_DELETE = 0)
BEGIN
    INSERT INTO [dbo].[SO_TYPE_MASTER] (SO_T_COMP_ID, SO_T_SHORT_NAME, SO_T_DESC, ES_DELETE)
    VALUES
        (1, 'REG',  'Regular Order', 0),
        (1, 'AMC',  'AMC Order', 0),
        (1, 'SPOT', 'Spot Order', 0);
END
GO

-- CUSTOMER_TYPE_MASTER
IF NOT EXISTS (SELECT 1 FROM [dbo].[CUSTOMER_TYPE_MASTER] WHERE CTM_CM_COMP_ID = 1 AND ES_DELETE = 0)
BEGIN
    INSERT INTO [dbo].[CUSTOMER_TYPE_MASTER]
        (CTM_TYPE_CODE, CTM_TYPE_DESC, CTM_CM_COMP_ID, CTM_COMPANY_ID, CTM_ACTIVE_IND, ES_DELETE)
    VALUES
        ('GOVT',  'Government', 1, 1, 1, 0),
        ('CORP',  'Corporate',  1, 1, 1, 0),
        ('SME',   'SME',        1, 1, 1, 0);
END
GO

-- ITEM_UNIT_MASTER (UoM — required for PO detail line items)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ITEM_UNIT_MASTER] WHERE I_UOM_CODE = 1)
BEGIN
    SET IDENTITY_INSERT [dbo].[ITEM_UNIT_MASTER] ON;
    INSERT INTO [dbo].[ITEM_UNIT_MASTER] (I_UOM_CODE, I_UOM_NAME, I_UOM_CM_COMP_ID, ES_DELETE)
    VALUES (1, 'Nos', 1, 0);
    SET IDENTITY_INSERT [dbo].[ITEM_UNIT_MASTER] OFF;
END
IF NOT EXISTS (SELECT 1 FROM [dbo].[ITEM_UNIT_MASTER] WHERE I_UOM_CODE = 2)
BEGIN
    SET IDENTITY_INSERT [dbo].[ITEM_UNIT_MASTER] ON;
    INSERT INTO [dbo].[ITEM_UNIT_MASTER] (I_UOM_CODE, I_UOM_NAME, I_UOM_CM_COMP_ID, ES_DELETE)
    VALUES (2, 'Kg', 1, 0);
    SET IDENTITY_INSERT [dbo].[ITEM_UNIT_MASTER] OFF;
END
GO
