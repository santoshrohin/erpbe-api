using Dapper;
using ErpBE.Domain.Interfaces;
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
            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(level))
            {
                whereClause += " AND Level = @Level";
                parameters.Add("@Level", level);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += " AND Message LIKE @SearchTerm";
                parameters.Add("@SearchTerm", $"%{searchTerm}%");
            }

            if (startDate.HasValue)
            {
                whereClause += " AND TimeStamp >= @StartDate";
                parameters.Add("@StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                whereClause += " AND TimeStamp <= @EndDate";
                parameters.Add("@EndDate", endDate.Value);
            }

            using var connection = new SqlConnection(_connectionString);

            // Get total count
            var countQuery = $@"
                SELECT COUNT(*) 
                FROM Logs 
                {whereClause}";

            var count = await connection.QuerySingleAsync<int>(countQuery, parameters);

            // Get logs
            var logsQuery = $@"
                SELECT 
                    Id,
                    TimeStamp,
                    ISNULL(Level, 'Information') as Level,
                    ISNULL(Message, 'No message') as Message,
                    ISNULL(Exception, '') as Exception,
                    ISNULL(Properties, '') as Properties,
                    ISNULL(UserId, '') as UserId,
                    ISNULL(RequestId, '') as RequestId,
                    ISNULL(ActionName, '') as ActionName
                FROM Logs 
                {whereClause}
                ORDER BY TimeStamp DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", pageSize);

            var logs = await connection.QueryAsync(logsQuery, parameters);

            return (logs, count);
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

