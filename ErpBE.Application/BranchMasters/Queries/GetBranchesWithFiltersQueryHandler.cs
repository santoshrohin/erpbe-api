using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.BranchMasters.Queries
{
    public class GetBranchesWithFiltersQueryHandler : IRequestHandler<GetBranchesWithFiltersQuery, PagedResponse<BranchDto>>
    {
        private readonly IBranchRepository _repository;

        public GetBranchesWithFiltersQueryHandler(IBranchRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<BranchDto>> Handle(GetBranchesWithFiltersQuery request, CancellationToken cancellationToken)
        {
            request.Parameters.Validate();
            return await _repository.GetPagedAsync(request.Parameters);
        }
    }
}
