using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces
{
    public interface IDeliveryChallanRepository
    {
        Task<(IEnumerable<DeliveryChallanMasterDto> Data, int TotalCount)> GetAllAsync(DeliveryChallanQueryParameters parameters);
        Task<DeliveryChallanMasterDto?> GetByIdAsync(int challanCode, int companyCode);
        Task<DeliveryChallanMasterDto> CreateAsync(CreateDeliveryChallanRequest request);
        Task<DeliveryChallanMasterDto> UpdateAsync(UpdateDeliveryChallanRequest request);
        Task<bool> DeleteAsync(int challanCode, int companyCode);
        Task<bool> LockAsync(int challanCode, int companyCode, int lockedByUserId);
        Task<bool> UnlockAsync(int challanCode, int companyCode);
        Task<DeliveryChallanPrintDto?> GetPrintDataAsync(int challanCode, int companyCode);
    }
}
