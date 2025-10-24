namespace ErpBE.Application.Common.Models
{
    public class CustomerMasterQueryParameters : QueryParameters
    {
        public int CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public int? AreaCode { get; set; }
        public string? CustomerType { get; set; }
        public int? StateCode { get; set; }
        public int? CityCode { get; set; }
        public int? CategoryCode { get; set; }
    }
}

