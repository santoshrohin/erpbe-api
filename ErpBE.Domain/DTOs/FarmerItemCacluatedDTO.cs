namespace ErpBE.Domain.DTOs
{
    public class FarmerItemCalculatedDTO
    {
        public decimal FeedPercent { get; set; }
        public int OffsetDays { get; set; }
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public decimal FeedQty { get; set; }
        public decimal BagsRequired { get; set; }
    }
}
