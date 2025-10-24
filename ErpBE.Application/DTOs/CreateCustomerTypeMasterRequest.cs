namespace ErpBE.Application.DTOs
{
    public class CreateCustomerTypeMasterRequest
    {
        public int CompanyId { get; set; }
        public string TypeCode { get; set; } = string.Empty;
        public string TypeDescription { get; set; } = string.Empty;
        public string FirstLetter { get; set; } = string.Empty;
    }
}

