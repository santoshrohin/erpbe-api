using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;

namespace ErpBE.Domain.Interfaces
{
    public interface IBranchRepository
    {
        Task<List<BranchDto>> GetAllAsync();
        Task<PagedResponse<BranchDto>> GetPagedAsync(BranchQueryParameters parameters);
        Task<int> CreateAsync(string branchName);
    }
}
