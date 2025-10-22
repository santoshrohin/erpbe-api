namespace ErpBE.Domain.FarmerEntities
{
    public class Placement
    {
        public int PlacementId { get; set; }
        public DateTime EntryDate { get; set; }  // User selects
        public int FarmerId { get; set; }        // FarmerMaster (foreign key)
        public string FarmerName { get; set; }   // User selects
        public string FarmerCode { get; set; }   // Auto fetched
        public string FarmerAddress { get; set; }// Auto fetched
        public int BranchId { get; set; }        // BranchMaster (foreign key)
        public int LineId { get; set; }          // LineMaster (foreign key)
        public double PlacementQty { get; set; } // User enters
        public DateTime PlacementDate { get; set; } // User selects
        public bool Status { get; set; }         // Default is true (active)
    }

}
