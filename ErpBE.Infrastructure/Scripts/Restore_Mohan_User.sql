USE [db_a2ea4b_sunv2]
GO

-- Restore Mohan user by setting ES_DELETE = 0
UPDATE USER_MASTER 
SET ES_DELETE = 0 
WHERE UM_USERNAME = 'mohan';

-- Verify the user is restored
SELECT UM_CODE, UM_USERNAME, UM_NAME, ES_DELETE FROM USER_MASTER WHERE UM_USERNAME = 'mohan';
GO
