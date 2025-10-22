USE [db_a2ea4b_sunv2]
GO

-- Check if Mohan user still exists
SELECT UM_CODE, UM_USERNAME, UM_NAME, ES_DELETE FROM USER_MASTER WHERE UM_USERNAME = 'mohan';
GO
