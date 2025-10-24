-- =============================================
-- Stored Procedure: ERP_IsPartyNameUnique
-- Description: Checks if Party Name is unique (case-insensitive)
-- Returns: 1 if unique, 0 if not unique
-- =============================================
CREATE OR ALTER PROCEDURE ERP_IsPartyNameUnique
    @PartyName NVARCHAR(500),
    @Id INT = NULL,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT OFF;

    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM PARTY_MASTER
    WHERE LOWER(P_NAME) = LOWER(@PartyName)
        AND P_C_CODE = @CompanyId
        AND P_TYPE = 1
        AND ES_DELETE = 0
        AND (@Id IS NULL OR P_CODE <> @Id);

    IF @Count > 0
        SELECT 0 AS IsUnique; -- Not unique
    ELSE
        SELECT 1 AS IsUnique; -- Unique
END
GO

