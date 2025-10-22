namespace ErpBE.Domain.CommonDto
{
    public class FarmerQueryParameters : QueryParameters
    {
        public int? BranchId { get; set; }
        public int? LineId { get; set; }
        public string? FarmerCode { get; set; }
        public string? FarmerName { get; set; }
        
        public override void Validate()
        {
            base.Validate();
            
            // Add any farmer-specific validation here
            if (BranchId.HasValue && BranchId < 0)
                BranchId = null;
                
            if (LineId.HasValue && LineId < 0)
                LineId = null;
        }
    }
}
