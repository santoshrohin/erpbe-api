using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.Placement.Queries
{
    public class GetAllPlacementsQueryHandler : IRequestHandler<GetAllPlacementsQuery, IEnumerable<PlacementWithDetailsDto>>
    {
        private readonly IPlacementRepository _placementRepository;

        public GetAllPlacementsQueryHandler(IPlacementRepository placementRepository)
        {
            _placementRepository = placementRepository;
        }

        public async Task<IEnumerable<PlacementWithDetailsDto>> Handle(GetAllPlacementsQuery request, CancellationToken cancellationToken)
        {
            return await _placementRepository.GetAllPlacementsWithDetailsAsync();
        }
    }
}
