-- =============================================
-- Check Abbreviation Uniqueness
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_CheckAbbreviationUnique]
    @Abbreviation NVARCHAR(20),
    @Id INT = NULL,
    @CompanyId INT,
    @IsUnique BIT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;
    
    IF EXISTS (
        SELECT 1 
        FROM PARTY_MASTER 
        WHERE P_ABBREVATION = @Abbreviation 
          AND P_CM_COMP_ID = @CompanyId 
          AND P_TYPE = 1
          AND (@Id IS NULL OR P_CODE <> @Id)
    )
        SET @IsUnique = 0;
    ELSE
        SET @IsUnique = 1;
END

