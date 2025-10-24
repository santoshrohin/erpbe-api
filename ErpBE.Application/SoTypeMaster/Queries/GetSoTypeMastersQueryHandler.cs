using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class GetSoTypeMastersQueryHandler : IRequestHandler<GetSoTypeMastersQuery, PagedResponse<SoTypeMasterDto>>
    {
        private readonly ISoTypeMasterRepository _repository;

        public GetSoTypeMastersQueryHandler(ISoTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<SoTypeMasterDto>> Handle(GetSoTypeMastersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetPagedAsync(request.QueryParameters);
        }
    }
}

