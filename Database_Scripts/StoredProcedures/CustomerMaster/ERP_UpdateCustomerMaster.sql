-- =============================================
-- Update Customer Master
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_UpdateCustomerMaster]
    @Id INT,
    @CompanyId INT,
    @PartyCode INT,
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
    @IsLbtApplicable BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        UPDATE PARTY_MASTER
        SET 
            P_TYPE = 1,
            P_NAME = @PartyName,
            P_CONTACT = ISNULL(@ContactPerson, ''),
            P_ABBREVATION = ISNULL(@Abbreviation, ''),
            P_VEND_CODE = @VendorCode,
            P_ADD1 = ISNULL(@Address, ''),
            P_PHONE = @Phone,
            P_MOB = @Mobile,
            P_EMAIL = @Email,
            P_FAX = @FaxNo,
            P_PIN_CODE = @PinCode,
            P_A_CODE = @AreaCode,
            P_CUST_TYPE = @CustomerType,
            P_COUNTRY_CODE = @CountryCode,
            P_SM_CODE = @StateCode,
            P_CITY_CODE = @CityCode,
            P_CATEGORY = @CategoryCode,
            P_E_CODE = @EmployeeCode,
            P_PAN = @PanNo,
            P_CST = @CstNo,
            P_VAT = @VatNo,
            P_SER_TAX_NO = @ServiceTaxNo,
            P_ECC_NO = @EccNo,
            P_LBT_NO = @LbtNo,
            P_EXC_RANGE = @ExciseRange,
            P_EXC_DIV = @ExciseDivision,
            P_EXC_COLLECTORATE = @ExciseCollectorate,
            P_TALLY = @TallyName,
            P_CREDITDAYS = @CreditDays,
            P_TDS = @TdsPercentage,
            P_ACTIVE_IND = @IsActive,
            P_LBT_IND = @IsLbtApplicable
        WHERE P_CODE = @Id AND P_CM_COMP_ID = @CompanyId;
        
        IF @@ROWCOUNT = 0
        BEGIN
            THROW 50001, 'Customer not found or access denied.', 1;
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        THROW;
    END CATCH
END

