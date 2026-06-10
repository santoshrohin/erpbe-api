using Dapper;
using ErpBE.Domain.Auth;
using System.Data;

namespace ErpBE.Infrastructure.Auth
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnection _db;

        public RefreshTokenRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(
            int      userCode,
            string   tokenHash,
            DateTime expiresAt,
            string?  ipAddress  = null,
            string?  userAgent  = null,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserCode",  userCode);
            parameters.Add("@TokenHash", tokenHash);
            parameters.Add("@ExpiresAt", expiresAt);
            parameters.Add("@IpAddress", ipAddress);
            parameters.Add("@UserAgent", userAgent);

            var result = await _db.QueryFirstOrDefaultAsync<int>(
                "ERP_CreateRefreshToken",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int?> ValidateAndRotateAsync(
            string currentTokenHash,
            string newTokenHash,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TokenHash",    currentTokenHash);
            parameters.Add("@NewTokenHash", newTokenHash);

            var result = await _db.QueryFirstOrDefaultAsync<int?>(
                "ERP_ValidateAndRotateRefreshToken",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task RevokeAllForUserAsync(
            int userCode,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserCode", userCode);

            await _db.ExecuteAsync(
                "ERP_RevokeUserRefreshTokens",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
