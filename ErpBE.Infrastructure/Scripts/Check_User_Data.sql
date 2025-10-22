USE [db_a2ea4b_sunv2]
GO

-- Check USER_MASTER table structure and sample data
SELECT TOP 5 
    UM_CODE,
    UM_USERNAME,
    UM_NAME,
    UM_EMAIL,
    UM_CM_ID,
    UM_LEVEL,
    UM_IS_ADMIN
FROM USER_MASTER;
GO

-- Check what's in the FinancialYearCode column (UM_LEVEL)
SELECT DISTINCT UM_LEVEL FROM USER_MASTER;
GO
