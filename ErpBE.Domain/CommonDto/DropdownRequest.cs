namespace ErpBE.Domain.CommonDto
{
    public class DropdownRequest
    {
        public string Table { get; set; }              // e.g., "Countries"
        public string IdColumn { get; set; }           // e.g., "CountryId"
        public string DisplayColumn { get; set; }      // e.g., "CountryName"
        public string Where { get; set; }              // Optional, e.g., "IsActive = 1"
        public string OrderBy { get; set; }            // Optional, e.g., "CountryName ASC"
    }
}
