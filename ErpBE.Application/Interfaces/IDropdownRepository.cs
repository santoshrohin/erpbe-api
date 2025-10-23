using ErpBE.Application.Common.Models;

namespace ErpBE.Application.Interfaces
{
    public interface IDropdownRepository
    {
        Task<List<DropdownItem>> GetDropdownDataAsync(DropdownRequest request);
    }
}

