using ErpBE.Domain.Auth;
using ErpBE.Domain.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ErpBE.Infrastructure.Auth
{
    /// <summary>
    /// Reads company and financial-year context from JWT claims.
    /// Scoped per-request — injected into repositories to enforce multi-tenant isolation.
    ///
    /// Usage in repositories:
    ///   parameters.Add("@CompanyId",         _companyContext.CompanyId);
    ///   parameters.Add("@FinancialYearCode",  _companyContext.FinancialYearCode);
    /// </summary>
    public class CompanyContext : ICompanyContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int    UserCode          => GetInt("user_code");
        public string Username          => GetString(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)
                                        ?? GetString("unique_name")
                                        ?? string.Empty;
        public int    CompanyId         => GetInt("company_id");
        public int    FinancialYearCode => GetInt("financial_year_code");
        public string OpeningDate       => GetString("fy_opening")  ?? string.Empty;
        public string ClosingDate       => GetString("fy_closing")  ?? string.Empty;
        public bool   IsAdmin           => GetString("is_admin") == "true";

        public string GetPermissions(int moduleCode)
        {
            var claim = User?.FindFirst(PermissionClaimNames.For(moduleCode));
            return claim?.Value ?? "0000000";
        }

        public bool HasPermission(int moduleCode, int bitPosition)
        {
            var mask = GetPermissions(moduleCode);
            if (bitPosition < 0 || bitPosition >= mask.Length) return false;
            return mask[bitPosition] == '1';
        }

        // ─── helpers ────────────────────────────────────────────────────────

        private string? GetString(string claimType)
            => User?.FindFirst(claimType)?.Value;

        private int GetInt(string claimType)
            => int.TryParse(GetString(claimType), out var v) ? v : 0;
    }
}
