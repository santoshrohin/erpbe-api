using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class GetUnitMasterByNameQueryHandler : IRequestHandler<GetUnitMasterByNameQuery, UnitMasterDto?>
    {
        private readonly IUnitMasterRepository _repository;

        public GetUnitMasterByNameQueryHandler(IUnitMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<UnitMasterDto?> Handle(GetUnitMasterByNameQuery request, CancellationToken cancellationToken)
        {
            var unit = await _repository.GetUnitMasterByNameAsync(request.UnitName, request.CompanyId);
            
            // Return null if not found instead of throwing exception
            // This allows the caller to handle the case gracefully
            return unit;
        }
    }
}

