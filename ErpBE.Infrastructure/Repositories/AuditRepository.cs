using Dapper;
using ErpBE.Domain.Audit;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly IDbConnection _db;

        public AuditRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(AuditEntry auditEntry)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@EntityName", auditEntry.EntityName);
            parameters.Add("@EntityId", auditEntry.EntityId);
            parameters.Add("@Action", auditEntry.Action);
            parameters.Add("@UserId", auditEntry.UserId);
            parameters.Add("@UserName", auditEntry.UserName);
            parameters.Add("@UserRole", auditEntry.UserRole);
            parameters.Add("@CompanyId", auditEntry.CompanyId);
            parameters.Add("@IpAddress", auditEntry.IpAddress);
            parameters.Add("@UserAgent", auditEntry.UserAgent);
            parameters.Add("@Endpoint", auditEntry.Endpoint);
            parameters.Add("@HttpMethod", auditEntry.HttpMethod);
            parameters.Add("@Description", auditEntry.Description);
            parameters.Add("@OldValues", auditEntry.OldValues);
            parameters.Add("@NewValues", auditEntry.NewValues);
            parameters.Add("@Timestamp", auditEntry.Timestamp);
            parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _db.ExecuteAsync(
                "SP_CreateAuditEntry",
                parameters,
                commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@Id");
        }

        public async Task<AuditEntry?> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var result = await _db.QueryFirstOrDefaultAsync<AuditEntry>(
                "SP_GetAuditEntryById",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<PagedResponse<AuditEntryDto>> GetPagedAsync(AuditQueryParameters parameters)
        {
            var p = new DynamicParameters();
            p.Add("@PageNumber", parameters.PageNumber);
            p.Add("@PageSize", parameters.PageSize);
            p.Add("@SortBy", parameters.SortBy ?? "Timestamp");
            p.Add("@SortDirection", parameters.SortDirection ?? "DESC");
            p.Add("@SearchTerm", parameters.SearchTerm);
            p.Add("@EntityName", parameters.EntityName);
            p.Add("@EntityId", parameters.EntityId);
            p.Add("@Action", parameters.Action);
            p.Add("@UserId", parameters.UserId);
            p.Add("@UserName", parameters.UserName);
            p.Add("@FromDate", parameters.FromDate);
            p.Add("@ToDate", parameters.ToDate);
            p.Add("@Endpoint", parameters.Endpoint);
            p.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = await _db.QueryAsync<AuditEntryDto>(
                "SP_GetAuditEntriesWithFilters",
                p,
                commandType: CommandType.StoredProcedure);

            var totalCount = p.Get<int>("@TotalCount");

            return new PagedResponse<AuditEntryDto>(data.ToList(), totalCount, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<List<AuditEntryDto>> GetAuditHistoryAsync(string entityName, string entityId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@EntityName", entityName);
            parameters.Add("@EntityId", entityId);

            var result = await _db.QueryAsync<AuditEntryDto>(
                "SP_GetAuditHistory",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<AuditConfiguration?> GetAuditConfigurationAsync(string endpoint, string httpMethod)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Endpoint", endpoint);
            parameters.Add("@HttpMethod", httpMethod);

            var result = await _db.QueryFirstOrDefaultAsync<AuditConfiguration>(
                "SP_GetAuditConfiguration",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> CreateConfigurationAsync(AuditConfiguration configuration)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Endpoint", configuration.Endpoint);
            parameters.Add("@HttpMethod", configuration.HttpMethod);
            parameters.Add("@EntityName", configuration.EntityName);
            parameters.Add("@EntityIdProperty", configuration.EntityIdProperty);
            parameters.Add("@IsEnabled", configuration.IsEnabled);
            parameters.Add("@TrackPropertyChanges", configuration.TrackPropertyChanges);
            parameters.Add("@TrackOldValues", configuration.TrackOldValues);
            parameters.Add("@TrackNewValues", configuration.TrackNewValues);
            parameters.Add("@Description", configuration.Description);
            parameters.Add("@CreatedBy", configuration.CreatedBy);
            parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _db.ExecuteAsync(
                "SP_CreateAuditConfiguration",
                parameters,
                commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@Id");
        }

        public async Task<bool> UpdateConfigurationAsync(AuditConfiguration configuration)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", configuration.Id);
            parameters.Add("@IsEnabled", configuration.IsEnabled);
            parameters.Add("@TrackPropertyChanges", configuration.TrackPropertyChanges);
            parameters.Add("@TrackOldValues", configuration.TrackOldValues);
            parameters.Add("@TrackNewValues", configuration.TrackNewValues);
            parameters.Add("@Description", configuration.Description);

            var result = await _db.ExecuteAsync(
                "SP_UpdateAuditConfiguration",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<List<AuditConfiguration>> GetAllConfigurationsAsync()
        {
            var result = await _db.QueryAsync<AuditConfiguration>(
                "SP_GetAllAuditConfigurations",
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
    }
}
