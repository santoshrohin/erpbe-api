using ErpBE.Domain.DTOs;
using ErpBE.Domain.CommonDto;

namespace ErpBE.Domain.Interfaces
{
    public interface IUnitMasterRepository
    {
        Task<int> CreateUnitMasterAsync(CreateUnitMasterRequest request);
        Task<bool> UpdateUnitMasterAsync(UpdateUnitMasterRequest request);
        Task<bool> DeleteUnitMasterAsync(int id);
        Task<UnitMasterDto?> GetUnitMasterByIdAsync(int id);
        Task<UnitMasterDto?> GetUnitMasterByNameAsync(string unitName, int companyId);
        Task<PagedResponse<UnitMasterDto>> GetUnitMastersAsync(UnitMasterQueryParameters queryParameters);
        Task<bool> IsUnitNameUniqueAsync(string unitName, int companyId, int? excludeId = null);
        Task<bool> SetUnitMasterActiveStatusAsync(int id, bool isActive);
    }
}
