IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ERP_CheckPoNumberExists')
    DROP PROCEDURE [dbo].[ERP_CheckPoNumberExists]
GO

-- Returns 1 if a PO number already exists for the company (case-insensitive).
-- Pass @ExcludePoCode to skip the current record when checking on update/amend.
-- Mirrors legacy CustomerPO.aspx.cs: CUSTPO_MASTER WHERE CPOM_PONO = UPPER(txtPONumber) check.
CREATE PROCEDURE [dbo].[ERP_CheckPoNumberExists]
    @PoNumber VARCHAR(100),
    @CompanyId INT,
    @ExcludePoCode INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @ExcludePoCode IS NOT NULL
    BEGIN
        SELECT CAST(
            CASE WHEN EXISTS (
                SELECT 1 FROM CUSTPO_MASTER
                WHERE UPPER(CPOM_PONO) = UPPER(@PoNumber)
                  AND CPOM_CM_COMP_ID = @CompanyId
                  AND ES_DELETE = 0
                  AND CPOM_CODE != @ExcludePoCode
            ) THEN 1 ELSE 0 END
        AS BIT) AS PoExists;
    END
    ELSE
    BEGIN
        SELECT CAST(
            CASE WHEN EXISTS (
                SELECT 1 FROM CUSTPO_MASTER
                WHERE UPPER(CPOM_PONO) = UPPER(@PoNumber)
                  AND CPOM_CM_COMP_ID = @CompanyId
                  AND ES_DELETE = 0
            ) THEN 1 ELSE 0 END
        AS BIT) AS PoExists;
    END
END
GO
