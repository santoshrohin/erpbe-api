using ErpBE.Domain.DTOs;
using ErpBE.Domain.FarmerEntities;
using MediatR;

namespace ErpBE.Application.Placement.Queries
{
    public class GetPlacementDetailsQuery : IRequest<IEnumerable<FarmerItemCalculatedDTO>>
    {
        public int? PlacementQty { get; set; }
        public DateTime? PlacementDate { get; set; }

        public GetPlacementDetailsQuery(int? placementQty, DateTime? placementDate)
        {
            PlacementQty = placementQty;
            PlacementDate = placementDate;
        }
    }

}
