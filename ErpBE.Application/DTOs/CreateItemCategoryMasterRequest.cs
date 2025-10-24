namespace ErpBE.Application.DTOs
{
    public class CreateItemCategoryMasterRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public bool IsAutoShortClose { get; set; }
        public bool IsActive { get; set; } = true;
    }
}


