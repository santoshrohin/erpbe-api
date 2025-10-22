namespace ErpBE.Domain.DTOs
{
    public class PlacementWithDetailsDto
    {
        public string FarmerName { get; set; } = string.Empty;
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerAddress { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string LineName { get; set; } = string.Empty;
        public int PlacementQty { get; set; }
        public DateTime PlacementDate { get; set; }
        public bool Status { get; set; }
    }

}
