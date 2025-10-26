-- =============================================
-- Check Party Name Uniqueness
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_CheckPartyNameUnique]
    @PartyName NVARCHAR(500),
    @Id INT = NULL,
    @CompanyId INT,
    @IsUnique BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT 1 
        FROM PARTY_MASTER 
        WHERE P_NAME = @PartyName 
          AND P_CM_COMP_ID = @CompanyId 
          AND P_TYPE = 1
          AND (@Id IS NULL OR P_CODE <> @Id)
    )
        SET @IsUnique = 0;
    ELSE
        SET @IsUnique = 1;
END

