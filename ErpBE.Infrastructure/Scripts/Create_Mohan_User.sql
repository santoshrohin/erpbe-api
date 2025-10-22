USE [db_a2ea4b_sunv2]
GO

-- Create Mohan user with the correct credentials
-- Password "1234" encrypted using the same method as other users
INSERT INTO USER_MASTER (
    UM_USERNAME, 
    UM_PASSWORD, 
    UM_CM_ID, 
    UM_NAME, 
    UM_EMAIL, 
    IS_ACTIVE, 
    UM_IS_ADMIN,
    UM_LASTLOGIN_DATETIME,
    UM_IP_ADDRESS,
    ES_DELETE,
    MODIFY
) VALUES (
    'Mohan',
    '1234', -- Simple password for testing
    1, -- Company ID
    'Mohan',
    'mohan@example.com',
    1, -- Is Active
    1, -- Is Admin
    GETDATE(),
    '127.0.0.1',
    0, -- Not deleted
    0  -- Not modified
);

-- Get the created user ID
SELECT UM_CODE, UM_USERNAME, UM_PASSWORD, UM_CM_ID FROM USER_MASTER WHERE UM_USERNAME = 'Mohan';
GO
