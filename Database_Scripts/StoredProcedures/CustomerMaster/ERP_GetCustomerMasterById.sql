-- =============================================
-- Stored Procedure: ERP_GetCustomerMasterById
-- Description: Retrieves a single Customer Master record by ID
-- =============================================
CREATE OR ALTER PROCEDURE ERP_GetCustomerMasterById
    @Id INT
AS
BEGIN
    SET NOCOUNT OFF;

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
    WHERE pm.P_CODE = @Id
        AND pm.P_TYPE = 1
        AND pm.ES_DELETE = 0;
END
GO

