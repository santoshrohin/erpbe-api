using Dapper;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ErpBE.Infrastructure.Repositories
{
    public class SoTypeMasterRepository : ISoTypeMasterRepository
    {
        private readonly IDbConnection _db;

        public SoTypeMasterRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateSoTypeMasterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_COMP_ID", request.CompanyId);
            parameters.Add("@SO_T_SHORT_NAME", request.ShortName);
            parameters.Add("@SO_T_DESC", request.Description);
            parameters.Add("@SO_T_FIRST_LETTER", request.FirstLetter);

            var id = await _db.QuerySingleAsync<int>(
                "ERP_CreateSoTypeMaster",
                parameters,
                commandType: CommandType.StoredProcedure);

            return id;
        }

        public async Task<bool> UpdateAsync(UpdateSoTypeMasterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_CODE", request.Id);
            parameters.Add("@SO_T_SHORT_NAME", request.ShortName);
            parameters.Add("@SO_T_DESC", request.Description);
            parameters.Add("@SO_T_FIRST_LETTER", request.FirstLetter);

            var result = await _db.ExecuteAsync(
                "ERP_UpdateSoTypeMaster",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_CODE", id);

            var result = await _db.ExecuteAsync(
                "ERP_DeleteSoTypeMaster",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<SoTypeMasterDto?> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_CODE", id);

            return await _db.QueryFirstOrDefaultAsync<SoTypeMasterDto>(
                "ERP_GetSoTypeMasterById",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResponse<SoTypeMasterDto>> GetPagedAsync(SoTypeMasterQueryParameters queryParameters)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", queryParameters.PageNumber);
            parameters.Add("@PageSize", queryParameters.PageSize);
            parameters.Add("@SortBy", queryParameters.SortBy ?? "SO_T_SHORT_NAME");
            parameters.Add("@SortDirection", queryParameters.SortDirection ?? "ASC");
            parameters.Add("@SearchTerm", queryParameters.SearchTerm);
            parameters.Add("@CompanyId", queryParameters.CompanyId);
            parameters.Add("@ShortName", queryParameters.ShortName);
            parameters.Add("@Description", queryParameters.Description);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var soTypes = await _db.QueryAsync<SoTypeMasterDto>(
                "ERP_GetSoTypeMasters",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalCount = parameters.Get<int>("@TotalCount");
            var totalPages = (int)Math.Ceiling((double)totalCount / queryParameters.PageSize);

            return new PagedResponse<SoTypeMasterDto>
            {
                Data = soTypes.ToList(),
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = queryParameters.PageNumber > 1,
                HasNextPage = queryParameters.PageNumber < totalPages
            };
        }

        public async Task<SoTypeMasterDto?> GetByShortNameAsync(string shortName, int companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_SHORT_NAME", shortName);
            parameters.Add("@SO_T_COMP_ID", companyId);

            return await _db.QueryFirstOrDefaultAsync<SoTypeMasterDto>(
                "ERP_GetSoTypeMasterByShortName",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> IsShortNameUniqueAsync(string shortName, int companyId, int? excludeId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_SHORT_NAME", shortName);
            parameters.Add("@SO_T_COMP_ID", companyId);
            parameters.Add("@ExcludeId", excludeId);

            var count = await _db.QuerySingleAsync<int>(
                "ERP_IsSoTypeShortNameUnique",
                parameters,
                commandType: CommandType.StoredProcedure);

            return count == 0;
        }

        public async Task<bool> IsUsedInCustomerPOAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_CODE", id);

            var count = await _db.QuerySingleAsync<int>(
                "ERP_CheckSoTypeUsage",
                parameters,
                commandType: CommandType.StoredProcedure);

            return count > 0;
        }

        public async Task<bool> IsFixedRecordAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SO_T_CODE", id);

            var isFixed = await _db.QuerySingleAsync<bool>(
                "ERP_IsSoTypeFixedRecord",
                parameters,
                commandType: CommandType.StoredProcedure);

            return isFixed;
        }
    }
}

