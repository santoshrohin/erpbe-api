CREATE OR ALTER PROCEDURE [dbo].[ERP_IsTypeCodeUnique]
    @CTM_TYPE_CODE VARCHAR(50),
    @ExcludeId INT = NULL,
    @CTM_CM_COMP_ID INT,
    @IsUnique BIT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;

    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM CUSTOMER_TYPE_MASTER
    WHERE LOWER(CTM_TYPE_CODE) = LOWER(@CTM_TYPE_CODE)
        AND CTM_CM_COMP_ID = @CTM_CM_COMP_ID
        AND ES_DELETE = 0
        AND (@ExcludeId IS NULL OR CTM_CODE <> @ExcludeId);

    IF @Count = 0
        SET @IsUnique = 1;
    ELSE
        SET @IsUnique = 0;
END;
GO

