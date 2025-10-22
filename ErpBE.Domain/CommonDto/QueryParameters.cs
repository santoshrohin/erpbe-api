namespace ErpBE.Domain.CommonDto
{
    public class QueryParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Sorting
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc"; // "asc" or "desc"
        
        // Filtering - Key-value pairs for column filters
        public Dictionary<string, string>? Filters { get; set; }
        
        // Global Search
        public string? SearchTerm { get; set; }
        
        // Validation
        public virtual void Validate()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            if (PageSize > 100) PageSize = 100; // Max page size limit
            
            if (!string.IsNullOrEmpty(SortDirection))
            {
                SortDirection = SortDirection.ToLower();
                if (SortDirection != "asc" && SortDirection != "desc")
                    SortDirection = "asc";
            }
        }
    }
}
