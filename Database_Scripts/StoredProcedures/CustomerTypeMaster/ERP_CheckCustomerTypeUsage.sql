CREATE OR ALTER PROCEDURE [dbo].[ERP_CheckCustomerTypeUsage]
    @CTM_CODE INT,
    @IsUsed BIT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;

    DECLARE @Count INT;

    -- Check if customer type is used in PARTY_MASTER (P_TYPE=1 indicates customer)
    SELECT @Count = COUNT(*)
    FROM PARTY_MASTER
    WHERE P_CUST_TYPE = @CTM_CODE
        AND P_TYPE = 1
        AND ES_DELETE = 0;

    IF @Count > 0
        SET @IsUsed = 1;
    ELSE
        SET @IsUsed = 0;
END;
GO

