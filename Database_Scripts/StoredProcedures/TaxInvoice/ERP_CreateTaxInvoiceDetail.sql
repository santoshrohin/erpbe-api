-- =============================================
-- Author:      AI Assistant
-- Create date: 2025-01-24
-- Description: Creates a Tax Invoice Detail line item
-- =============================================
ALTER PROCEDURE [dbo].[ERP_CreateTaxInvoiceDetail]
    @InvoiceMasterCode BIGINT,
    @ItemCode INT,
    @UomCode INT,
    @CustomerPoCode INT = NULL,
    @InvoiceQuantity FLOAT,
    @Rate FLOAT = NULL,
    @ConversionQuantity FLOAT = NULL,
    @AmortizationRate FLOAT = NULL,
    @NumberOfPackages INT = NULL,
    @PackageDescription VARCHAR(200) = NULL,
    @QuantityPerPack DECIMAL(18,2) = NULL,
    @Amount FLOAT = NULL,
    @DeliveryChallanNumbers VARCHAR(500) = NULL,
    @DeliveryChallanDates VARCHAR(500) = NULL,
    @ExciseNumbers VARCHAR(500) = NULL,
    @ProcessCode INT = NULL,
    @GinNumber VARCHAR(50) = NULL,
    @GinDate DATETIME = NULL,
    @GinReceipt FLOAT = NULL,
    @MrCode DECIMAL(18,2) = NULL,
    @GinAcceptance FLOAT = NULL,
    @ExciseAmount FLOAT = NULL,
    @EducationCessAmount FLOAT = NULL,
    @SecondaryHigherEducationCessAmount FLOAT = NULL,
    @CgstPercentage FLOAT = NULL,
    @SgstPercentage FLOAT = NULL,
    @IgstPercentage FLOAT = NULL,
    @SerialNumber VARCHAR(100) = NULL,
    @Remarks VARCHAR(500) = NULL,
    @ItemWarehouseCode INT = NULL,
    @ActualWeight FLOAT = NULL,
    @Size VARCHAR(50) = NULL,
    @SubHeading VARCHAR(200) = NULL,
    @BatchNumber VARCHAR(50) = NULL,
    @PackingQuantity FLOAT = NULL,
    @GrossWeight FLOAT = NULL,
    @NetWeight FLOAT = NULL,
    @SizeOfBox FLOAT = NULL,
    @NumberOfBarrels FLOAT = NULL,
    @NumberOfPackagesDescription VARCHAR(200) = NULL,
    @ContainerNumber VARCHAR(50) = NULL,
    @RefundableQuantity FLOAT = NULL,
    @AmortRate FLOAT = NULL,
    @AmortAmount FLOAT = NULL,
    @HsnCode VARCHAR(50) = NULL,
    @StoreCode INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Insert into INVOICE_DETAIL
        INSERT INTO INVOICE_DETAIL (
            IND_INM_CODE, IND_I_CODE, IND_UOM_CODE, IND_CPOM_CODE, IND_INQTY,
            IND_RATE, IND_CON_QTY, IND_AMORT_RATE, IND_NO_PACK, IND_PACK_DESC,
            IND_QTY_PACK, IND_AMT, IND_DC_NO, IND_DC_DATE, IND_EX_NO,
            IND_PROCESS_CODE, IND_GIN_NO, IND_GIN_DATE, IND_GIN_RCPT, IND_MR_CODE,
            IND_GIN_ACCP, IND_EX_AMT, IND_E_CESS_AMT, IND_SH_CESS_AMT, E_BASIC_CentralT,
            E_EDU_CESS_State, E_H_EDU_Integrated, IND_SR_NO, IND_REMARK, IND_IWM_CODE,
            IND_ACT_WEIGHT, IND_SIZE, IND_SUBHEADING, IND_BACHNO, IND_PAK_QTY,
            IND_GROSS_WEIGHT, IND_NET_WEIGHT, IND_SIZE_OF_BOX, IND_NO_OF_BARRELS, IND_NO_OF_PACK_DESC,
            IND_CONTAINER_NO, IND_REFUNDABLE_QTY, IND_AMORTRATE, IND_AMORTAMT, IND_HSN_CODE,
            IND_STORE_CODE, ES_DELETE
        )
        VALUES (
            @InvoiceMasterCode, @ItemCode, @UomCode, @CustomerPoCode, @InvoiceQuantity,
            @Rate, @ConversionQuantity, @AmortizationRate, @NumberOfPackages, @PackageDescription,
            @QuantityPerPack, @Amount, @DeliveryChallanNumbers, @DeliveryChallanDates, @ExciseNumbers,
            @ProcessCode, @GinNumber, @GinDate, @GinReceipt, @MrCode,
            @GinAcceptance, @ExciseAmount, @EducationCessAmount, @SecondaryHigherEducationCessAmount, @CgstPercentage,
            @SgstPercentage, @IgstPercentage, @SerialNumber, @Remarks, @ItemWarehouseCode,
            @ActualWeight, @Size, @SubHeading, @BatchNumber, @PackingQuantity,
            @GrossWeight, @NetWeight, @SizeOfBox, @NumberOfBarrels, @NumberOfPackagesDescription,
            @ContainerNumber, @RefundableQuantity, @AmortRate, @AmortAmount, @HsnCode,
            @StoreCode, 0
        );

        -- Update dispatched qty in CUSTPO_DETAIL when a PO line is referenced
        IF @CustomerPoCode IS NOT NULL
        BEGIN
            UPDATE CUSTPO_DETAIL
            SET CPOD_DISPACH = ISNULL(CPOD_DISPACH, 0) + @InvoiceQuantity
            WHERE CPOD_CPOM_CODE = @CustomerPoCode
              AND CPOD_I_CODE    = @ItemCode;
        END

        -- Manage Stock only for TAXINV (Labour invoices have no stock impact)
        DECLARE @InvoiceDate DATETIME;
        DECLARE @InvoiceType NVARCHAR(20);
        SELECT @InvoiceDate = INM_DATE, @InvoiceType = INM_TYPE
        FROM INVOICE_MASTER WHERE INM_CODE = @InvoiceMasterCode;

        IF @InvoiceType = 'TAXINV'
        BEGIN
            EXEC ERP_ManageTaxInvoiceStock
                @Operation = 'INSERT',
                @InvoiceCode = @InvoiceMasterCode,
                @InvoiceDate = @InvoiceDate,
                @ItemCode = @ItemCode,
                @Quantity = @InvoiceQuantity;
        END

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

