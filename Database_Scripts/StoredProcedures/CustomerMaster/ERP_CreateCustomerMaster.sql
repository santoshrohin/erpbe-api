-- =============================================
-- Stored Procedure: ERP_CreateCustomerMaster
-- Description: Creates a new Customer Master record with auto-generated Party Code
-- =============================================
CREATE OR ALTER PROCEDURE ERP_CreateCustomerMaster
    @CompanyId INT,
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

    DECLARE @NewId INT;
    DECLARE @PartyCode NVARCHAR(50);

    -- Generate Party Code: MAX(P_PARTY_CODE) + 1
    SELECT @PartyCode = ISNULL(MAX(P_PARTY_CODE), 0) + 1
    FROM PARTY_MASTER
    WHERE P_C_CODE = @CompanyId;

    -- Insert into PARTY_MASTER
    INSERT INTO PARTY_MASTER (
        P_C_CODE,
        P_PARTY_CODE,
        P_NAME,
        P_CONTACT,
        P_ABBREVATION,
        P_ADDRESS,
        P_PHONE,
        P_MOB,
        P_EMAIL,
        P_WEBSITE,
        P_FAX_NO,
        P_A_CODE,
        P_CUST_TYPE,
        P_COUNTRY_CODE,
        P_STATE_CODE,
        P_CITY_CODE,
        P_PIN_CODE,
        P_VAT_TIN_NO,
        P_CST_NO,
        P_GST_NO,
        P_PAN,
        P_SERVICE_TAX_NO,
        P_TALLY_NAME,
        P_OP_BAL,
        P_OP_BAL_TYPE,
        P_CREDIT_LIMIT,
        P_CREDIT_DAYS,
        P_BANK_NAME,
        P_BANK_AC_NO,
        P_BANK_BRANCH,
        P_BANK_IFSC_CODE,
        P_LBT_APPLICABLE,
        P_SEZ_CUSTOMER,
        P_COMPOSITE_DEALER,
        P_REMARK,
        P_TYPE,
        ES_DELETE,
        ES_ACTIVE
    )
    VALUES (
        @CompanyId,
        @PartyCode,
        @PartyName,
        @ContactPerson,
        @Abbreviation,
        @Address,
        @Phone,
        @Mobile,
        @Email,
        @Website,
        @FaxNo,
        @AreaCode,
        @CustomerType,
        @CountryCode,
        @StateCode,
        @CityCode,
        @PinCode,
        @VatTinNo,
        @CstNo,
        @GstNo,
        @PanNo,
        @ServiceTaxNo,
        @TallyName,
        @OpeningBalance,
        @OpeningBalanceType,
        @CreditLimit,
        @CreditDays,
        @BankName,
        @BankAccountNo,
        @BankBranchName,
        @BankIfscCode,
        @IsLbtApplicable,
        @IsSezCustomer,
        @IsCompositeDealer,
        @Remark,
        1, -- P_TYPE = 1 for Customer
        0, -- ES_DELETE = 0 (not deleted)
        1  -- ES_ACTIVE = 1 (active)
    );

    SET @NewId = SCOPE_IDENTITY();

    -- Return the created record with joined data
    SELECT 
        pm.P_CODE AS Id,
        pm.P_C_CODE AS CompanyId,
        pm.P_PARTY_CODE AS PartyCode,
        pm.P_NAME AS PartyName,
        pm.P_CONTACT AS ContactPerson,
        pm.P_ABBREVATION AS Abbreviation,
        pm.P_ADDRESS AS Address,
        pm.P_PHONE AS Phone,
        pm.P_MOB AS Mobile,
        pm.P_EMAIL AS Email,
        pm.P_WEBSITE AS Website,
        pm.P_FAX_NO AS FaxNo,
        pm.P_A_CODE AS AreaCode,
        am.A_NAME AS AreaName,
        pm.P_CUST_TYPE AS CustomerType,
        ctm.CTM_TYPE_DESCRIPTION AS CustomerTypeName,
        pm.P_COUNTRY_CODE AS CountryCode,
        pm.P_STATE_CODE AS StateCode,
        pm.P_CITY_CODE AS CityCode,
        pm.P_PIN_CODE AS PinCode,
        pm.P_VAT_TIN_NO AS VatTinNo,
        pm.P_CST_NO AS CstNo,
        pm.P_GST_NO AS GstNo,
        pm.P_PAN AS PanNo,
        pm.P_SERVICE_TAX_NO AS ServiceTaxNo,
        pm.P_TALLY_NAME AS TallyName,
        pm.P_OP_BAL AS OpeningBalance,
        pm.P_OP_BAL_TYPE AS OpeningBalanceType,
        pm.P_CREDIT_LIMIT AS CreditLimit,
        pm.P_CREDIT_DAYS AS CreditDays,
        pm.P_BANK_NAME AS BankName,
        pm.P_BANK_AC_NO AS BankAccountNo,
        pm.P_BANK_BRANCH AS BankBranchName,
        pm.P_BANK_IFSC_CODE AS BankIfscCode,
        pm.P_LBT_APPLICABLE AS IsLbtApplicable,
        pm.P_SEZ_CUSTOMER AS IsSezCustomer,
        pm.P_COMPOSITE_DEALER AS IsCompositeDealer,
        pm.P_REMARK AS Remark,
        pm.ES_ACTIVE AS IsActive,
        pm.ES_CREATE_DATE AS CreatedDate,
        pm.ES_MODIFY_DATE AS ModifiedDate
    FROM PARTY_MASTER pm
    LEFT JOIN AREA_MASTER am ON pm.P_A_CODE = am.A_CODE
    LEFT JOIN CUSTOMER_TYPE_MASTER ctm ON pm.P_CUST_TYPE = ctm.CTM_CODE
    WHERE pm.P_CODE = @NewId;
END
GO

