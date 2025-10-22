using ErpBE.Domain.CommonDto;

namespace ErpBE.Domain.Common
{
    public interface IDropdownRepository
    {
        Task<List<DropdownItem>> GetDropdownDataAsync(DropdownRequest request);
    }
}
