using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;

namespace ErpBE.Domain.Interfaces
{
    public interface IFarmerRepository
    {
        Task<List<FarmerDto>> GetAllAsync();
        Task<PagedResponse<FarmerDto>> GetPagedAsync(FarmerQueryParameters parameters);
        Task<int> CreateAsync(FarmerDto farmer);
        Task<FarmerDto?> GetByIdAsync(int id);
    }
}
