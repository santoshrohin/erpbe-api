using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpBE.Domain.FarmerEntities
{
    public class FarmerItemMaster
    {
        public int Id { get; set; }
        public string ItemType { get; set; }    // Item name (A, B, C, etc.)
        public string ItemName { get; set; }    // Item name (A, B, C, etc.)
        public decimal FeedPercent { get; set; } // Feed % for this item
        public int OffsetDays { get; set; }     // Scheduled dispatch offset
        public int StandardBagSize { get; set; }

    }

}
