CREATE OR ALTER PROCEDURE [dbo].[ERP_UnlockCustomerPo]
    @PoCode INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CUSTPO_MASTER
    SET    MODIFY      = 0,
           MODIFY_TIME = NULL,
           MODIFY_BY   = NULL
    WHERE  CPOM_CODE = @PoCode;
END
GO
