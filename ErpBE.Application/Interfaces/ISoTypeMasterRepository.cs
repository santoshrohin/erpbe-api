using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces
{
    public interface ISoTypeMasterRepository
    {
        Task<int> CreateAsync(CreateSoTypeMasterRequest request);
        Task<bool> UpdateAsync(UpdateSoTypeMasterRequest request);
        Task<bool> DeleteAsync(int id);
        Task<SoTypeMasterDto?> GetByIdAsync(int id);
        Task<PagedResponse<SoTypeMasterDto>> GetPagedAsync(SoTypeMasterQueryParameters queryParameters);
        Task<SoTypeMasterDto?> GetByShortNameAsync(string shortName, int companyId);
        Task<bool> IsShortNameUniqueAsync(string shortName, int companyId, int? excludeId = null);
        Task<bool> IsUsedInCustomerPOAsync(int id);
        Task<bool> IsFixedRecordAsync(int id);
    }
}

