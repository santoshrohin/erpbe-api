namespace ErpBE.Domain.DTOs
{
    public class FarmerItemDto
    {
        public int Id { get; set; }
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public double FeedPercent { get; set; }
        public int OffsetDays { get; set; }
        public int StandardBagSize { get; set; }
    }
}
