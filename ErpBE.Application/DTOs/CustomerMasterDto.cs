namespace ErpBE.Application.DTOs
{
    public class CustomerMasterDto
    {
        public int Id { get; set; }                         // P_CODE
        public int? CompanyId { get; set; }                 // P_CM_COMP_ID
        public int? PartyCode { get; set; }                 // P_PARTY_CODE (auto-generated)
        public string PartyName { get; set; } = string.Empty; // P_NAME
        public string? ContactPerson { get; set; }          // P_CONTACT
        public string? Abbreviation { get; set; }           // P_ABBREVATION
        public string? VendorCode { get; set; }             // P_VEND_CODE
        public string? Address { get; set; }                // P_ADD1
        public string? Phone { get; set; }                  // P_PHONE
        public string? Mobile { get; set; }                 // P_MOB
        public string? Email { get; set; }                  // P_EMAIL
        public string? FaxNo { get; set; }                  // P_FAX
        public string? PinCode { get; set; }                // P_PIN_CODE
        
        // Foreign Keys
        public int? AreaCode { get; set; }                  // P_A_CODE
        public string? AreaName { get; set; }               // From AREA_MASTER.A_DESC
        public string? CustomerType { get; set; }           // P_CUST_TYPE (varchar!)
        public string? CustomerTypeName { get; set; }       // From CUSTOMER_TYPE_MASTER.CTM_TYPE_DESC
        public int? CountryCode { get; set; }               // P_COUNTRY_CODE
        public int? StateCode { get; set; }                 // P_SM_CODE
        public int? CityCode { get; set; }                  // P_CITY_CODE
        public int? CategoryCode { get; set; }              // P_CATEGORY
        public int? EmployeeCode { get; set; }              // P_E_CODE
        
        // Tax Numbers
        public string? PanNo { get; set; }                  // P_PAN
        public string? CstNo { get; set; }                  // P_CST
        public string? VatNo { get; set; }                  // P_VAT
        public string? ServiceTaxNo { get; set; }           // P_SER_TAX_NO
        public string? EccNo { get; set; }                  // P_ECC_NO
        public string? LbtNo { get; set; }                  // P_LBT_NO (GST Number)
        
        // Excise Details
        public string? ExciseRange { get; set; }            // P_EXC_RANGE
        public string? ExciseDivision { get; set; }         // P_EXC_DIV
        public string? ExciseCollectorate { get; set; }     // P_EXC_COLLECTORATE
        
        // Other Details
        public string? TallyName { get; set; }              // P_TALLY
        public int? CreditDays { get; set; }                // P_CREDITDAYS
        public double? TdsPercentage { get; set; }          // P_TDS
        
        // Flags
        public bool? IsActive { get; set; }                 // P_ACTIVE_IND
        public bool? IsLbtApplicable { get; set; }          // P_LBT_IND
    }
}
