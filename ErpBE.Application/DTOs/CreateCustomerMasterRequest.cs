namespace ErpBE.Application.DTOs
{
    public class CreateCustomerMasterRequest
    {
        public int CompanyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Abbreviation { get; set; }
        public string? VendorCode { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? FaxNo { get; set; }
        public string? PinCode { get; set; }
        
        // Foreign Keys
        public int AreaCode { get; set; }
        public string CustomerType { get; set; } = string.Empty;
        public int? CountryCode { get; set; }
        public int? StateCode { get; set; }
        public int? CityCode { get; set; }
        public int? CategoryCode { get; set; }
        public int? EmployeeCode { get; set; }
        
        // Tax Numbers
        public string? PanNo { get; set; }
        public string? CstNo { get; set; }
        public string? VatNo { get; set; }
        public string? ServiceTaxNo { get; set; }
        public string? EccNo { get; set; }
        public string? LbtNo { get; set; }
        
        // Excise Details
        public string? ExciseRange { get; set; }
        public string? ExciseDivision { get; set; }
        public string? ExciseCollectorate { get; set; }
        
        // Other Details
        public string? TallyName { get; set; }
        public int? CreditDays { get; set; }
        public double? TdsPercentage { get; set; }
        
        // Flags
        public bool? IsActive { get; set; }
        public bool? IsLbtApplicable { get; set; }
    }
}
