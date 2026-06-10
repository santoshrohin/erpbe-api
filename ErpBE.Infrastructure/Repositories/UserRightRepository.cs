using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ErpBE.Infrastructure.Repositories;

public class UserRightRepository : IUserRightRepository
{
    private readonly string _connectionString;

    public UserRightRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<ScreenMasterDto>> GetScreensAsync(CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<ScreenMasterDto>(
            "ERP_GetScreenMasters",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<UserRightDto>> GetUserRightsAsync(int userCode, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<UserRightDto>(
            "ERP_GetUserRights",
            new { UserCode = userCode },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpsertUserRightsAsync(int userCode, IEnumerable<UserRightRequest> rights, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var right in rights)
            {
                var bitmask = (right.Bitmask ?? "0000000").PadRight(7, '0');
                if (bitmask.Length > 7) bitmask = bitmask[..7];

                await connection.ExecuteAsync(
                    "ERP_UpsertUserRight",
                    new { UserCode = userCode, ScreenCode = right.ScreenCode, Bitmask = bitmask },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> CopyRightsAsync(int fromUserCode, int toUserCode, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteScalarAsync<int>(
            "ERP_CopyUserRights",
            new { FromUserCode = fromUserCode, ToUserCode = toUserCode },
            commandType: CommandType.StoredProcedure);
        return rows >= 0;
    }

    public async Task<bool> DeleteUserRightsAsync(int userCode, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteScalarAsync<int>(
            "ERP_DeleteUserRights",
            new { UserCode = userCode },
            commandType: CommandType.StoredProcedure);
        return rows >= 0;
    }
}
