using Dapper;
using ErpBE.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ErpBE.Infrastructure.Services;

/// <summary>
/// Writes activity log entries to LOG_MASTER via ERP_WriteActivityLog.
/// Matches the legacy CommonClasses.WriteLog() behavior exactly.
/// Swallows all exceptions — a logging failure must never abort the caller.
/// </summary>
public sealed class ActivityLogService : IActivityLogService
{
    private readonly IDbConnection _db;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(IDbConnection db, ILogger<ActivityLogService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task WriteLogAsync(
        int    companyId,
        string source,
        string @event,
        string docName,
        string docNo,
        long   docCode,
        string userName,
        int    userCode,
        string? ipAddress          = null,
        int?   companyCode         = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var p = new DynamicParameters();
            p.Add("@CompanyId",   companyId);
            p.Add("@Source",      source);
            p.Add("@Event",       @event);
            p.Add("@DocName",     docName);
            p.Add("@DocNo",       docNo);
            p.Add("@DocCode",     docCode);
            p.Add("@UserName",    userName);
            p.Add("@UserCode",    userCode);
            p.Add("@IpAddress",   ipAddress);
            p.Add("@CompanyCode", companyCode);

            await _db.ExecuteAsync(
                "ERP_WriteActivityLog",
                p,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to write activity log: Source={Source} Event={Event} DocCode={DocCode}",
                source, @event, docCode);
        }
    }
}
