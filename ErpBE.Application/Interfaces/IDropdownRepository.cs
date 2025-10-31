using ErpBE.Application.Common.Models;

namespace ErpBE.Application.Interfaces
{
    public interface IDropdownRepository
    {
        Task<List<DropdownItem>> GetDropdownDataAsync(DropdownRequest request);
        Task<(List<DropdownItem> Items, int? TotalCount)> GetDropdownDataPagedAsync(DropdownRequest request);
        Task<BatchDropdownResponse> GetBatchDropdownsAsync(BatchDropdownRequest batchRequest);
    }
}

