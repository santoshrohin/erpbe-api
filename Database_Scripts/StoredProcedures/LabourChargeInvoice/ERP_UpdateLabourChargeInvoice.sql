-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-11
-- Description: Updates an existing Labour Charge Invoice (INM_TYPE='OutJWINM')
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_UpdateLabourChargeInvoice]
    @InvoiceCode INT,
    @CompanyCode INT,
    @InvoiceDate DATETIME,
    @InvoiceType TINYINT = NULL,
    @CustomerCode INT,
    @CustomerPoCode INT = NULL,
    @NetAmount FLOAT = NULL,
    @DiscountPercentage FLOAT = NULL,
    @DiscountAmount FLOAT = NULL,
    @ServiceTaxPercentage FLOAT = NULL,
    @ServiceTaxAmount FLOAT = NULL,
    @TcsPercentage FLOAT = NULL,
    @TcsAmount FLOAT = NULL,
    @PackingAmount FLOAT = NULL,
    @GrossAmount FLOAT = NULL,
    @TaxCode INT = NULL,
    @VehicleNumber VARCHAR(50) = NULL,
    @TransportName VARCHAR(200) = NULL,
    @IssueDate DATETIME = NULL,
    @RemovalDate DATETIME = NULL,
    @Remarks VARCHAR(500) = NULL,
    @LrNumber VARCHAR(50) = NULL,
    @LrDate DATETIME = NULL,
    @TaxableAmount FLOAT = NULL,
    @RoundingAmount FLOAT = NULL,
    @OtherAmount FLOAT = NULL,
    @FreightCharges FLOAT = NULL,
    @InsuranceAmount FLOAT = NULL,
    @TransportAmount FLOAT = NULL,
    @OctriAmount FLOAT = NULL,
    @CreditDays INT = NULL,
    @HsnCode VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Clear old STOCK_LEDGER entries so re-inserted details don't double-count stock
        DELETE FROM STOCK_LEDGER
        WHERE STL_DOC_NO   = @InvoiceCode
          AND STL_DOC_TYPE = 'TAXINV';

        -- Update INVOICE_MASTER for OutJWINM type only
        UPDATE INVOICE_MASTER
        SET
            INM_DATE = @InvoiceDate,
            INM_INVOICE_TYPE = @InvoiceType,
            INM_P_CODE = @CustomerCode,
            INM_CPOM_CODE = @CustomerPoCode,
            INM_NET_AMT = @NetAmount,
            INM_DISC = @DiscountPercentage,
            INM_DISC_AMT = @DiscountAmount,
            INM_S_TAX = @ServiceTaxPercentage,
            INM_S_TAX_AMT = @ServiceTaxAmount,
            INM_TAX_TCS = @TcsPercentage,
            INM_TAX_TCS_AMT = @TcsAmount,
            INM_PACK_AMT = @PackingAmount,
            INM_G_AMT = @GrossAmount,
            INM_T_CODE = @TaxCode,
            INM_VEH_NO = @VehicleNumber,
            INM_TRANSPORT = @TransportName,
            INM_ISSUE_DATE = @IssueDate,
            INM_REMOVAL_DATE = @RemovalDate,
            INM_REMARK = @Remarks,
            INM_LR_NO = @LrNumber,
            INM_LR_DATE = @LrDate,
            INM_TAXABLE_AMT = @TaxableAmount,
            INM_ROUNDING_AMT = @RoundingAmount,
            INM_OTHER_AMT = @OtherAmount,
            INM_FREIGHT = @FreightCharges,
            INM_INSURANCE = @InsuranceAmount,
            INM_TRANS_AMT = @TransportAmount,
            INM_OCTRI_AMT = @OctriAmount,
            INM_C_DAYS = @CreditDays,
            INM_HSN_CODE = @HsnCode
        WHERE INM_CODE = @InvoiceCode AND INM_CM_CODE = @CompanyCode AND INM_TYPE = 'OutJWINM';

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
