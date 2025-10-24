-- =============================================
-- Stored Procedure: ERP_UpdateCustomerMaster
-- Description: Updates an existing Customer Master record
-- =============================================
CREATE OR ALTER PROCEDURE ERP_UpdateCustomerMaster
    @Id INT,
    @CompanyId INT,
    @PartyCode NVARCHAR(50),
    @PartyName NVARCHAR(500),
    @ContactPerson NVARCHAR(75) = NULL,
    @Abbreviation NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(50) = NULL,
    @Mobile NVARCHAR(55) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Website NVARCHAR(100) = NULL,
    @FaxNo NVARCHAR(50) = NULL,
    @AreaCode INT,
    @CustomerType INT,
    @CountryCode INT = NULL,
    @StateCode INT = NULL,
    @CityCode INT = NULL,
    @PinCode NVARCHAR(15) = NULL,
    @VatTinNo NVARCHAR(50) = NULL,
    @CstNo NVARCHAR(50) = NULL,
    @GstNo NVARCHAR(50) = NULL,
    @PanNo NVARCHAR(25) = NULL,
    @ServiceTaxNo NVARCHAR(50) = NULL,
    @TallyName NVARCHAR(200) = NULL,
    @OpeningBalance DECIMAL(18,2) = NULL,
    @OpeningBalanceType NVARCHAR(10) = NULL,
    @CreditLimit DECIMAL(18,2) = NULL,
    @CreditDays INT = NULL,
    @BankName NVARCHAR(100) = NULL,
    @BankAccountNo NVARCHAR(50) = NULL,
    @BankBranchName NVARCHAR(100) = NULL,
    @BankIfscCode NVARCHAR(20) = NULL,
    @IsLbtApplicable BIT = NULL,
    @IsSezCustomer BIT = NULL,
    @IsCompositeDealer BIT = NULL,
    @Remark NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT OFF;

    UPDATE PARTY_MASTER
    SET 
        P_PARTY_CODE = @PartyCode,
        P_NAME = @PartyName,
        P_CONTACT = @ContactPerson,
        P_ABBREVATION = @Abbreviation,
        P_ADDRESS = @Address,
        P_PHONE = @Phone,
        P_MOB = @Mobile,
        P_EMAIL = @Email,
        P_WEBSITE = @Website,
        P_FAX_NO = @FaxNo,
        P_A_CODE = @AreaCode,
        P_CUST_TYPE = @CustomerType,
        P_COUNTRY_CODE = @CountryCode,
        P_STATE_CODE = @StateCode,
        P_CITY_CODE = @CityCode,
        P_PIN_CODE = @PinCode,
        P_VAT_TIN_NO = @VatTinNo,
        P_CST_NO = @CstNo,
        P_GST_NO = @GstNo,
        P_PAN = @PanNo,
        P_SERVICE_TAX_NO = @ServiceTaxNo,
        P_TALLY_NAME = @TallyName,
        P_OP_BAL = @OpeningBalance,
        P_OP_BAL_TYPE = @OpeningBalanceType,
        P_CREDIT_LIMIT = @CreditLimit,
        P_CREDIT_DAYS = @CreditDays,
        P_BANK_NAME = @BankName,
        P_BANK_AC_NO = @BankAccountNo,
        P_BANK_BRANCH = @BankBranchName,
        P_BANK_IFSC_CODE = @BankIfscCode,
        P_LBT_APPLICABLE = @IsLbtApplicable,
        P_SEZ_CUSTOMER = @IsSezCustomer,
        P_COMPOSITE_DEALER = @IsCompositeDealer,
        P_REMARK = @Remark,
        ES_MODIFY_DATE = GETDATE()
    WHERE P_CODE = @Id 
        AND P_C_CODE = @CompanyId
        AND P_TYPE = 1
        AND ES_DELETE = 0;
END
GO

