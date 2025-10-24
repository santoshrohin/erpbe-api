using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.CustomerMaster.Interfaces
{
    public interface ICustomerMasterRepository
    {
        Task<CustomerMasterDto> CreateAsync(CreateCustomerMasterRequest request);
        Task UpdateAsync(UpdateCustomerMasterRequest request);
        Task DeleteAsync(int id, int companyId);
        Task<CustomerMasterDto?> GetByIdAsync(int id, int companyId);
        Task<PagedResponse<CustomerMasterDto>> GetAllAsync(CustomerMasterQueryParameters parameters);
        Task<bool> IsPartyNameUniqueAsync(string partyName, int? id, int companyId);
        Task<bool> IsAbbreviationUniqueAsync(string abbreviation, int? id, int companyId);
    }
}

