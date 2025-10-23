using Dapper;
using ErpBE.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace ErpBE.Infrastructure.Repositories
{
    public class LogsRepository : ILogsRepository
    {
        private readonly string _connectionString;

        public LogsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<(IEnumerable<dynamic> logs, int totalCount)> GetLogsAsync(
            int pageNumber,
            int pageSize,
            string? level,
            string? searchTerm,
            DateTime? startDate,
            DateTime? endDate)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@Level", level);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);
            parameters.Add("@TotalCount", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            var logs = await connection.QueryAsync(
                "SP_GetLogs",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            var totalCount = parameters.Get<int>("@TotalCount");

            return (logs, totalCount);
        }

        public async Task<(IEnumerable<dynamic> levelStats, IEnumerable<dynamic> dailyStats)> GetLogStatisticsAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            // Use stored procedure for better performance
            var results = await connection.QueryMultipleAsync("SP_GetLogStatistics");
            var levelStats = await results.ReadAsync();
            var dailyStats = await results.ReadAsync();

            return (levelStats, dailyStats);
        }

        public async Task<int> CleanupOldLogsAsync(int daysToKeep)
        {
            using var connection = new SqlConnection(_connectionString);

            // Use stored procedure for better performance and safety
            var parameters = new DynamicParameters();
            parameters.Add("@DaysToKeep", daysToKeep);
            parameters.Add("@DeletedCount", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            await connection.ExecuteAsync("SP_CleanupOldLogs", parameters, commandType: System.Data.CommandType.StoredProcedure);

            return parameters.Get<int>("@DeletedCount");
        }
    }
}
