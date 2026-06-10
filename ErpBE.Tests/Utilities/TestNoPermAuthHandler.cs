using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ErpBE.Tests.Utilities;

/// <summary>
/// Fake authentication handler that authenticates as a non-admin user
/// with zero permissions on every module. Exercises the RequirePermission
/// attribute's 403 path for all business controllers.
/// </summary>
public class TestNoPermAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestNoPermAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, "TestUser"),
            new Claim("is_admin", "false"),
            new Claim("user_code", "99"),
            new Claim("company_id", "1"),
            new Claim("financial_year_code", "-2147483641"),
            new Claim("fy_opening", "01/04/2024"),
            new Claim("fy_closing", "31/03/2025"),
            // All modules get a "no permission" bitmask — 7 bits all zero
            new Claim("perm_72", "0000000"),  // Masters
            new Claim("perm_73", "0000000"),  // Purchase
            new Claim("perm_74", "0000000"),  // Production
            new Claim("perm_75", "0000000"),  // Sales
            new Claim("perm_76", "0000000"),  // Utility
            new Claim("perm_77", "0000000"),  // Admin
        };

        var identity = new ClaimsIdentity(claims, "TestNoPerm");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestNoPerm");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
