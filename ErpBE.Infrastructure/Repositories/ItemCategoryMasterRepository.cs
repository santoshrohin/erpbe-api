using Dapper;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace ErpBE.Infrastructure.Repositories
{
    public class ItemCategoryMasterRepository : IItemCategoryMasterRepository
    {
        private readonly IDbConnection _db;

        public ItemCategoryMasterRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateItemCategoryMasterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_NAME", request.CategoryName);
            parameters.Add("@I_CAT_CM_COMP_ID", request.CompanyId);
            parameters.Add("@I_CAT_SHORTCLOSE", request.IsAutoShortClose);
            parameters.Add("@ES_DELETE", !request.IsActive);

            var categoryId = await _db.QuerySingleAsync<int>(
                "SP_CreateItemCategoryMaster",
                parameters,
                commandType: CommandType.StoredProcedure);

            return categoryId;
        }

        public async Task<bool> UpdateAsync(UpdateItemCategoryMasterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_CODE", request.CategoryId);
            parameters.Add("@I_CAT_NAME", request.CategoryName);
            parameters.Add("@I_CAT_SHORTCLOSE", request.IsAutoShortClose);
            parameters.Add("@ES_DELETE", !request.IsActive);

            var result = await _db.ExecuteAsync(
                "SP_UpdateItemCategoryMaster",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_CODE", categoryId);

            var result = await _db.ExecuteAsync(
                "SP_DeleteItemCategoryMaster",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<ItemCategoryMasterDto?> GetByIdAsync(int categoryId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_CODE", categoryId);

            return await _db.QueryFirstOrDefaultAsync<ItemCategoryMasterDto>(
                "SP_GetItemCategoryMasterById",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResponse<ItemCategoryMasterDto>> GetPagedAsync(ItemCategoryMasterQueryParameters queryParameters)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", queryParameters.PageNumber);
            parameters.Add("@PageSize", queryParameters.PageSize);
            parameters.Add("@SortBy", queryParameters.SortBy ?? "I_CAT_NAME");
            parameters.Add("@SortDirection", queryParameters.SortDirection ?? "ASC");
            parameters.Add("@SearchTerm", queryParameters.SearchTerm);
            parameters.Add("@CompanyId", queryParameters.CompanyId);
            parameters.Add("@IsActive", queryParameters.IsActive);
            parameters.Add("@IsAutoShortClose", queryParameters.IsAutoShortClose);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var categories = await _db.QueryAsync<ItemCategoryMasterDto>(
                "SP_GetItemCategoryMasters",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalCount = parameters.Get<int>("@TotalCount");
            var totalPages = (int)Math.Ceiling((double)totalCount / queryParameters.PageSize);

            return new PagedResponse<ItemCategoryMasterDto>
            {
                Data = categories.ToList(),
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = queryParameters.PageNumber > 1,
                HasNextPage = queryParameters.PageNumber < totalPages
            };
        }

        public async Task<ItemCategoryMasterDto?> GetByNameAsync(string categoryName, int companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_NAME", categoryName);
            parameters.Add("@I_CAT_CM_COMP_ID", companyId);

            return await _db.QueryFirstOrDefaultAsync<ItemCategoryMasterDto>(
                "SP_GetItemCategoryMasterByName",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> IsCategoryNameUniqueAsync(string categoryName, int companyId, int? excludeCategoryId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_NAME", categoryName);
            parameters.Add("@I_CAT_CM_COMP_ID", companyId);
            parameters.Add("@ExcludeCategoryId", excludeCategoryId);

            var count = await _db.QuerySingleAsync<int>(
                "SP_IsItemCategoryNameUnique",
                parameters,
                commandType: CommandType.StoredProcedure);

            return count == 0;
        }

        public async Task<bool> SetActiveStatusAsync(int categoryId, bool isActive)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_CODE", categoryId);
            parameters.Add("@ES_DELETE", !isActive);

            var result = await _db.ExecuteAsync(
                "SP_SetItemCategoryActiveStatus",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<bool> IsCategoryUsedInItemMasterAsync(int categoryId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@I_CAT_CODE", categoryId);

            var count = await _db.QuerySingleAsync<int>(
                "SP_CheckItemCategoryUsage",
                parameters,
                commandType: CommandType.StoredProcedure);

            return count > 0;
        }
    }
}

