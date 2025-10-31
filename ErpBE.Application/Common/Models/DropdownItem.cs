namespace ErpBE.Application.Common.Models
{
    public class DropdownItem
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        // Optionally support string IDs
        public string IdRaw { get; set; }

        // For paging responses
        public int? TotalCount { get; set; }
    }
}
