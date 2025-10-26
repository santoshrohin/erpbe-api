-- =============================================
-- Create Customer Master
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_CreateCustomerMaster]
    @CompanyId INT,
    @PartyName NVARCHAR(500),
    @ContactPerson NVARCHAR(75) = NULL,
    @Abbreviation NVARCHAR(20) = NULL,
    @VendorCode NVARCHAR(30) = NULL,
    @Address NVARCHAR(255) = NULL,
    @Phone NVARCHAR(50) = NULL,
    @Mobile NVARCHAR(55) = NULL,
    @Email NVARCHAR(100) = NULL,
    @FaxNo NVARCHAR(50) = NULL,
    @PinCode NVARCHAR(15) = NULL,
    @AreaCode INT,
    @CustomerType VARCHAR(20),
    @CountryCode INT = NULL,
    @StateCode INT = NULL,
    @CityCode INT = NULL,
    @CategoryCode INT = NULL,
    @EmployeeCode INT = NULL,
    @PanNo NVARCHAR(25) = NULL,
    @CstNo NVARCHAR(50) = NULL,
    @VatNo NVARCHAR(50) = NULL,
    @ServiceTaxNo NVARCHAR(50) = NULL,
    @EccNo NVARCHAR(50) = NULL,
    @LbtNo NVARCHAR(50) = NULL,
    @ExciseRange NVARCHAR(50) = NULL,
    @ExciseDivision NVARCHAR(50) = NULL,
    @ExciseCollectorate NVARCHAR(50) = NULL,
    @TallyName NVARCHAR(MAX) = NULL,
    @CreditDays INT = NULL,
    @TdsPercentage FLOAT = NULL,
    @IsActive BIT = 1,
    @IsLbtApplicable BIT = 0,
    @NewId INT OUTPUT,
    @NewPartyCode INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Generate new P_PARTY_CODE
        DECLARE @MaxPartyCode INT;
        SELECT @MaxPartyCode = ISNULL(MAX(P_PARTY_CODE), 0) + 1
        FROM PARTY_MASTER
        WHERE P_CM_COMP_ID = @CompanyId;
        
        SET @NewPartyCode = @MaxPartyCode;
        
        -- Insert new customer (use ISNULL for non-nullable columns)
        INSERT INTO PARTY_MASTER (
            P_CM_COMP_ID, P_PARTY_CODE, P_TYPE, P_NAME, P_CONTACT, 
            P_ABBREVATION, P_VEND_CODE, P_ADD1, P_PHONE, P_MOB, 
            P_EMAIL, P_FAX, P_PIN_CODE, P_A_CODE, P_CUST_TYPE, 
            P_COUNTRY_CODE, P_SM_CODE, P_CITY_CODE, P_CATEGORY, P_E_CODE,
            P_PAN, P_CST, P_VAT, P_SER_TAX_NO, P_ECC_NO, P_LBT_NO,
            P_EXC_RANGE, P_EXC_DIV, P_EXC_COLLECTORATE, P_TALLY,
            P_CREDITDAYS, P_TDS, P_ACTIVE_IND, P_LBT_IND
        )
        VALUES (
            @CompanyId, @NewPartyCode, 1, @PartyName, ISNULL(@ContactPerson, ''),
            ISNULL(@Abbreviation, ''), @VendorCode, ISNULL(@Address, ''), @Phone, @Mobile,
            @Email, @FaxNo, @PinCode, @AreaCode, @CustomerType,
            @CountryCode, @StateCode, @CityCode, @CategoryCode, @EmployeeCode,
            @PanNo, @CstNo, @VatNo, @ServiceTaxNo, @EccNo, @LbtNo,
            @ExciseRange, @ExciseDivision, @ExciseCollectorate, @TallyName,
            @CreditDays, @TdsPercentage, @IsActive, @IsLbtApplicable
        );
        
        SET @NewId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        THROW;
    END CATCH
END

