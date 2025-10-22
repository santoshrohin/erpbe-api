USE [db_a2ea4b_sunv2]
GO

-- Add Admin role if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM ROLES WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO ROLES (RoleName, RoleDescription, IsActive)
    VALUES ('Admin', 'Full system access - can manage everything', 1);
    PRINT 'Admin role created';
END
ELSE
BEGIN
    PRINT 'Admin role already exists';
END
GO

-- Get the Admin role ID
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = RoleId FROM ROLES WHERE RoleName = 'Admin';
PRINT 'Admin Role ID: ' + CAST(@AdminRoleId AS VARCHAR(10));
GO

-- Get the Mohan user ID
DECLARE @MohanUserId INT;
SELECT @MohanUserId = UM_CODE FROM USER_MASTER WHERE UM_USERNAME = 'Mohan';
PRINT 'Mohan User ID: ' + CAST(@MohanUserId AS VARCHAR(10));
GO

-- Assign Admin role to Mohan if not already assigned
DECLARE @AdminRoleId INT, @MohanUserId INT;

SELECT @AdminRoleId = RoleId FROM ROLES WHERE RoleName = 'Admin';
SELECT @MohanUserId = UM_CODE FROM USER_MASTER WHERE UM_USERNAME = 'Mohan';

IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @MohanUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, IsActive)
    VALUES (@MohanUserId, @AdminRoleId, 1);
    PRINT 'Admin role assigned to Mohan';
END
ELSE
BEGIN
    PRINT 'Admin role already assigned to Mohan';
END
GO

-- Verify the assignment
SELECT 
    um.UM_USERNAME,
    r.RoleName,
    ur.IsActive
FROM USER_MASTER um
JOIN UserRoles ur ON um.UM_CODE = ur.UserId
JOIN ROLES r ON ur.RoleId = r.RoleId
WHERE um.UM_USERNAME = 'Mohan';
GO

PRINT 'User role assignment completed successfully!';
