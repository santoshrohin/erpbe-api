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
            
            if (unit == null)
            {
                throw new KeyNotFoundException($"Unit master with name '{request.UnitName}' not found for company {request.CompanyId}.");
            }
            
            return unit;
        }
    }
}

