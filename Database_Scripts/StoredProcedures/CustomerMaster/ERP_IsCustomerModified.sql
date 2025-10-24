-- =============================================
-- Stored Procedure: ERP_IsCustomerModified
-- Description: Checks if a Customer Master record is being modified by another user
-- Returns: 1 if modified, 0 if not modified
-- Note: In legacy, this checks ES_MODIFY_LOCK. Since we don't have that column,
-- we'll check if ES_MODIFY_DATE was updated in the last 5 minutes
-- =============================================
CREATE OR ALTER PROCEDURE ERP_IsCustomerModified
    @Id INT
AS
BEGIN
    SET NOCOUNT OFF;

    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM PARTY_MASTER
    WHERE P_CODE = @Id
        AND P_TYPE = 1
        AND ES_DELETE = 0
        AND ES_MODIFY_DATE IS NOT NULL
        AND DATEDIFF(MINUTE, ES_MODIFY_DATE, GETDATE()) < 5;

    IF @Count > 0
        SELECT 1 AS IsModified; -- Modified by another user
    ELSE
        SELECT 0 AS IsModified; -- Not modified
END
GO

