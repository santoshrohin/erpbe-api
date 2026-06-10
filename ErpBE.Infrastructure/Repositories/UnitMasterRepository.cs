using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.Common.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ErpBE.Infrastructure.Repositories
{
    public class UnitMasterRepository : IUnitMasterRepository
    {
        private readonly IDbConnection _db;

        public UnitMasterRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreateUnitMasterAsync(CreateUnitMasterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UnitName", request.UnitName);
            parameters.Add("@UnitDescription", request.UnitDescription);
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@IsActive", request.IsActive);
            parameters.Add("@UnitId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _db.ExecuteAsync("SP_CreateUnitMaster", parameters, commandType: CommandType.StoredProcedure);
            return parameters.Get<int>("@UnitId");
        }

        public async Task<bool> UpdateUnitMasterAsync(UpdateUnitMasterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UnitId", request.Id);
            parameters.Add("@UnitName", request.UnitName);
            parameters.Add("@UnitDescription", request.UnitDescription);
            parameters.Add("@IsActive", request.IsActive);

            var result = await _db.ExecuteAsync("SP_UpdateUnitMaster", parameters, commandType: CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<bool> DeleteUnitMasterAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UnitId", id);

            var result = await _db.ExecuteAsync("SP_DeleteUnitMaster", parameters, commandType: CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<UnitMasterDto?> GetUnitMasterByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UnitId", id);

            return await _db.QueryFirstOrDefaultAsync<UnitMasterDto>(
                "SP_GetUnitMasterById", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<UnitMasterDto?> GetUnitMasterByNameAsync(string unitName, int companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UnitName", unitName);
            parameters.Add("@CompanyId", companyId);

            return await _db.QueryFirstOrDefaultAsync<UnitMasterDto>(
                "SP_GetUnitMasterByName", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResponse<UnitMasterDto>> GetUnitMastersAsync(UnitMasterQueryParameters queryParameters)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", queryParameters.PageNumber);
            parameters.Add("@PageSize", queryParameters.PageSize);
            parameters.Add("@SortBy", queryParameters.SortBy ?? "I_UOM_NAME"); // Default to I_UOM_NAME if null
            parameters.Add("@SortDirection", queryParameters.SortDirection ?? "ASC"); // Default to ASC if null
            parameters.Add("@SearchTerm", queryParameters.SearchTerm);
            parameters.Add("@CompanyId", queryParameters.CompanyId);
            parameters.Add("@IsActive", queryParameters.IsActive);
            parameters.Add("@UnitName", queryParameters.UnitName);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var units = await _db.QueryAsync<UnitMasterDto>("SP_GetUnitMasters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");

            return new PagedResponse<UnitMasterDto>(units.ToList(), totalCount, queryParameters.PageNumber, queryParameters.PageSize);
        }

        public async Task<bool> IsUnitNameUniqueAsync(string unitName, int companyId, int? excludeId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UnitName", unitName);
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@ExcludeId", excludeId);

            var count = await _db.QuerySingleAsync<int>("SP_IsUnitNameUnique", parameters, commandType: CommandType.StoredProcedure);
            return count == 0;
        }

        public async Task<bool> SetUnitMasterActiveStatusAsync(int id, bool isActive)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UnitId", id);
            parameters.Add("@IsActive", isActive);

            var result = await _db.ExecuteAsync("SP_SetUnitMasterActiveStatus", parameters, commandType: CommandType.StoredProcedure);
            return result > 0;
        }
    }
}
