using Dapper;
using ErpBE.Application.Common.Models;
using ErpBE.Application.CustomerMaster.Interfaces;
using ErpBE.Application.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class CustomerMasterRepository : ICustomerMasterRepository
    {
        private readonly string _connectionString;

        public CustomerMasterRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<CustomerMasterDto> CreateAsync(CreateCustomerMasterRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@PartyName", request.PartyName);
            parameters.Add("@ContactPerson", request.ContactPerson);
            parameters.Add("@Abbreviation", request.Abbreviation);
            parameters.Add("@VendorCode", request.VendorCode);
            parameters.Add("@Address", request.Address);
            parameters.Add("@Phone", request.Phone);
            parameters.Add("@Mobile", request.Mobile);
            parameters.Add("@Email", request.Email);
            parameters.Add("@FaxNo", request.FaxNo);
            parameters.Add("@PinCode", request.PinCode);
            parameters.Add("@AreaCode", request.AreaCode);
            parameters.Add("@CustomerType", request.CustomerType);
            parameters.Add("@CountryCode", request.CountryCode);
            parameters.Add("@StateCode", request.StateCode);
            parameters.Add("@CityCode", request.CityCode);
            parameters.Add("@CategoryCode", request.CategoryCode);
            parameters.Add("@EmployeeCode", request.EmployeeCode);
            parameters.Add("@PanNo", request.PanNo);
            parameters.Add("@CstNo", request.CstNo);
            parameters.Add("@VatNo", request.VatNo);
            parameters.Add("@ServiceTaxNo", request.ServiceTaxNo);
            parameters.Add("@EccNo", request.EccNo);
            parameters.Add("@LbtNo", request.LbtNo);
            parameters.Add("@ExciseRange", request.ExciseRange);
            parameters.Add("@ExciseDivision", request.ExciseDivision);
            parameters.Add("@ExciseCollectorate", request.ExciseCollectorate);
            parameters.Add("@TallyName", request.TallyName);
            parameters.Add("@CreditDays", request.CreditDays);
            parameters.Add("@TdsPercentage", request.TdsPercentage);
            parameters.Add("@IsActive", request.IsActive);
            parameters.Add("@IsLbtApplicable", request.IsLbtApplicable);
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@NewPartyCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "ERP_CreateCustomerMaster",
                parameters,
                commandType: CommandType.StoredProcedure);

            var newId = parameters.Get<int>("@NewId");
            var newPartyCode = parameters.Get<int>("@NewPartyCode");

            // Fetch the newly created record
            var result = await GetByIdAsync(newId, request.CompanyId);
            return result ?? throw new Exception("Failed to retrieve created customer.");
        }

        public async Task UpdateAsync(UpdateCustomerMasterRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            
            parameters.Add("@Id", request.Id);
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@PartyCode", request.PartyCode);
            parameters.Add("@PartyName", request.PartyName);
            parameters.Add("@ContactPerson", request.ContactPerson);
            parameters.Add("@Abbreviation", request.Abbreviation);
            parameters.Add("@VendorCode", request.VendorCode);
            parameters.Add("@Address", request.Address);
            parameters.Add("@Phone", request.Phone);
            parameters.Add("@Mobile", request.Mobile);
            parameters.Add("@Email", request.Email);
            parameters.Add("@FaxNo", request.FaxNo);
            parameters.Add("@PinCode", request.PinCode);
            parameters.Add("@AreaCode", request.AreaCode);
            parameters.Add("@CustomerType", request.CustomerType);
            parameters.Add("@CountryCode", request.CountryCode);
            parameters.Add("@StateCode", request.StateCode);
            parameters.Add("@CityCode", request.CityCode);
            parameters.Add("@CategoryCode", request.CategoryCode);
            parameters.Add("@EmployeeCode", request.EmployeeCode);
            parameters.Add("@PanNo", request.PanNo);
            parameters.Add("@CstNo", request.CstNo);
            parameters.Add("@VatNo", request.VatNo);
            parameters.Add("@ServiceTaxNo", request.ServiceTaxNo);
            parameters.Add("@EccNo", request.EccNo);
            parameters.Add("@LbtNo", request.LbtNo);
            parameters.Add("@ExciseRange", request.ExciseRange);
            parameters.Add("@ExciseDivision", request.ExciseDivision);
            parameters.Add("@ExciseCollectorate", request.ExciseCollectorate);
            parameters.Add("@TallyName", request.TallyName);
            parameters.Add("@CreditDays", request.CreditDays);
            parameters.Add("@TdsPercentage", request.TdsPercentage);
            parameters.Add("@IsActive", request.IsActive);
            parameters.Add("@IsLbtApplicable", request.IsLbtApplicable);

            await connection.ExecuteAsync(
                "ERP_UpdateCustomerMaster",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int id, int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@CompanyId", companyId);

            await connection.ExecuteAsync(
                "ERP_DeleteCustomerMaster",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CustomerMasterDto?> GetByIdAsync(int id, int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@CompanyId", companyId);

            var result = await connection.QueryFirstOrDefaultAsync<CustomerMasterDto>(
                "ERP_GetCustomerMasterById",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<PagedResponse<CustomerMasterDto>> GetAllAsync(CustomerMasterQueryParameters parameters)
        {
            using var connection = new SqlConnection(_connectionString);
            var spParameters = new DynamicParameters();
            
            spParameters.Add("@CompanyId", parameters.CompanyId);
            spParameters.Add("@IsActive", parameters.IsActive);
            spParameters.Add("@AreaCode", parameters.AreaCode);
            spParameters.Add("@CustomerType", parameters.CustomerType);
            spParameters.Add("@StateCode", parameters.StateCode);
            spParameters.Add("@CityCode", parameters.CityCode);
            spParameters.Add("@CategoryCode", parameters.CategoryCode);
            spParameters.Add("@SearchTerm", parameters.SearchTerm);
            spParameters.Add("@SortBy", parameters.SortBy);
            spParameters.Add("@SortDescending", parameters.SortDirection == "desc" ? 1 : 0);
            spParameters.Add("@PageNumber", parameters.PageNumber);
            spParameters.Add("@PageSize", parameters.PageSize);
            spParameters.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var customers = await connection.QueryAsync<CustomerMasterDto>(
                "ERP_GetCustomerMasters",
                spParameters,
                commandType: CommandType.StoredProcedure);

            var totalRecords = spParameters.Get<int>("@TotalRecords");

            return new PagedResponse<CustomerMasterDto>
            {
                Data = customers.ToList(),
                TotalCount = totalRecords,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<bool> IsPartyNameUniqueAsync(string partyName, int? id, int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@PartyName", partyName);
            parameters.Add("@Id", id);
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@IsUnique", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "ERP_CheckPartyNameUnique",
                parameters,
                commandType: CommandType.StoredProcedure);

            return parameters.Get<bool>("@IsUnique");
        }

        public async Task<bool> IsAbbreviationUniqueAsync(string abbreviation, int? id, int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Abbreviation", abbreviation);
            parameters.Add("@Id", id);
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@IsUnique", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "ERP_CheckAbbreviationUnique",
                parameters,
                commandType: CommandType.StoredProcedure);

            return parameters.Get<bool>("@IsUnique");
        }
    }
}

