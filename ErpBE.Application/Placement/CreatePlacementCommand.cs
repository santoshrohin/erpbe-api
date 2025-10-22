using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.Placement
{
    public class CreatePlacementCommand : IRequest<int>
    {
        public int FarmerId { get; set; }
        public string FarmerName { get; set; }
        public string FarmerCode { get; set; }
        public string FarmerAddress { get; set; }
        public int BranchId { get; set; }
        public int LineId { get; set; }
        public double PlacementQty { get; set; }
        public DateTime PlacementDate { get; set; }
        public List<FarmerItemCalculatedDTO> Details { get; set; } = new();
    }
}
