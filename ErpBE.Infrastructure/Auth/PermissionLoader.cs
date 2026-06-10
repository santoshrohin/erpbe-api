using Dapper;
using ErpBE.Domain.Auth;
using System.Data;

namespace ErpBE.Infrastructure.Auth
{
    /// <summary>
    /// Loads USER_RIGHT bitmask entries for a user from the shared database.
    /// Called once at login time — results are embedded in JWT claims.
    /// Read-only query; no risk to legacy system.
    /// </summary>
    public class PermissionLoader : IPermissionLoader
    {
        private readonly IDbConnection _db;

        public PermissionLoader(IDbConnection db)
        {
            _db = db;
        }

        public async Task<Dictionary<int, string>> LoadAsync(
            int userCode,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserCode", userCode);

            var rows = await _db.QueryAsync<PermissionRow>(
                "ERP_GetUserPermissions",
                parameters,
                commandType: CommandType.StoredProcedure);

            var result = new Dictionary<int, string>();
            foreach (var row in rows)
            {
                // Validate bitmask length — legacy stores exactly 7 characters
                var mask = (row.RightsBitmask ?? string.Empty).PadRight(7, '0');
                if (mask.Length > 7) mask = mask[..7];
                result[row.ModuleCode] = mask;
            }

            return result;
        }

        private sealed class PermissionRow
        {
            public int    ModuleCode     { get; set; }
            public string RightsBitmask  { get; set; } = string.Empty;
        }
    }
}
