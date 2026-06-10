CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateDeliveryChallanDetail]
    @ChallanCode      INT,
    @ItemCode         INT           = NULL,
    @OrderedQuantity  FLOAT         = 0,
    @BatchNumber      VARCHAR(50)   = NULL,
    @NumberOfPacks    VARCHAR(50)   = NULL,
    @UomCode          INT           = NULL,
    @Remark           VARCHAR(100)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DELIVERY_CHALLAN_DETAIL
        (DCD_DCM_CODE, DCD_I_CODE, DCD_ORD_QTY, DCD_BATCH_NO,
         DCD_NO_OF_PACKS, DCD_UM_CODE, ES_DELETE, DCD_REMARK, DCD_RET_QTY)
    VALUES
        (@ChallanCode, @ItemCode, @OrderedQuantity, @BatchNumber,
         @NumberOfPacks, @UomCode, 0, @Remark, 0);

    -- Deduct stock: insert outbound row into STOCK_LEDGER
    -- STL_STORE_TYPE = -2147483648 (default main store, matches ERP_GetTaxInvoiceItemDetails filter)
    INSERT INTO STOCK_LEDGER
        (STL_I_CODE, STL_DOC_NO, STL_DOC_TYPE, STL_DOC_DATE, STL_DOC_QTY, STL_STORE_TYPE)
    SELECT
        @ItemCode,
        @ChallanCode,
        'DCOUT',
        DCM_DATE,
        -@OrderedQuantity,  -- negative = stock OUT
        -2147483648
    FROM DELIVERY_CHALLAN_MASTER
    WHERE DCM_CODE = @ChallanCode;
END
GO
