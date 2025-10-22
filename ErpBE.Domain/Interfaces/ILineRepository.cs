using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;

namespace ErpBE.Domain.Interfaces
{
    public interface ILineRepository
    {
        Task<List<LineDto>> GetAllAsync();
        Task<PagedResponse<LineDto>> GetPagedAsync(LineQueryParameters parameters);
        Task<int> CreateAsync(string lineName);
    }
}
