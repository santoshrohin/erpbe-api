namespace ErpBE.Domain.FarmerEntities
{
    public class PlacementDetail
    {
        public int PlacementDetailId { get; set; }
        public int PlacementId { get; set; }  // Placement (foreign key)
        public int ItemId { get; set; }      // FarmerItemMaster (foreign key)
        public DateTime ScheduledDate { get; set; }   // Date calculated for dispatch
        public double FeedQtyPercentage { get; set; } // Feed percentage for the item
        public double FeedQty { get; set; }  // Calculated Feed quantity (in kg)
        public int OffsetDays { get; set; }     // Scheduled dispatch offset
        public int BagsRequired { get; set; } // Calculated Bags required (50kg bags)
    }

}
