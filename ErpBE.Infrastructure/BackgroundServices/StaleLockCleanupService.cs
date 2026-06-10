using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBE.Infrastructure.BackgroundServices;

/// <summary>
/// Runs every 15 minutes inside the API process (no separate deployment needed).
/// Calls ERP_CleanStaleLocks which clears MODIFY = 1 rows whose lock timestamp
/// has exceeded the timeout window across all registered module tables.
///
/// To support a new module table: add an UPDATE block to ERP_CleanStaleLocks.sql.
/// No changes to this class are required.
/// </summary>
public sealed class StaleLockCleanupService : BackgroundService
{
    private static readonly TimeSpan SweepInterval  = TimeSpan.FromMinutes(15);
    private const           int      TimeoutMinutes  = 30;

    private readonly string                   _connectionString;
    private readonly ILogger<StaleLockCleanupService> _logger;

    public StaleLockCleanupService(IConfiguration configuration,
                                   ILogger<StaleLockCleanupService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StaleLockCleanupService started. Sweep interval: {Interval} min, Timeout: {Timeout} min.",
            SweepInterval.TotalMinutes, TimeoutMinutes);

        // Wait one full interval before the first sweep so startup noise settles.
        await Task.Delay(SweepInterval, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SweepAsync(stoppingToken);
            await Task.Delay(SweepInterval, stoppingToken);
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await connection.ExecuteAsync(
                "ERP_CleanStaleLocks",
                new { TimeoutMinutes },
                commandType: CommandType.StoredProcedure);

            _logger.LogDebug("Stale lock sweep completed.");
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // Log and continue — a transient DB failure must not kill the service.
            _logger.LogWarning(ex, "Stale lock sweep failed. Will retry on next interval.");
        }
    }
}
