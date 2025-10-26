-- =============================================
-- Get Customer Master By ID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ERP_GetCustomerMasterById]
    @Id INT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PM.P_CODE AS Id,
        PM.P_CM_COMP_ID AS CompanyId,
        PM.P_PARTY_CODE AS PartyCode,
        PM.P_NAME AS PartyName,
        PM.P_CONTACT AS ContactPerson,
        PM.P_ABBREVATION AS Abbreviation,
        PM.P_VEND_CODE AS VendorCode,
        PM.P_ADD1 AS Address,
        PM.P_PHONE AS Phone,
        PM.P_MOB AS Mobile,
        PM.P_EMAIL AS Email,
        PM.P_FAX AS FaxNo,
        PM.P_PIN_CODE AS PinCode,
        PM.P_A_CODE AS AreaCode,
        AM.A_DESC AS AreaName,
        PM.P_CUST_TYPE AS CustomerType,
        CTM.CTM_TYPE_DESC AS CustomerTypeName,
        PM.P_COUNTRY_CODE AS CountryCode,
        PM.P_SM_CODE AS StateCode,
        PM.P_CITY_CODE AS CityCode,
        PM.P_CATEGORY AS CategoryCode,
        PM.P_E_CODE AS EmployeeCode,
        PM.P_PAN AS PanNo,
        PM.P_CST AS CstNo,
        PM.P_VAT AS VatNo,
        PM.P_SER_TAX_NO AS ServiceTaxNo,
        PM.P_ECC_NO AS EccNo,
        PM.P_LBT_NO AS LbtNo,
        PM.P_EXC_RANGE AS ExciseRange,
        PM.P_EXC_DIV AS ExciseDivision,
        PM.P_EXC_COLLECTORATE AS ExciseCollectorate,
        PM.P_TALLY AS TallyName,
        PM.P_CREDITDAYS AS CreditDays,
        PM.P_TDS AS TdsPercentage,
        PM.P_ACTIVE_IND AS IsActive,
        PM.P_LBT_IND AS IsLbtApplicable
    FROM PARTY_MASTER PM
    LEFT JOIN AREA_MASTER AM ON PM.P_A_CODE = AM.A_CODE AND PM.P_CM_COMP_ID = AM.A_CM_COMP_ID
    LEFT JOIN CUSTOMER_TYPE_MASTER CTM ON PM.P_CUST_TYPE = CTM.CTM_TYPE_CODE AND PM.P_CM_COMP_ID = CTM.CTM_CM_COMP_ID
    WHERE PM.P_CODE = @Id 
      AND PM.P_CM_COMP_ID = @CompanyId
      AND PM.P_TYPE = 1;
END

