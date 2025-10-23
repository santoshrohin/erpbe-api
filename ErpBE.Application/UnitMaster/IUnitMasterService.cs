using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;

namespace ErpBE.Application.UnitMaster
{
    public interface IUnitMasterService
    {
        Task<int> CreateUnitMasterAsync(CreateUnitMasterRequest request);
        Task<bool> UpdateUnitMasterAsync(UpdateUnitMasterRequest request);
        Task<bool> DeleteUnitMasterAsync(int id);
        Task<UnitMasterDto?> GetUnitMasterByIdAsync(int id);
        Task<UnitMasterDto?> GetUnitMasterByNameAsync(string unitName, int companyId);
        Task<PagedResponse<UnitMasterDto>> GetUnitMastersAsync(UnitMasterQueryParameters queryParameters);
        Task<bool> SetUnitMasterActiveStatusAsync(int id, bool isActive);
    }
}
