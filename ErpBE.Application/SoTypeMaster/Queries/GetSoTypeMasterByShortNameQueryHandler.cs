using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class GetSoTypeMasterByShortNameQueryHandler : IRequestHandler<GetSoTypeMasterByShortNameQuery, SoTypeMasterDto>
    {
        private readonly ISoTypeMasterRepository _repository;

        public GetSoTypeMasterByShortNameQueryHandler(ISoTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<SoTypeMasterDto> Handle(GetSoTypeMasterByShortNameQuery request, CancellationToken cancellationToken)
        {
            var soType = await _repository.GetByShortNameAsync(request.ShortName, request.CompanyId);
            
            if (soType == null)
            {
                throw new KeyNotFoundException($"SO Type Master with short name '{request.ShortName}' not found for company {request.CompanyId}.");
            }
            
            return soType;
        }
    }
}

