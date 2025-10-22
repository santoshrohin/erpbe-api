using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.Placement
{


    public class CreatePlacementCommandHandler : IRequestHandler<CreatePlacementCommand, int>
    {
        private readonly IPlacementRepository _placementRepo;

        public CreatePlacementCommandHandler(IPlacementRepository placementRepo)
        {
            _placementRepo = placementRepo;
        }

        public async Task<int> Handle(CreatePlacementCommand request, CancellationToken cancellationToken)
        {
            var placement = new PlacementDto
            {
                FarmerId = request.FarmerId,
                FarmerName = request.FarmerName,
                FarmerCode = request.FarmerCode,
                FarmerAddress = request.FarmerAddress,
                BranchId = request.BranchId,
                LineId = request.LineId,
                PlacementQty = request.PlacementQty,
                PlacementDate = request.PlacementDate
            };

            // Reuse FarmerItemCalculatedDTO for details
            var placementId = await _placementRepo.CreatePlacementAsync(placement, request.Details);

            return placementId;
        }
    }

}
