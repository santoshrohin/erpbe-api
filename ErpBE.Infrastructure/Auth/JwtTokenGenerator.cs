using ErpBE.Domain.Auth;
using ErpBE.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ErpBE.Infrastructure.Auth
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(LoginResult user)
        {
            var settings    = _configuration.GetSection("JwtSettings");
            var secret      = settings["Secret"]
                ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
            var issuer      = settings["Issuer"]   ?? "ErpBE.API";
            var audience    = settings["Audience"] ?? "ErpBE.Client";
            var expiryMins  = Convert.ToDouble(settings["AccessTokenExpiryMinutes"] ?? "60");

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = BuildClaims(user);

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            DateTime.UtcNow.AddMinutes(expiryMins),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // 64 cryptographically random bytes → 88-char base64 URL string
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        // ─── private ────────────────────────────────────────────────────────

        private static List<Claim> BuildClaims(LoginResult user)
        {
            var claims = new List<Claim>
            {
                // Standard JWT claims
                new(JwtRegisteredClaimNames.Sub,        user.UserCode.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),

                // User identity
                new("user_code",     user.UserCode.ToString()),
                new("display_name",  user.DisplayName),
                new("user_email",    user.UserEmail),
                new("is_admin",      user.IsAdmin.ToString().ToLower()),

                // Company / FY context — required for repository scoping
                new("company_id",          user.CompanyId.ToString()),
                new("financial_year_code", user.FinancialYearCode.ToString()),
                new("company_name",        user.CompanyName),
                new("fy_opening",          user.OpeningDate),
                new("fy_closing",          user.ClosingDate),
            };

            // Legacy bitmask permission claims — one per module
            // Claim name: "perm_75" = "1111000"
            foreach (var (moduleCode, bitmask) in user.Permissions)
            {
                claims.Add(new Claim(PermissionClaimNames.For(moduleCode), bitmask));
            }

            // Admin users implicitly get full access to all Admin module functions
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            return claims;
        }
    }
}
