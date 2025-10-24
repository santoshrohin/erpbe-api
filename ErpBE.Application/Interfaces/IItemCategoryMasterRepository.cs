using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces
{
    public interface IItemCategoryMasterRepository
    {
        Task<int> CreateAsync(CreateItemCategoryMasterRequest request);
        Task<bool> UpdateAsync(UpdateItemCategoryMasterRequest request);
        Task<bool> DeleteAsync(int categoryId);
        Task<ItemCategoryMasterDto?> GetByIdAsync(int categoryId);
        Task<PagedResponse<ItemCategoryMasterDto>> GetPagedAsync(ItemCategoryMasterQueryParameters queryParameters);
        Task<ItemCategoryMasterDto?> GetByNameAsync(string categoryName, int companyId);
        Task<bool> IsCategoryNameUniqueAsync(string categoryName, int companyId, int? excludeCategoryId = null);
        Task<bool> SetActiveStatusAsync(int categoryId, bool isActive);
        Task<bool> IsCategoryUsedInItemMasterAsync(int categoryId);
    }
}



