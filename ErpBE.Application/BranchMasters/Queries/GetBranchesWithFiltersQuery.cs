using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.BranchMasters.Queries
{
    public class GetBranchesWithFiltersQuery : IRequest<PagedResponse<BranchDto>>
    {
        public BranchQueryParameters Parameters { get; }

        public GetBranchesWithFiltersQuery(BranchQueryParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
