using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces
{
    public interface IIssueMasterRepository
    {
        Task<(IEnumerable<IssueMasterDto> Data, int TotalCount)> GetAllAsync(IssueMasterQueryParameters parameters);
        Task<IssueMasterDto?> GetByIdAsync(int issueCode, int companyCode);
        Task<IssueMasterDto> CreateAsync(CreateIssueMasterRequest request);
        Task<IssueMasterDto> UpdateAsync(UpdateIssueMasterRequest request);
        Task<bool> DeleteAsync(int issueCode, int companyCode);
    }
}
