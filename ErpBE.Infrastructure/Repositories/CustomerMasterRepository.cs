using Dapper;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
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
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<CustomerMasterDto> CreateAsync(CreateCustomerMasterRequest request, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@PartyName", request.PartyName);
            parameters.Add("@ContactPerson", request.ContactPerson);
            parameters.Add("@Abbreviation", request.Abbreviation);
            parameters.Add("@Address", request.Address);
            parameters.Add("@Phone", request.Phone);
            parameters.Add("@Mobile", request.Mobile);
            parameters.Add("@Email", request.Email);
            parameters.Add("@Website", request.Website);
            parameters.Add("@FaxNo", request.FaxNo);
            parameters.Add("@AreaCode", request.AreaCode);
            parameters.Add("@CustomerType", request.CustomerType);
            parameters.Add("@CountryCode", request.CountryCode);
            parameters.Add("@StateCode", request.StateCode);
            parameters.Add("@CityCode", request.CityCode);
            parameters.Add("@PinCode", request.PinCode);
            parameters.Add("@VatTinNo", request.VatTinNo);
            parameters.Add("@CstNo", request.CstNo);
            parameters.Add("@GstNo", request.GstNo);
            parameters.Add("@PanNo", request.PanNo);
            parameters.Add("@ServiceTaxNo", request.ServiceTaxNo);
            parameters.Add("@TallyName", request.TallyName);
            parameters.Add("@OpeningBalance", request.OpeningBalance);
            parameters.Add("@OpeningBalanceType", request.OpeningBalanceType);
            parameters.Add("@CreditLimit", request.CreditLimit);
            parameters.Add("@CreditDays", request.CreditDays);
            parameters.Add("@BankName", request.BankName);
            parameters.Add("@BankAccountNo", request.BankAccountNo);
            parameters.Add("@BankBranchName", request.BankBranchName);
            parameters.Add("@BankIfscCode", request.BankIfscCode);
            parameters.Add("@IsLbtApplicable", request.IsLbtApplicable);
            parameters.Add("@IsSezCustomer", request.IsSezCustomer);
            parameters.Add("@IsCompositeDealer", request.IsCompositeDealer);
            parameters.Add("@Remark", request.Remark);

            var result = await connection.QuerySingleAsync<CustomerMasterDto>(
                "ERP_CreateCustomerMaster",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<bool> UpdateAsync(UpdateCustomerMasterRequest request, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.Id);
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@PartyCode", request.PartyCode);
            parameters.Add("@PartyName", request.PartyName);
            parameters.Add("@ContactPerson", request.ContactPerson);
            parameters.Add("@Abbreviation", request.Abbreviation);
            parameters.Add("@Address", request.Address);
            parameters.Add("@Phone", request.Phone);
            parameters.Add("@Mobile", request.Mobile);
            parameters.Add("@Email", request.Email);
            parameters.Add("@Website", request.Website);
            parameters.Add("@FaxNo", request.FaxNo);
            parameters.Add("@AreaCode", request.AreaCode);
            parameters.Add("@CustomerType", request.CustomerType);
            parameters.Add("@CountryCode", request.CountryCode);
            parameters.Add("@StateCode", request.StateCode);
            parameters.Add("@CityCode", request.CityCode);
            parameters.Add("@PinCode", request.PinCode);
            parameters.Add("@VatTinNo", request.VatTinNo);
            parameters.Add("@CstNo", request.CstNo);
            parameters.Add("@GstNo", request.GstNo);
            parameters.Add("@PanNo", request.PanNo);
            parameters.Add("@ServiceTaxNo", request.ServiceTaxNo);
            parameters.Add("@TallyName", request.TallyName);
            parameters.Add("@OpeningBalance", request.OpeningBalance);
            parameters.Add("@OpeningBalanceType", request.OpeningBalanceType);
            parameters.Add("@CreditLimit", request.CreditLimit);
            parameters.Add("@CreditDays", request.CreditDays);
            parameters.Add("@BankName", request.BankName);
            parameters.Add("@BankAccountNo", request.BankAccountNo);
            parameters.Add("@BankBranchName", request.BankBranchName);
            parameters.Add("@BankIfscCode", request.BankIfscCode);
            parameters.Add("@IsLbtApplicable", request.IsLbtApplicable);
            parameters.Add("@IsSezCustomer", request.IsSezCustomer);
            parameters.Add("@IsCompositeDealer", request.IsCompositeDealer);
            parameters.Add("@Remark", request.Remark);

            var rowsAffected = await connection.ExecuteAsync(
                "ERP_UpdateCustomerMaster",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@CompanyId", companyId);

            var rowsAffected = await connection.ExecuteAsync(
                "ERP_DeleteCustomerMaster",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0;
        }

        public async Task<CustomerMasterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var result = await connection.QueryFirstOrDefaultAsync<CustomerMasterDto>(
                "ERP_GetCustomerMasterById",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<PagedResponse<CustomerMasterDto>> GetAllAsync(CustomerMasterQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("@CompanyId", parameters.CompanyId);
            dynamicParams.Add("@IsActive", parameters.IsActive);
            dynamicParams.Add("@AreaCode", parameters.AreaCode);
            dynamicParams.Add("@CustomerType", parameters.CustomerType);
            dynamicParams.Add("@CountryCode", parameters.CountryCode);
            dynamicParams.Add("@StateCode", parameters.StateCode);
            dynamicParams.Add("@CityCode", parameters.CityCode);
            dynamicParams.Add("@SearchTerm", parameters.SearchTerm);
            dynamicParams.Add("@SortBy", parameters.SortBy);
            dynamicParams.Add("@SortDescending", parameters.SortDirection == "desc");
            dynamicParams.Add("@PageNumber", parameters.PageNumber);
            dynamicParams.Add("@PageSize", parameters.PageSize);
            dynamicParams.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = await connection.QueryAsync<CustomerMasterDto>(
                "ERP_GetCustomerMasters",
                dynamicParams,
                commandType: CommandType.StoredProcedure
            );

            var totalCount = dynamicParams.Get<int>("@TotalCount");

            return new PagedResponse<CustomerMasterDto>(
                items.ToList(),
                totalCount,
                parameters.PageNumber,
                parameters.PageSize
            );
        }

        public async Task<bool> IsPartyNameUniqueAsync(string partyName, int? id, int companyId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@PartyName", partyName);
            parameters.Add("@Id", id);
            parameters.Add("@CompanyId", companyId);

            var result = await connection.QuerySingleAsync<int>(
                "ERP_IsPartyNameUnique",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result == 1;
        }

        public async Task<bool> IsAbbreviationUniqueAsync(string abbreviation, int? id, int companyId, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Abbreviation", abbreviation);
            parameters.Add("@Id", id);
            parameters.Add("@CompanyId", companyId);

            var result = await connection.QuerySingleAsync<int>(
                "ERP_IsAbbreviationUnique",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result == 1;
        }

        public async Task<bool> IsModifiedByAnotherUserAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var result = await connection.QuerySingleAsync<int>(
                "ERP_IsCustomerModified",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result == 1;
        }

        public async Task<bool> IsUsedInTransactionsAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var result = await connection.QuerySingleAsync<int>(
                "ERP_CheckCustomerUsage",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result == 1;
        }
    }
}

