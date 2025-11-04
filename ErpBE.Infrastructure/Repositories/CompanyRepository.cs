using Dapper;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly string _connectionString;

        public CompanyRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<List<CompanyDto>> GetActiveCompaniesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var companies = await connection.QueryAsync<CompanyDto>(
                "ERP_GetActiveCompanies",
                commandType: CommandType.StoredProcedure
            );
            return companies.ToList();
        }
    }
}

