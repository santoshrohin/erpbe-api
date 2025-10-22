using ErpBE.Application.BranchMasters.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.BranchMasters.Queries
{
    public class GetAllBranchesQueryHandler : IRequestHandler<GetAllBranchesQuery, List<BranchDto>>
    {
        private readonly IBranchRepository _repository;

        public GetAllBranchesQueryHandler(IBranchRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BranchDto>> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
        {
            var branches = await _repository.GetAllAsync();

            return branches.Select(b => new BranchDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName
            }).ToList();
        }
    }

}
