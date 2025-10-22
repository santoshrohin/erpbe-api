USE [db_a2ea4b_sunv2]
GO

-- Check existing tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
GO

-- Create USER_ROLES table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USER_ROLES]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[USER_ROLES] (
        [USER_ROLE_ID] INT IDENTITY(1,1) PRIMARY KEY,
        [USER_ID] INT NOT NULL,
        [ROLE_ID] INT NOT NULL,
        [ASSIGNED_DATE] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [ASSIGNED_BY] NVARCHAR(100) NOT NULL,
        [IS_ACTIVE] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_USER_ROLES_USER_MASTER] FOREIGN KEY ([USER_ID]) REFERENCES [USER_MASTER]([USER_ID]),
        CONSTRAINT [FK_USER_ROLES_ROLES] FOREIGN KEY ([ROLE_ID]) REFERENCES [ROLES]([ROLE_ID]),
        CONSTRAINT [UK_USER_ROLES] UNIQUE ([USER_ID], [ROLE_ID])
    );
    PRINT 'USER_ROLES table created successfully!';
END
ELSE
BEGIN
    PRINT 'USER_ROLES table already exists';
END
GO

-- Add Admin role if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM ROLES WHERE ROLE_NAME = 'Admin')
BEGIN
    INSERT INTO ROLES (ROLE_NAME, ROLE_DESCRIPTION, IS_ACTIVE, CREATED_DATE, CREATED_BY)
    VALUES ('Admin', 'Full system access - can manage everything', 1, GETDATE(), 'System');
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

PRINT 'Setup completed successfully!';
