
using ErpBE.Application.DTOs;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Text.Json;

namespace ErpBE.Application.Audit
{
    public class AuditService : IAuditService
    {
        private readonly IDbConnection _db;

        public AuditService(IDbConnection db)
        {
            _db = db;
        }

        public async Task LogCreateAsync(string tableName, int recordId, object newValues, string createdBy, string? ipAddress = null, string? userAgent = null, string? sessionId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TableName", tableName);
            parameters.Add("@RecordId", recordId);
            parameters.Add("@ActionType", "INSERT");
            parameters.Add("@OldValues", (string?)null);
            parameters.Add("@NewValues", JsonSerializer.Serialize(newValues));
            parameters.Add("@CreatedBy", createdBy);
            parameters.Add("@IPAddress", ipAddress);
            parameters.Add("@UserAgent", userAgent);
            parameters.Add("@SessionId", sessionId);

            await _db.ExecuteAsync("SP_LogAuditTrail", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task LogUpdateAsync(string tableName, int recordId, object oldValues, object newValues, string modifiedBy, string? ipAddress = null, string? userAgent = null, string? sessionId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TableName", tableName);
            parameters.Add("@RecordId", recordId);
            parameters.Add("@ActionType", "UPDATE");
            parameters.Add("@OldValues", JsonSerializer.Serialize(oldValues));
            parameters.Add("@NewValues", JsonSerializer.Serialize(newValues));
            parameters.Add("@CreatedBy", modifiedBy);
            parameters.Add("@IPAddress", ipAddress);
            parameters.Add("@UserAgent", userAgent);
            parameters.Add("@SessionId", sessionId);

            await _db.ExecuteAsync("SP_LogAuditTrail", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task LogDeleteAsync(string tableName, int recordId, object oldValues, string deletedBy, string? ipAddress = null, string? userAgent = null, string? sessionId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TableName", tableName);
            parameters.Add("@RecordId", recordId);
            parameters.Add("@ActionType", "DELETE");
            parameters.Add("@OldValues", JsonSerializer.Serialize(oldValues));
            parameters.Add("@NewValues", (string?)null);
            parameters.Add("@CreatedBy", deletedBy);
            parameters.Add("@IPAddress", ipAddress);
            parameters.Add("@UserAgent", userAgent);
            parameters.Add("@SessionId", sessionId);

            await _db.ExecuteAsync("SP_LogAuditTrail", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<AuditTrailDto>> GetAuditTrailAsync(string tableName, int? recordId = null, int pageNumber = 1, int pageSize = 50)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TableName", tableName);
            parameters.Add("@RecordId", recordId);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await _db.QueryAsync<AuditTrailDto>("SP_GetAuditTrail", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }
    }
}