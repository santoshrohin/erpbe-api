using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using ErpBE.API.Common;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can view logs
    public class LogsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public LogsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        /// <summary>
        /// Get application logs with filtering and pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50)</param>
        /// <param name="level">Log level filter (Information, Warning, Error, etc.)</param>
        /// <param name="searchTerm">Search term for message content</param>
        /// <param name="startDate">Start date filter</param>
        /// <param name="endDate">End date filter</param>
        /// <returns>Paginated list of logs</returns>
        [HttpGet]
        public async Task<IActionResult> GetLogs(
            int pageNumber = 1,
            int pageSize = 50,
            string? level = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
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

                // Get total count
                var countQuery = $@"
                    SELECT COUNT(*) 
                    FROM Logs 
                    {whereClause}";

                using var connection = new SqlConnection(_connectionString);
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

                var totalPages = (int)Math.Ceiling((double)count / pageSize);

                return Ok(new
                {
                    Data = logs,
                    TotalCount = count,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving logs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get log statistics
        /// </summary>
        /// <returns>Log statistics by level and date</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetLogStatistics()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                
                // Use stored procedure for better performance
                var results = await connection.QueryMultipleAsync("SP_GetLogStatistics");
                var levelStats = await results.ReadAsync();
                var dailyStats = await results.ReadAsync();

                return Ok(new
                {
                    LevelStatistics = levelStats,
                    DailyStatistics = dailyStats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving log statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Debug endpoint to see raw database structure
        /// </summary>
        /// <returns>Raw database data</returns>
        [HttpGet("debug")]
        [AuthorizeAdmin]
        public async Task<IActionResult> DebugLogs()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                
                // Get table structure
                var structureQuery = @"
                    SELECT 
                        COLUMN_NAME,
                        DATA_TYPE,
                        IS_NULLABLE,
                        CHARACTER_MAXIMUM_LENGTH
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Logs' 
                    ORDER BY ORDINAL_POSITION";
                
                var structure = await connection.QueryAsync(structureQuery);
                
                // Get sample data
                var sampleQuery = "SELECT TOP 3 * FROM Logs ORDER BY Id DESC";
                var sampleData = await connection.QueryAsync(sampleQuery);
                
                return Ok(new
                {
                    TableStructure = structure,
                    SampleData = sampleData,
                    Message = "Debug information retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving debug info", error = ex.Message });
            }
        }

        /// <summary>
        /// Clear old logs (older than specified days)
        /// </summary>
        /// <param name="daysToKeep">Number of days to keep logs (default: 30)</param>
        /// <returns>Number of logs deleted</returns>
        [HttpDelete("cleanup")]
        [AuthorizeAdmin] // Only admins can clean up logs
        public async Task<IActionResult> CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                
                // Use stored procedure for better performance and safety
                var parameters = new DynamicParameters();
                parameters.Add("@DaysToKeep", daysToKeep);
                parameters.Add("@DeletedCount", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                await connection.ExecuteAsync("SP_CleanupOldLogs", parameters, commandType: System.Data.CommandType.StoredProcedure);
                
                var deletedCount = parameters.Get<int>("@DeletedCount");

                return Ok(new { message = $"Deleted {deletedCount} old log entries", deletedCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error cleaning up logs", error = ex.Message });
            }
        }
    }
}
