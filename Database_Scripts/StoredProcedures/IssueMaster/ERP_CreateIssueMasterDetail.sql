CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateIssueMasterDetail]
    @IssueCode    INT,
    @CompanyCode  INT,
    @ItemCode     INT           = NULL,
    @UomCode      INT           = NULL,
    @CurrentStock DECIMAL(18,3) = 0,
    @RequestedQty DECIMAL(18,3) = 0,
    @IssuedQty    DECIMAL(18,3) = 0,
    @Remark       VARCHAR(200)  = NULL,
    @Rate         DECIMAL(18,4) = NULL,
    @Amount       DECIMAL(18,4) = NULL,
    @ToStore      INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ISSUE_MASTER_DETAIL
        (IMD_COMP_ID, IM_CODE, IMD_I_CODE, IMD_UOM, IMD_CURR_STOCK,
         IMD_REQ_QTY, IMD_ISSUE_QTY, IMD_REMARK, IMD_RATE, IMD_AMOUNT,
         IMD_To_STORE, ES_DELETE)
    VALUES
        (@CompanyCode, @IssueCode, @ItemCode, @UomCode, @CurrentStock,
         @RequestedQty, @IssuedQty, @Remark, @Rate, @Amount,
         @ToStore, 0);
END
GO
