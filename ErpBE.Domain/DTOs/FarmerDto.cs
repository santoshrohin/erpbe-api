namespace ErpBE.Domain.DTOs
{
    public class FarmerDto
    {
        public int FarmerId { get; set; }
        public string FarmerName { get; set; }
        public string FarmerCode { get; set; }
        public string FarmerAddress { get; set; }
        public int BranchId { get; set; }
        public int LineId { get; set; }
    }
}
