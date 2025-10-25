CREATE PROCEDURE [dbo].[ERP_CreateCustomerPo]
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
        DECLARE @NewPoCode INT;

        -- Get next document number
        DECLARE @DocNumber INT;
        SELECT @DocNumber = ISNULL(MAX(CPOM_DOC_NO), 0) + 1 
        FROM CUSTPO_MASTER 
        WHERE CPOM_CM_COMP_ID = @CompanyId;

        -- Insert master record
        INSERT INTO CUSTPO_MASTER (
            CPOM_P_CODE,
            CPOM_PONO,
            CPOM_DOC_NO,
            CPOM_TYPE,
            CPOM_DATE,
            CPOM_CR_DAYS,
            CPOM_CM_COMP_ID,
            CPOM_WORK_ODR_NO,
            CPOM_PAY_TERM,
            CPOM_AUTH_FLG,
            CPOM_PO_DATE,
            CPOM_QE_CODE,
            CPOM_T_NAME,
            CPOM_T_PER,
            CPOM_T_AMT,
            CPOM_EXC_PER,
            CPOM_EXC_EDU_PER,
            CPOM_EXC_HEDU_PER,
            CPOM_BASIC_AMT,
            CPOM_DISCOUNT_PER,
            CPOM_DISCOUNT_AMT,
            CPOM_DISCOUNT_REASON,
            CPOM_DEVIATION_AMT,
            CPOM_DEVIATION_REASON,
            CPOM_PACKING_AMT,
            CPOM_EXC_AMT,
            CPOM_ROUNDING,
            CPOM_GRAND_TOT,
            CPOM_INV_FLAG,
            CPOM_AM_COUNT,
            CPOM_FINAL_DEST,
            CPOM_PRE_CARR_BY,
            CPOM_PORT_LOAD,
            CPOM_PORT_DIS,
            CPOM_PLACE_DEL,
            CPOM_BUYER_NAME,
            CPOM_BUYER_ADD,
            CPOM_CURR_CODE,
            CPOM_INQ_CODE,
            CPOM_IS_VERBAL,
            CPOM_PROJECT_CODE,
            CPOM_PROJECT_NAME,
            MODIFY,
            ES_DELETE
        )
        VALUES (
            @CustomerCode,
            @PoNumber,
            @DocNumber,
            @PoType,
            @PoDate,
            @CreditDays,
            @CompanyId,
            @WorkOrderNumber,
            @PaymentTerms,
            @IsAuthorized,
            @CustomerPoDate,
            @QuotationCode,
            @TaxName,
            @TaxPercentage,
            @TaxAmount,
            @ExcisePercentage,
            @ExciseEducationPercentage,
            @ExciseHigherEducationPercentage,
            @BasicAmount,
            @DiscountPercentage,
            @DiscountAmount,
            @DiscountReason,
            @DeviationAmount,
            @DeviationReason,
            @PackingAmount,
            @ExciseAmount,
            @RoundingAmount,
            @GrandTotal,
            0, -- INV_FLAG
            0, -- AM_COUNT
            @FinalDestination,
            @PreCarriageBy,
            @PortOfLoading,
            @PortOfDischarge,
            @PlaceOfDelivery,
            @BuyerName,
            @BuyerAddress,
            @CurrencyCode,
            @InquiryCode,
            @IsVerbalOrder,
            @ProjectCode,
            @ProjectName,
            0, -- MODIFY
            0  -- ES_DELETE
        );

        SET @NewPoCode = SCOPE_IDENTITY();
        
        SELECT @NewPoCode AS PoCode;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

