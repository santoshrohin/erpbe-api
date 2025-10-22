using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.FarmerMaster.Queries
{
    public class GetFarmersWithFiltersQueryHandler : IRequestHandler<GetFarmersWithFiltersQuery, PagedResponse<FarmerDto>>
    {
        private readonly IFarmerRepository _repository;

        public GetFarmersWithFiltersQueryHandler(IFarmerRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<FarmerDto>> Handle(GetFarmersWithFiltersQuery request, CancellationToken cancellationToken)
        {
            request.Parameters.Validate();
            return await _repository.GetPagedAsync(request.Parameters);
        }
    }
}
