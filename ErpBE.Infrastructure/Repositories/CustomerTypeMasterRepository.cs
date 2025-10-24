using Dapper;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class CustomerTypeMasterRepository : ICustomerTypeMasterRepository
    {
        private readonly string _connectionString;

        public CustomerTypeMasterRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<CustomerTypeMasterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_CODE", id);

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "ERP_GetCustomerTypeMasterById",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
                return null;

            return new CustomerTypeMasterDto
            {
                Id = result.CTM_CODE,
                CompanyId = result.CTM_CM_COMP_ID,
                TypeCode = result.CTM_TYPE_CODE,
                TypeDescription = result.CTM_TYPE_DESC,
                FirstLetter = result.CTM_FIRST_LETTER,
                IsDeleted = result.ES_DELETE,
                IsModified = result.MODIFY
            };
        }

        public async Task<CustomerTypeMasterDto?> GetByTypeCodeAsync(string typeCode, int companyId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_TYPE_CODE", typeCode);
            parameters.Add("@CTM_CM_COMP_ID", companyId);

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "ERP_GetCustomerTypeMasterByTypeCode",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
                return null;

            return new CustomerTypeMasterDto
            {
                Id = result.CTM_CODE,
                CompanyId = result.CTM_CM_COMP_ID,
                TypeCode = result.CTM_TYPE_CODE,
                TypeDescription = result.CTM_TYPE_DESC,
                FirstLetter = result.CTM_FIRST_LETTER,
                IsDeleted = result.ES_DELETE,
                IsModified = result.MODIFY
            };
        }

        public async Task<PagedResponse<CustomerTypeMasterDto>> GetAllAsync(
            CustomerTypeMasterQueryParameters parameters,
            CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var dbParams = new DynamicParameters();
            dbParams.Add("@CTM_CM_COMP_ID", parameters.CompanyId);
            dbParams.Add("@CTM_TYPE_CODE", parameters.TypeCode);
            dbParams.Add("@CTM_TYPE_DESC", parameters.TypeDescription);
            dbParams.Add("@CTM_FIRST_LETTER", parameters.FirstLetter);
            dbParams.Add("@SearchTerm", parameters.SearchTerm);
            dbParams.Add("@SortBy", parameters.SortBy);
            dbParams.Add("@SortDirection", parameters.SortDirection);
            dbParams.Add("@PageNumber", parameters.PageNumber);
            dbParams.Add("@PageSize", parameters.PageSize);
            dbParams.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<dynamic>(
                "ERP_GetCustomerTypeMasters",
                dbParams,
                commandType: CommandType.StoredProcedure
            );

            var totalCount = dbParams.Get<int>("@TotalCount");

            var dtos = results.Select(r => new CustomerTypeMasterDto
            {
                Id = r.CTM_CODE,
                CompanyId = r.CTM_CM_COMP_ID,
                TypeCode = r.CTM_TYPE_CODE,
                TypeDescription = r.CTM_TYPE_DESC,
                FirstLetter = r.CTM_FIRST_LETTER,
                IsDeleted = r.ES_DELETE,
                IsModified = r.MODIFY
            }).ToList();

            return new PagedResponse<CustomerTypeMasterDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<CustomerTypeMasterDto> CreateAsync(CreateCustomerTypeMasterRequest request, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_CM_COMP_ID", request.CompanyId);
            parameters.Add("@CTM_TYPE_CODE", request.TypeCode);
            parameters.Add("@CTM_TYPE_DESC", request.TypeDescription);
            parameters.Add("@CTM_FIRST_LETTER", request.FirstLetter);
            parameters.Add("@CTM_CODE", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "ERP_CreateCustomerTypeMaster",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var newId = parameters.Get<int>("@CTM_CODE");

            return new CustomerTypeMasterDto
            {
                Id = newId,
                CompanyId = request.CompanyId,
                TypeCode = request.TypeCode,
                TypeDescription = request.TypeDescription,
                FirstLetter = request.FirstLetter,
                IsDeleted = false,
                IsModified = false
            };
        }

        public async Task<bool> UpdateAsync(UpdateCustomerTypeMasterRequest request, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_CODE", request.Id);
            parameters.Add("@CTM_CM_COMP_ID", request.CompanyId);
            parameters.Add("@CTM_TYPE_CODE", request.TypeCode);
            parameters.Add("@CTM_TYPE_DESC", request.TypeDescription);
            parameters.Add("@CTM_FIRST_LETTER", request.FirstLetter);

            var rowsAffected = await connection.ExecuteAsync(
                "ERP_UpdateCustomerTypeMaster",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_CODE", id);
            parameters.Add("@CTM_CM_COMP_ID", companyId);

            var rowsAffected = await connection.ExecuteAsync(
                "ERP_DeleteCustomerTypeMaster",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        public async Task<bool> IsTypeCodeUniqueAsync(string typeCode, int? excludeId, int companyId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_TYPE_CODE", typeCode);
            parameters.Add("@ExcludeId", excludeId);
            parameters.Add("@CTM_CM_COMP_ID", companyId);
            parameters.Add("@IsUnique", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "ERP_IsTypeCodeUnique",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<bool>("@IsUnique");
        }

        public async Task<bool> IsUsedInPartyMasterAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_CODE", id);
            parameters.Add("@IsUsed", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "ERP_CheckCustomerTypeUsage",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<bool>("@IsUsed");
        }

        public async Task<bool> IsModifiedByAnotherUserAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CTM_CODE", id);
            parameters.Add("@IsModified", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "ERP_IsCustomerTypeModified",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<bool>("@IsModified");
        }
    }
}

