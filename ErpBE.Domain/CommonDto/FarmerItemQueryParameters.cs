namespace ErpBE.Domain.CommonDto
{
    public class FarmerItemQueryParameters : QueryParameters
    {
        public string? ItemType { get; set; }
        public string? ItemName { get; set; }
        public decimal? MinFeedPercent { get; set; }
        public decimal? MaxFeedPercent { get; set; }
        public int? MinOffsetDays { get; set; }
        public int? MaxOffsetDays { get; set; }
        
        public override void Validate()
        {
            base.Validate();
            
            // Add any farmer item specific validation here
            if (MinFeedPercent.HasValue && MinFeedPercent < 0)
                MinFeedPercent = 0;
                
            if (MaxFeedPercent.HasValue && MaxFeedPercent > 100)
                MaxFeedPercent = 100;
                
            if (MinOffsetDays.HasValue && MinOffsetDays < 0)
                MinOffsetDays = 0;
        }
    }
}
