CREATE OR ALTER PROCEDURE [dbo].[ERP_IsCustomerTypeModified]
    @CTM_CODE INT,
    @IsModified BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if MODIFY column exists and is not null (or if it's a BIT, check if it's 1)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CUSTOMER_TYPE_MASTER]') AND name = 'MODIFY' AND system_type_id = 56) -- BIT type
        SELECT @IsModified = ISNULL(CAST(MODIFY AS INT), 0)
        FROM CUSTOMER_TYPE_MASTER
        WHERE CTM_CODE = @CTM_CODE
            AND ES_DELETE = 0;
    ELSE
        SELECT @IsModified = CASE WHEN MODIFY IS NOT NULL THEN 1 ELSE 0 END
        FROM CUSTOMER_TYPE_MASTER
        WHERE CTM_CODE = @CTM_CODE
            AND ES_DELETE = 0;
END;
GO

