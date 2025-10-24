namespace ErpBE.Application.DTOs
{
    public class CustomerTypeMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string TypeCode { get; set; } = string.Empty;
        public string TypeDescription { get; set; } = string.Empty;
        public string FirstLetter { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
    }
}

