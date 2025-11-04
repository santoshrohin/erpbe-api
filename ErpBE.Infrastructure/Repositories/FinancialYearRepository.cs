using Dapper;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class FinancialYearRepository : IFinancialYearRepository
    {
        private readonly string _connectionString;

        public FinancialYearRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<List<FinancialYearDto>> GetFinancialYearsByCompanyIdAsync(int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var financialYears = await connection.QueryAsync<FinancialYearDto>(
                "ERP_GetFinancialYearsByCompanyId",
                new { CompanyId = companyId },
                commandType: CommandType.StoredProcedure
            );

            return financialYears.ToList();
        }
    }
}

