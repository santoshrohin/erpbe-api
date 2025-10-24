using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces
{
    public interface ICustomerTypeMasterRepository
    {
        Task<CustomerTypeMasterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CustomerTypeMasterDto?> GetByTypeCodeAsync(string typeCode, int companyId, CancellationToken cancellationToken = default);
        Task<PagedResponse<CustomerTypeMasterDto>> GetAllAsync(CustomerTypeMasterQueryParameters parameters, CancellationToken cancellationToken = default);
        Task<CustomerTypeMasterDto> CreateAsync(CreateCustomerTypeMasterRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(UpdateCustomerTypeMasterRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default);
        Task<bool> IsTypeCodeUniqueAsync(string typeCode, int? excludeId, int companyId, CancellationToken cancellationToken = default);
        Task<bool> IsUsedInPartyMasterAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> IsModifiedByAnotherUserAsync(int id, CancellationToken cancellationToken = default);
    }
}

