using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.FarmerMaster.Queries
{
    public class GetAllFarmersQueryHandler : IRequestHandler<GetAllFarmersQuery, List<FarmerDto>>
    {
        private readonly IFarmerRepository _repository;

        public GetAllFarmersQueryHandler(IFarmerRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FarmerDto>> Handle(GetAllFarmersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync();
        }
    }

}
