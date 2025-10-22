USE [db_a2ea4b_sunv2]
GO

-- =============================================
-- Author: ERP Development Team
-- Create date: 2025-01-23
-- Description: Add Admin role to Mohan user for testing
-- =============================================

-- First, let's check if Mohan exists and what roles he currently has
PRINT 'Checking Mohan user and current roles...';

SELECT 
    u.UM_CODE as UserId,
    u.UM_USERNAME as Username,
    u.UM_NAME as Name,
    r.RoleName,
    ur.IsActive
FROM USER_MASTER u
LEFT JOIN UserRoles ur ON u.UM_CODE = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.RoleId
WHERE u.UM_USERNAME = 'Mohan'
AND u.ES_DELETE = 0;

-- Check if Admin role exists
PRINT 'Checking if Admin role exists...';
SELECT RoleId, RoleName, IsActive 
FROM Roles 
WHERE RoleName = 'Admin';

-- Add Admin role to Mohan if it doesn't exist
PRINT 'Adding Admin role to Mohan...';

-- First, get Mohan's User ID
DECLARE @MohanUserId INT;
SELECT @MohanUserId = UM_CODE FROM USER_MASTER WHERE UM_USERNAME = 'Mohan' AND ES_DELETE = 0;

IF @MohanUserId IS NOT NULL
BEGIN
    PRINT 'Mohan User ID: ' + CAST(@MohanUserId AS VARCHAR(10));
    
    -- Get Admin Role ID
    DECLARE @AdminRoleId INT;
    SELECT @AdminRoleId = RoleId FROM Roles WHERE RoleName = 'Admin' AND IsActive = 1;
    
    IF @AdminRoleId IS NOT NULL
    BEGIN
        PRINT 'Admin Role ID: ' + CAST(@AdminRoleId AS VARCHAR(10));
        
        -- Check if Mohan already has Admin role
        IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @MohanUserId AND RoleId = @AdminRoleId)
        BEGIN
        -- Add Admin role to Mohan
        INSERT INTO UserRoles (UserId, RoleId, IsActive)
        VALUES (@MohanUserId, @AdminRoleId, 1);
            
            PRINT 'Admin role successfully added to Mohan!';
        END
        ELSE
        BEGIN
            PRINT 'Mohan already has Admin role.';
        END
    END
    ELSE
    BEGIN
        PRINT 'ERROR: Admin role not found!';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: Mohan user not found!';
END

-- Verify the assignment
PRINT 'Verifying Mohan''s roles after assignment...';
SELECT 
    u.UM_CODE as UserId,
    u.UM_USERNAME as Username,
    u.UM_NAME as Name,
    r.RoleName,
    ur.IsActive
FROM USER_MASTER u
LEFT JOIN UserRoles ur ON u.UM_CODE = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.RoleId
WHERE u.UM_USERNAME = 'Mohan'
AND u.ES_DELETE = 0
ORDER BY r.RoleName;

PRINT 'Script completed successfully!';
