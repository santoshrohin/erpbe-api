CREATE OR ALTER PROCEDURE [dbo].[ERP_GetCustomerTypeMasterById]
    @CTM_CODE INT
AS
BEGIN
    SET NOCOUNT OFF;

    SELECT 
        CTM_CODE,
        CTM_CM_COMP_ID,
        CTM_TYPE_CODE,
        CTM_TYPE_DESC,
        CTM_FIRST_LETTER,
        ES_DELETE,
        MODIFY
    FROM 
        CUSTOMER_TYPE_MASTER
    WHERE 
        CTM_CODE = @CTM_CODE
        AND ES_DELETE = 0;
END;
GO

