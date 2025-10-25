CREATE PROCEDURE [dbo].[ERP_UpdateCustomerPo]
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
    SET NOCOUNT OFF;

    BEGIN TRY
        -- Update master record
        UPDATE CUSTPO_MASTER
        SET 
            CPOM_P_CODE = @CustomerCode,
            CPOM_PONO = @PoNumber,
            CPOM_TYPE = @PoType,
            CPOM_DATE = @PoDate,
            CPOM_CR_DAYS = @CreditDays,
            CPOM_CM_COMP_ID = @CompanyId,
            CPOM_WORK_ODR_NO = @WorkOrderNumber,
            CPOM_PAY_TERM = @PaymentTerms,
            CPOM_AUTH_FLG = @IsAuthorized,
            CPOM_PO_DATE = @CustomerPoDate,
            CPOM_QE_CODE = @QuotationCode,
            CPOM_T_NAME = @TaxName,
            CPOM_T_PER = @TaxPercentage,
            CPOM_T_AMT = @TaxAmount,
            CPOM_EXC_PER = @ExcisePercentage,
            CPOM_EXC_EDU_PER = @ExciseEducationPercentage,
            CPOM_EXC_HEDU_PER = @ExciseHigherEducationPercentage,
            CPOM_BASIC_AMT = @BasicAmount,
            CPOM_DISCOUNT_PER = @DiscountPercentage,
            CPOM_DISCOUNT_AMT = @DiscountAmount,
            CPOM_DISCOUNT_REASON = @DiscountReason,
            CPOM_DEVIATION_AMT = @DeviationAmount,
            CPOM_DEVIATION_REASON = @DeviationReason,
            CPOM_PACKING_AMT = @PackingAmount,
            CPOM_EXC_AMT = @ExciseAmount,
            CPOM_ROUNDING = @RoundingAmount,
            CPOM_GRAND_TOT = @GrandTotal,
            CPOM_FINAL_DEST = @FinalDestination,
            CPOM_PRE_CARR_BY = @PreCarriageBy,
            CPOM_PORT_LOAD = @PortOfLoading,
            CPOM_PORT_DIS = @PortOfDischarge,
            CPOM_PLACE_DEL = @PlaceOfDelivery,
            CPOM_BUYER_NAME = @BuyerName,
            CPOM_BUYER_ADD = @BuyerAddress,
            CPOM_CURR_CODE = @CurrencyCode,
            CPOM_INQ_CODE = @InquiryCode,
            CPOM_IS_VERBAL = @IsVerbalOrder,
            CPOM_PROJECT_CODE = @ProjectCode,
            CPOM_PROJECT_NAME = @ProjectName,
            CPOM_AM_COUNT = CPOM_AM_COUNT + 1,
            CPOM_AM_DATE = GETDATE()
        WHERE 
            CPOM_CODE = @PoCode 
            AND CPOM_CM_COMP_ID = @CompanyId
            AND ES_DELETE = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

