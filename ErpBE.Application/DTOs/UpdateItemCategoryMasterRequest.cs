namespace ErpBE.Application.DTOs
{
    public class UpdateItemCategoryMasterRequest
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsAutoShortClose { get; set; }
        public bool IsActive { get; set; }
    }
}


