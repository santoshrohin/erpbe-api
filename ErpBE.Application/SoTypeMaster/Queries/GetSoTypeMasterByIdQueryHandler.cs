using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class GetSoTypeMasterByIdQueryHandler : IRequestHandler<GetSoTypeMasterByIdQuery, SoTypeMasterDto>
    {
        private readonly ISoTypeMasterRepository _repository;

        public GetSoTypeMasterByIdQueryHandler(ISoTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<SoTypeMasterDto> Handle(GetSoTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var soType = await _repository.GetByIdAsync(request.Id);
            
            if (soType == null)
            {
                throw new KeyNotFoundException($"SO Type Master with ID '{request.Id}' not found.");
            }
            
            return soType;
        }
    }
}

