using BCrypt.Net;
using Dapper;
using ErpBE.Application.Common;
using ErpBE.Domain.Auth;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ErpBE.Infrastructure.Auth
{
    /// <summary>
    /// Handles authentication against the shared SQL Server database.
    ///
    /// Dual-login coexistence strategy (users can log into BOTH systems):
    ///   Columns used:
    ///     UM_PASSWORD    — legacy cipher; written by legacy system; NEVER touched here
    ///     UM_BCrypt_Hash — BCrypt hash; written only by the modern system
    ///
    ///   Auth order:
    ///     1. If UM_BCrypt_Hash is present → BCrypt.Verify(plain, UM_BCrypt_Hash)
    ///     2. Else check UM_PASSWORD format:
    ///        a. BCrypt format ($2a$/$2b$) → BCrypt.Verify  (old data: pre-migration rehash)
    ///        b. Legacy cipher format      → LegacyEncryption.Verify
    ///     3. On step-2b success → fire-and-forget store BCrypt in UM_BCrypt_Hash
    ///        (UM_PASSWORD is NOT modified — legacy login remains working)
    ///
    ///   Prerequisite: run Migrate_Add_BCrypt_Hash_Column.sql before deploying.
    /// </summary>
    public class LoginRepository : ILoginRepository
    {
        private readonly IDbConnection   _db;
        private readonly IPermissionLoader _permissionLoader;
        private readonly ILogger<LoginRepository> _logger;

        public LoginRepository(
            IDbConnection    db,
            IPermissionLoader permissionLoader,
            ILogger<LoginRepository> logger)
        {
            _db               = db;
            _permissionLoader = permissionLoader;
            _logger           = logger;
        }

        public async Task<LoginResult?> AuthenticateAsync(
            string username,
            string plainPassword,
            int    companyId,
            int    financialYearCode,
            CancellationToken cancellationToken = default)
        {
            var userRecord = await FetchUserRecordAsync(username, companyId, financialYearCode);

            if (userRecord is null)
            {
                _logger.LogWarning("Login failed: user '{Username}' not found in company {CompanyId}, FY {FY}",
                    username, companyId, financialYearCode);
                return null;
            }

            string legacyCipher = userRecord.PasswordHash  ?? string.Empty;
            string bcryptHash   = userRecord.BCryptHash    ?? string.Empty;
            bool   isValid;
            bool   needsRehash = false;

            if (!string.IsNullOrEmpty(bcryptHash))
            {
                // Modern path: BCrypt hash stored in dedicated column
                isValid = BCrypt.Net.BCrypt.Verify(plainPassword, bcryptHash);
            }
            else if (LegacyEncryption.IsBcryptHash(legacyCipher))
            {
                // Transitional path: BCrypt was previously stored in UM_PASSWORD
                // (pre-migration data). Verify it and later write to UM_BCrypt_Hash.
                isValid    = BCrypt.Net.BCrypt.Verify(plainPassword, legacyCipher);
                needsRehash = isValid; // move hash to dedicated column
            }
            else
            {
                // Legacy path: UM_PASSWORD contains the cipher string
                isValid    = LegacyEncryption.Verify(plainPassword, legacyCipher);
                needsRehash = isValid; // store BCrypt in dedicated column; UM_PASSWORD untouched
            }

            if (!isValid)
            {
                _logger.LogWarning("Login failed: invalid password for '{Username}'", username);
                return null;
            }

            if (needsRehash)
                _ = RehashPasswordAsync(userRecord.UserCode, plainPassword);

            var permissions = await _permissionLoader.LoadAsync(userRecord.UserCode, cancellationToken);

            return new LoginResult
            {
                UserCode          = userRecord.UserCode,
                Username          = userRecord.Username,
                DisplayName       = userRecord.DisplayName,
                UserEmail         = userRecord.UserEmail,
                IsAdmin           = userRecord.IsAdmin,
                CompanyId         = userRecord.CompanyId,
                FinancialYearCode = userRecord.FinancialYearCode,
                CompanyName       = userRecord.CompanyName,
                CompanyEmail      = userRecord.CompanyEmail,
                OpeningDate       = userRecord.OpeningDate,
                ClosingDate       = userRecord.ClosingDate,
                Permissions       = permissions
            };
        }

        public async Task<LoginResult?> GetUserByCodeAsync(
            int userCode,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserCode", userCode);

            var row = await _db.QueryFirstOrDefaultAsync<UserAuthRow>(
                "ERP_GetUserByCode",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (row is null) return null;

            var permissions = await _permissionLoader.LoadAsync(userCode, cancellationToken);

            return new LoginResult
            {
                UserCode          = row.UserCode,
                Username          = row.Username,
                DisplayName       = row.DisplayName,
                UserEmail         = row.UserEmail,
                IsAdmin           = row.IsAdmin,
                CompanyId         = row.CompanyId,
                FinancialYearCode = row.FinancialYearCode,
                CompanyName       = row.CompanyName,
                CompanyEmail      = row.CompanyEmail,
                OpeningDate       = row.OpeningDate,
                ClosingDate       = row.ClosingDate,
                Permissions       = permissions
            };
        }

        // ─── private helpers ────────────────────────────────────────────────

        private async Task<UserAuthRow?> FetchUserRecordAsync(
            string username, int companyId, int financialYearCode)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Username",          username);
            parameters.Add("@CompanyId",         companyId);
            parameters.Add("@FinancialYearCode", financialYearCode);

            return await _db.QueryFirstOrDefaultAsync<UserAuthRow>(
                "ERP_GetUserForAuth",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        private async Task RehashPasswordAsync(int userCode, string plainPassword)
        {
            try
            {
                var bcryptHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);

                var parameters = new DynamicParameters();
                parameters.Add("@UserCode",   userCode);
                parameters.Add("@BcryptHash", bcryptHash);

                // ERP_StoreBcryptHash writes ONLY to UM_BCrypt_Hash — UM_PASSWORD is untouched,
                // so the legacy SP_VerifyLogin continues to work for this user.
                await _db.ExecuteAsync(
                    "ERP_StoreBcryptHash",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                _logger.LogInformation(
                    "BCrypt hash stored in UM_BCrypt_Hash for UserCode={UserCode}", userCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to store BCrypt hash for UserCode={UserCode}. " +
                    "User is logged in. Will retry on next login.", userCode);
            }
        }

        private sealed class UserAuthRow
        {
            public int    UserCode          { get; set; }
            public string Username          { get; set; } = string.Empty;
            public string DisplayName       { get; set; } = string.Empty;
            public string UserEmail         { get; set; } = string.Empty;
            public string PasswordHash      { get; set; } = string.Empty; // UM_PASSWORD (legacy cipher)
            public string BCryptHash        { get; set; } = string.Empty; // UM_BCrypt_Hash (modern system only)
            public bool   IsAdmin           { get; set; }
            public int    CompanyId         { get; set; }
            public int    FinancialYearCode { get; set; }
            public string CompanyName       { get; set; } = string.Empty;
            public string CompanyEmail      { get; set; } = string.Empty;
            public string OpeningDate       { get; set; } = string.Empty;
            public string ClosingDate       { get; set; } = string.Empty;
        }
    }
}
