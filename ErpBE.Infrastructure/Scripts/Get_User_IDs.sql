USE [db_a2ea4b_sunv2]
GO

-- Get actual user IDs and usernames
SELECT UM_CODE, UM_USERNAME, UM_NAME FROM USER_MASTER ORDER BY UM_CODE;
GO
