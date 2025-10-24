namespace ErpBE.Application.DTOs
{
    public class SoTypeMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FirstLetter { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
    }
}

