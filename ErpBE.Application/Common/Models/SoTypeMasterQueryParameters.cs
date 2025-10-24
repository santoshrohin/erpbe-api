namespace ErpBE.Application.Common.Models
{
    public class SoTypeMasterQueryParameters : QueryParameters
    {
        public int? CompanyId { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
    }
}

