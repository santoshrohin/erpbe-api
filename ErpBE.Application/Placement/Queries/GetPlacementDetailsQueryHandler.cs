using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.Placement.Queries
{

    public class GetPlacementDetailsQueryHandler : IRequestHandler<GetPlacementDetailsQuery, IEnumerable<FarmerItemCalculatedDTO>>
    {
        private readonly IPlacementRepository _placementRepository;

        public GetPlacementDetailsQueryHandler(IPlacementRepository placementRepository)
        {
            _placementRepository = placementRepository;
        }

        public async Task<IEnumerable<FarmerItemCalculatedDTO>> Handle(GetPlacementDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _placementRepository.GetPlacementDetailsAsync(request.PlacementQty, request.PlacementDate);
        }
    }


}
