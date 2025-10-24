namespace ErpBE.Application.DTOs
{
    public class UpdateCustomerMasterRequest
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string PartyCode { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Abbreviation { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? FaxNo { get; set; }
        public int AreaCode { get; set; }
        public int CustomerType { get; set; }
        public int? CountryCode { get; set; }
        public int? StateCode { get; set; }
        public int? CityCode { get; set; }
        public string? PinCode { get; set; }
        public string? VatTinNo { get; set; }
        public string? CstNo { get; set; }
        public string? GstNo { get; set; }
        public string? PanNo { get; set; }
        public string? ServiceTaxNo { get; set; }
        public string? TallyName { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string? OpeningBalanceType { get; set; }
        public decimal? CreditLimit { get; set; }
        public int? CreditDays { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankBranchName { get; set; }
        public string? BankIfscCode { get; set; }
        public bool? IsLbtApplicable { get; set; }
        public bool? IsSezCustomer { get; set; }
        public bool? IsCompositeDealer { get; set; }
        public string? Remark { get; set; }
    }
}

