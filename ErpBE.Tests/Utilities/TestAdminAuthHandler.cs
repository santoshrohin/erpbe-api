using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ErpBE.Tests.Utilities;

/// <summary>
/// Fake authentication handler that always authenticates requests as an Admin user.
/// Used in HTTP controller tests so they exercise controller code without needing
/// a real admin user in the database.
/// </summary>
public class TestAdminAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAdminAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestAdmin"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("is_admin", "true"),
            new Claim("user_code", "1"),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, "TestAdmin"),
            new Claim("company_id", "1"),
            new Claim("financial_year_code", "-2147483641"),
            new Claim("fy_opening", "01/04/2024"),
            new Claim("fy_closing", "31/03/2025"),
        };

        var identity = new ClaimsIdentity(claims, "TestAdmin");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestAdmin");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
