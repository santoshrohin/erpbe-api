namespace ErpBE.Domain.CommonDto
{
    public class PlacementQueryParameters : QueryParameters
    {
        public int? FarmerId { get; set; }
        public int? BranchId { get; set; }
        public int? LineId { get; set; }
        public string? FarmerName { get; set; }
        public string? FarmerCode { get; set; }
        public DateTime? PlacementDateFrom { get; set; }
        public DateTime? PlacementDateTo { get; set; }
        public double? MinPlacementQty { get; set; }
        public double? MaxPlacementQty { get; set; }
        public bool? Status { get; set; }
        
        public override void Validate()
        {
            base.Validate();
            
            // Add any placement-specific validation here
            if (FarmerId.HasValue && FarmerId < 0)
                FarmerId = null;
                
            if (BranchId.HasValue && BranchId < 0)
                BranchId = null;
                
            if (LineId.HasValue && LineId < 0)
                LineId = null;
                
            if (MinPlacementQty.HasValue && MinPlacementQty < 0)
                MinPlacementQty = 0;
                
            if (MaxPlacementQty.HasValue && MaxPlacementQty < 0)
                MaxPlacementQty = null;
        }
    }
}
