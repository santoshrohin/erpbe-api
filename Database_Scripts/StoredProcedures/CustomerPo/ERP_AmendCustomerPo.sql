IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ERP_AmendCustomerPo')
    DROP PROCEDURE [dbo].[ERP_AmendCustomerPo]
GO

-- Mirrors legacy CustomerPO.aspx.cs AMEND path (SaveRec, lines 795-856):
--   1. Increment CPOM_AM_COUNT
--   2. Copy current CUSTPO_MASTER row → CUSTPO_AM_MASTER
--   3. Copy current CUSTPO_DETAIL rows → CUSTPO_AMD_DETAIL (linked to new CPOM_AM_CODE)
--   4. UPDATE CUSTPO_MASTER with new data + CPOM_AM_COUNT + CPOM_AM_DATE
--   5. DELETE CUSTPO_DETAIL (caller re-inserts via ERP_CreateCustomerPoDetail)
--   6. Release lock (MODIFY = 0)
CREATE PROCEDURE [dbo].[ERP_AmendCustomerPo]
    @PoCode INT,
    @CustomerCode INT,
    @PoNumber VARCHAR(100),
    @PoType INT,
    @PoDate DATETIME,
    @CreditDays INT,
    @CompanyId INT,
    @WorkOrderNumber VARCHAR(50) = NULL,
    @PaymentTerms VARCHAR(260) = NULL,
    @IsAuthorized BIT = 0,
    @CustomerPoDate DATETIME = NULL,
    @QuotationCode INT = NULL,
    @TaxName VARCHAR(50) = NULL,
    @TaxPercentage FLOAT = NULL,
    @TaxAmount FLOAT = NULL,
    @ExcisePercentage FLOAT = NULL,
    @ExciseEducationPercentage FLOAT = NULL,
    @ExciseHigherEducationPercentage FLOAT = NULL,
    @BasicAmount FLOAT = NULL,
    @DiscountPercentage FLOAT = NULL,
    @DiscountAmount FLOAT = NULL,
    @DiscountReason VARCHAR(50) = NULL,
    @DeviationAmount FLOAT = NULL,
    @DeviationReason VARCHAR(50) = NULL,
    @PackingAmount FLOAT = NULL,
    @ExciseAmount FLOAT = NULL,
    @RoundingAmount FLOAT = NULL,
    @GrandTotal FLOAT = NULL,
    @FinalDestination VARCHAR(50) = NULL,
    @PreCarriageBy VARCHAR(50) = NULL,
    @PortOfLoading VARCHAR(50) = NULL,
    @PortOfDischarge VARCHAR(50) = NULL,
    @PlaceOfDelivery VARCHAR(50) = NULL,
    @BuyerName VARCHAR(50) = NULL,
    @BuyerAddress VARCHAR(150) = NULL,
    @CurrencyCode INT = NULL,
    @InquiryCode INT = NULL,
    @IsVerbalOrder BIT = 0,
    @ProjectCode INT = NULL,
    @ProjectName VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- 1. Compute new amendment count
        DECLARE @AmendCount INT;
        SELECT @AmendCount = ISNULL(CPOM_AM_COUNT, 0) + 1
        FROM CUSTPO_MASTER
        WHERE CPOM_CODE = @PoCode;

        IF @AmendCount IS NULL
            RAISERROR('Customer PO not found: %d', 16, 1, @PoCode);

        -- 2. Archive current master into CUSTPO_AM_MASTER
        INSERT INTO CUSTPO_AM_MASTER (
            CPOM_CODE, CPOM_P_CODE, CPOM_PONO, CPOM_DOC_NO, CPOM_TYPE,
            CPOM_DATE, CPOM_CR_DAYS, CPOM_CM_COMP_ID, MODIFY, ES_DELETE,
            CPOM_PAY_TERM, CPOM_AUTH_FLG, CPOM_QE_CODE, CPOM_T_NAME,
            CPOM_T_PER, CPOM_T_AMT, CPOM_BASIC_AMT, CPOM_DISCOUNT_PER,
            CPOM_DISCOUNT_AMT, CPOM_DISCOUNT_REASON, CPOM_DEVIATION_AMT,
            CPOM_DEVIATION_REASON, CPOM_PACKING_AMT, CPOM_EXC_PER,
            CPOM_EXC_EDU_PER, CPOM_EXC_HEDU_PER, CPOM_EXC_AMT,
            CPOM_ROUNDING, CPOM_GRAND_TOT, CPOM_INV_FLAG,
            CPOM_FINAL_DEST, CPOM_PRE_CARR_BY, CPOM_PORT_LOAD,
            CPOM_PORT_DIS, CPOM_PLACE_DEL, CPOM_BUYER_NAME, CPOM_BUYER_ADD,
            CPOM_CURR_CODE, CPOM_WORK_ODR_NO, CPOM_PO_DATE,
            CPOM_AM_COUNT, CPOM_AM_DATE, CPOM_INQ_CODE
        )
        SELECT
            CPOM_CODE, CPOM_P_CODE, CPOM_PONO, CPOM_DOC_NO, CPOM_TYPE,
            CPOM_DATE, CPOM_CR_DAYS, CPOM_CM_COMP_ID, MODIFY, ES_DELETE,
            CPOM_PAY_TERM, CPOM_AUTH_FLG, CPOM_QE_CODE, CPOM_T_NAME,
            CPOM_T_PER, CPOM_T_AMT, CPOM_BASIC_AMT, CPOM_DISCOUNT_PER,
            CPOM_DISCOUNT_AMT, CPOM_DISCOUNT_REASON, CPOM_DEVIATION_AMT,
            CPOM_DEVIATION_REASON, CPOM_PACKING_AMT, CPOM_EXC_PER,
            CPOM_EXC_EDU_PER, CPOM_EXC_HEDU_PER, CPOM_EXC_AMT,
            CPOM_ROUNDING, CPOM_GRAND_TOT, CPOM_INV_FLAG,
            CPOM_FINAL_DEST, CPOM_PRE_CARR_BY, CPOM_PORT_LOAD,
            CPOM_PORT_DIS, CPOM_PLACE_DEL, CPOM_BUYER_NAME, CPOM_BUYER_ADD,
            CPOM_CURR_CODE, CPOM_WORK_ODR_NO, CPOM_PO_DATE,
            @AmendCount, GETDATE(), CPOM_INQ_CODE
        FROM CUSTPO_MASTER
        WHERE CPOM_CODE = @PoCode;

        DECLARE @AmCode INT = SCOPE_IDENTITY();

        -- 3. Archive current detail lines into CUSTPO_AMD_DETAIL
        INSERT INTO CUSTPO_AMD_DETAIL (
            CPOD_CPOM_CODE, CPOD_I_CODE, CPOD_UOM_CODE, CPOD_ORD_QTY,
            CPOD_RATE, CPOD_AMT, CPOD_STATUS, CPOD_DISPACH, CPOD_DESC,
            CPOD_CUST_I_CODE, CPOD_CUST_I_NAME, CPOD_ST_CODE, CPOD_CURR_CODE,
            AMD_AM_CODE, CPOD_MODNO, CPOD_MODDATE, CPOD_AMORTRATE
        )
        SELECT
            CPOD_CPOM_CODE, CPOD_I_CODE, CPOD_UOM_CODE, CPOD_ORD_QTY,
            CPOD_RATE, CPOD_AMT, CPOD_STATUS, CPOD_DISPACH, CPOD_DESC,
            CPOD_CUST_I_CODE, CPOD_CUST_I_NAME, CPOD_ST_CODE, CPOD_CURR_CODE,
            @AmCode, CPOD_MODNO, CPOD_MODDATE, CPOD_AMORTRATE
        FROM CUSTPO_DETAIL
        WHERE CPOD_CPOM_CODE = @PoCode;

        -- 4. Update master with new field values, increment amendment counter
        UPDATE CUSTPO_MASTER SET
            CPOM_P_CODE         = @CustomerCode,
            CPOM_PONO           = @PoNumber,
            CPOM_TYPE           = @PoType,
            CPOM_DATE           = @PoDate,
            CPOM_CR_DAYS        = @CreditDays,
            CPOM_WORK_ODR_NO    = @WorkOrderNumber,
            CPOM_PAY_TERM       = @PaymentTerms,
            CPOM_AUTH_FLG       = @IsAuthorized,
            CPOM_PO_DATE        = @CustomerPoDate,
            CPOM_QE_CODE        = @QuotationCode,
            CPOM_T_NAME         = @TaxName,
            CPOM_T_PER          = @TaxPercentage,
            CPOM_T_AMT          = @TaxAmount,
            CPOM_EXC_PER        = @ExcisePercentage,
            CPOM_EXC_EDU_PER    = @ExciseEducationPercentage,
            CPOM_EXC_HEDU_PER   = @ExciseHigherEducationPercentage,
            CPOM_BASIC_AMT      = @BasicAmount,
            CPOM_DISCOUNT_PER   = @DiscountPercentage,
            CPOM_DISCOUNT_AMT   = @DiscountAmount,
            CPOM_DISCOUNT_REASON= @DiscountReason,
            CPOM_DEVIATION_AMT  = @DeviationAmount,
            CPOM_DEVIATION_REASON= @DeviationReason,
            CPOM_PACKING_AMT    = @PackingAmount,
            CPOM_EXC_AMT        = @ExciseAmount,
            CPOM_ROUNDING       = @RoundingAmount,
            CPOM_GRAND_TOT      = @GrandTotal,
            CPOM_FINAL_DEST     = @FinalDestination,
            CPOM_PRE_CARR_BY    = @PreCarriageBy,
            CPOM_PORT_LOAD      = @PortOfLoading,
            CPOM_PORT_DIS       = @PortOfDischarge,
            CPOM_PLACE_DEL      = @PlaceOfDelivery,
            CPOM_BUYER_NAME     = @BuyerName,
            CPOM_BUYER_ADD      = @BuyerAddress,
            CPOM_CURR_CODE      = @CurrencyCode,
            CPOM_INQ_CODE       = @InquiryCode,
            CPOM_IS_VERBAL      = @IsVerbalOrder,
            CPOM_PROJECT_CODE   = @ProjectCode,
            CPOM_PROJECT_NAME   = @ProjectName,
            CPOM_AM_COUNT       = @AmendCount,
            CPOM_AM_DATE        = GETDATE(),
            MODIFY              = 0   -- release lock (mirrors RemoveModifyLock)
        WHERE CPOM_CODE = @PoCode AND CPOM_CM_COMP_ID = @CompanyId;

        -- 5. Delete existing detail lines (caller re-inserts via ERP_CreateCustomerPoDetail)
        DELETE FROM CUSTPO_DETAIL WHERE CPOD_CPOM_CODE = @PoCode;

        -- Return the amendment count for confirmation
        SELECT @AmendCount AS AmendCount;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
