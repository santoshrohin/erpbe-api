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

            // Query matches legacy: select distinct CM_ID,CM_NAME from COMPANY_MASTER where CM_ACTIVE_IND=1
            var query = @"
                SELECT DISTINCT 
                    CM_ID AS Id,
                    CM_NAME AS DisplayName
                FROM COMPANY_MASTER 
                WHERE CM_ACTIVE_IND = 1 
                ORDER BY CM_NAME";

            var companies = await connection.QueryAsync<CompanyDto>(query);
            return companies.ToList();
        }
    }
}

