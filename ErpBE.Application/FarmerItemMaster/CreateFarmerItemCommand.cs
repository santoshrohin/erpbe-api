using MediatR;

namespace ErpBE.Application.FarmerItemMaster
{
   
    public class CreateFarmerItemCommand : IRequest<int>
    {
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public double FeedPercent { get; set; }
        public int OffsetDays { get; set; }
        public int StandardBagSize { get; set; }
    }

}
