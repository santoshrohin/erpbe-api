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

            // Query matches legacy: select distinct CM_CODE, 'From  ' +convert(varchar(10),CM_OPENING_DATE,103)+' To '+ convert(varchar(10),CM_CLOSING_DATE,103)as FINANCIAL from COMPANY_MASTER where CM_ID=<companyId> order by CM_CODE desc
            var query = @"
                SELECT DISTINCT 
                    CM_CODE AS Id,
                    'From ' + CONVERT(VARCHAR(10), CM_OPENING_DATE, 103) + ' To ' + CONVERT(VARCHAR(10), CM_CLOSING_DATE, 103) AS DisplayName,
                    CM_CODE AS FinancialYearCode,
                    CONVERT(VARCHAR(10), CM_OPENING_DATE, 103) AS OpeningDate,
                    CONVERT(VARCHAR(10), CM_CLOSING_DATE, 103) AS ClosingDate
                FROM COMPANY_MASTER 
                WHERE CM_ID = @CompanyId 
                ORDER BY CM_CODE DESC";

            var financialYears = await connection.QueryAsync<FinancialYearDto>(
                query,
                new { CompanyId = companyId }
            );

            return financialYears.ToList();
        }
    }
}

