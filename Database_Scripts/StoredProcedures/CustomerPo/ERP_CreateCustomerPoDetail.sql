IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ERP_CreateCustomerPoDetail')
    DROP PROCEDURE [dbo].[ERP_CreateCustomerPoDetail]
GO

CREATE PROCEDURE [dbo].[ERP_CreateCustomerPoDetail]
    @PoCode INT,
    @ItemCode INT,
    @UomCode INT,
    @OrderedQuantity FLOAT,
    @Rate FLOAT,
    @Amount FLOAT,
    @Description VARCHAR(MAX) = NULL,
    @CustomerItemCode VARCHAR(MAX) = NULL,
    @CustomerItemName VARCHAR(MAX) = NULL,
    @Status INT = 0,
    @DispatchedQuantity FLOAT = 0,
    @IsOrder BIT = 0,
    @StoreCode INT = NULL,
    @CurrencyCode INT = NULL,
    @WorkOrderQuantity FLOAT = NULL,
    @ModificationNumber VARCHAR(50) = NULL,
    @ModificationDate DATETIME = NULL,
    @AmortizationRate FLOAT = NULL,
    @DieAmortizationRate FLOAT = NULL,
    @DiscountPercentage FLOAT = NULL,
    @DiscountAmount FLOAT = NULL,
    @TaxCategoryCode INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO CUSTPO_DETAIL (
            CPOD_CPOM_CODE,
            CPOD_I_CODE,
            CPOD_UOM_CODE,
            CPOD_ORD_QTY,
            CPOD_RATE,
            CPOD_AMT,
            CPOD_DESC,
            CPOD_CUST_I_CODE,
            CPOD_CUST_I_NAME,
            CPOD_STATUS,
            CPOD_DISPACH,
            CPOD_IS_ORDER,
            CPOD_ST_CODE,
            CPOD_CURR_CODE,
            CPOD_WO_QTY,
            CPOD_MODNO,
            CPOD_MODDATE,
            CPOD_AMORTRATE,
            CPOD_DIEAMORTRATE,
            CPOD_DISC_PER,
            CPOD_DISC_AMT
        )
        VALUES (
            @PoCode,
            @ItemCode,
            @UomCode,
            @OrderedQuantity,
            @Rate,
            @Amount,
            @Description,
            @CustomerItemCode,
            @CustomerItemName,
            @Status,
            @DispatchedQuantity,
            @IsOrder,
            @TaxCategoryCode,
            @CurrencyCode,
            @WorkOrderQuantity,
            @ModificationNumber,
            @ModificationDate,
            @AmortizationRate,
            @DieAmortizationRate,
            @DiscountPercentage,
            @DiscountAmount
        );

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

