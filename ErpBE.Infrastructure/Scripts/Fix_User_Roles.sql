USE [db_a2ea4b_sunv2]
GO

-- Check current roles
SELECT * FROM ROLES;
GO

-- Check current user roles
SELECT * FROM USER_ROLES;
GO

-- Check user details
SELECT * FROM USER_MASTER WHERE USER_NAME = 'Mohan';
GO

-- Create Admin role if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM ROLES WHERE ROLE_NAME = 'Admin')
BEGIN
    INSERT INTO ROLES (ROLE_NAME, ROLE_DESCRIPTION, IS_ACTIVE, CREATED_DATE, CREATED_BY)
    VALUES ('Admin', 'Full system access', 1, GETDATE(), 'System');
    PRINT 'Admin role created';
END
ELSE
BEGIN
    PRINT 'Admin role already exists';
END
GO

-- Get the Admin role ID
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = ROLE_ID FROM ROLES WHERE ROLE_NAME = 'Admin';
PRINT 'Admin Role ID: ' + CAST(@AdminRoleId AS VARCHAR(10));
GO

-- Get the Mohan user ID
DECLARE @MohanUserId INT;
SELECT @MohanUserId = USER_ID FROM USER_MASTER WHERE USER_NAME = 'Mohan';
PRINT 'Mohan User ID: ' + CAST(@MohanUserId AS VARCHAR(10));
GO

-- Assign Admin role to Mohan if not already assigned
DECLARE @AdminRoleId INT, @MohanUserId INT;

SELECT @AdminRoleId = ROLE_ID FROM ROLES WHERE ROLE_NAME = 'Admin';
SELECT @MohanUserId = USER_ID FROM USER_MASTER WHERE USER_NAME = 'Mohan';

IF NOT EXISTS (SELECT 1 FROM USER_ROLES WHERE USER_ID = @MohanUserId AND ROLE_ID = @AdminRoleId)
BEGIN
    INSERT INTO USER_ROLES (USER_ID, ROLE_ID, ASSIGNED_DATE, ASSIGNED_BY)
    VALUES (@MohanUserId, @AdminRoleId, GETDATE(), 'System');
    PRINT 'Admin role assigned to Mohan';
END
ELSE
BEGIN
    PRINT 'Admin role already assigned to Mohan';
END
GO

-- Verify the assignment
SELECT 
    um.USER_NAME,
    r.ROLE_NAME,
    ur.ASSIGNED_DATE
FROM USER_MASTER um
JOIN USER_ROLES ur ON um.USER_ID = ur.USER_ID
JOIN ROLES r ON ur.ROLE_ID = r.ROLE_ID
WHERE um.USER_NAME = 'Mohan';
GO

PRINT 'User role assignment completed!';
