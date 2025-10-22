using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;

namespace ErpBE.Domain.Interfaces
{
    public interface IPlacementRepository
    {
        Task<int> CreatePlacementAsync(PlacementDto placement, List<FarmerItemCalculatedDTO> details);
        Task<PagedResponse<PlacementWithDetailsDto>> GetPagedAsync(PlacementQueryParameters parameters);
        Task<IEnumerable<FarmerItemCalculatedDTO>> GetPlacementDetailsAsync(int? placementQty, DateTime? placementDate);
        Task<IEnumerable<PlacementWithDetailsDto>> GetAllPlacementsWithDetailsAsync();
    }
}
