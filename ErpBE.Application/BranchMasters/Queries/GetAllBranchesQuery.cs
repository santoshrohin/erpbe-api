using ErpBE.Application.BranchMasters.DTOs;
using MediatR;

namespace ErpBE.Application.BranchMasters.Queries
{
    public class GetAllBranchesQuery : IRequest<List<BranchDto>>
    {
    }
}
