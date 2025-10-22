using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;

namespace ErpBE.Domain.Interfaces
{
    public interface IFarmerItemRepository
    {
        Task<List<FarmerItemDto>> GetAllAsync();
        Task<PagedResponse<FarmerItemDto>> GetPagedAsync(FarmerItemQueryParameters parameters);
        Task<int> CreateAsync(FarmerItemDto item);
    }
}
