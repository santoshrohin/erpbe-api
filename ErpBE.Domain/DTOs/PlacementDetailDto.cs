namespace ErpBE.Domain.DTOs
{
    public class PlacementDetailDto
    {
        public int ItemId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public double FeedQtyPercentage { get; set; }
        public double FeedQty { get; set; }
        public int OffsetDays { get; set; }
        public int BagsRequired { get; set; }
    }

}
