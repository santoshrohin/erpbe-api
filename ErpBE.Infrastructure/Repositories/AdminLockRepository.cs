using System.Data;
using Dapper;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ErpBE.Infrastructure.Repositories;

public class AdminLockRepository : IAdminLockRepository
{
    private readonly string _connectionString;

    public AdminLockRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    public async Task<IEnumerable<ActiveLockDto>> GetActiveLocksAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.QueryAsync<ActiveLockDto>(
            "ERP_GetActiveLocks",
            commandType: CommandType.StoredProcedure);
        return rows;
    }

    public async Task<bool> ForceUnlockAsync(string module, int recordId)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteScalarAsync<int>(
            "ERP_ForceUnlock",
            new { Module = module, RecordId = recordId },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }
}
