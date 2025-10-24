using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces
{
    public interface ICustomerMasterRepository
    {
        Task<CustomerMasterDto> CreateAsync(CreateCustomerMasterRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(UpdateCustomerMasterRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default);
        Task<CustomerMasterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResponse<CustomerMasterDto>> GetAllAsync(CustomerMasterQueryParameters parameters, CancellationToken cancellationToken = default);
        Task<bool> IsPartyNameUniqueAsync(string partyName, int? id, int companyId, CancellationToken cancellationToken = default);
        Task<bool> IsAbbreviationUniqueAsync(string abbreviation, int? id, int companyId, CancellationToken cancellationToken = default);
        Task<bool> IsModifiedByAnotherUserAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> IsUsedInTransactionsAsync(int id, CancellationToken cancellationToken = default);
    }
}

