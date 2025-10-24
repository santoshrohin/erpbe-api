namespace ErpBE.Application.Common.Models
{
    public class ItemCategoryMasterQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SortBy { get; set; } = "I_CAT_NAME";
        public string SortDirection { get; set; } = "ASC";
        public string? SearchTerm { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsAutoShortClose { get; set; }
    }
}


