namespace ErpBE.Application.Common.Models
{
    public class CustomerTypeMasterQueryParameters : QueryParameters
    {
        public int? CompanyId { get; set; }
        public string? TypeCode { get; set; }
        public string? TypeDescription { get; set; }
        public string? FirstLetter { get; set; }
    }
}

