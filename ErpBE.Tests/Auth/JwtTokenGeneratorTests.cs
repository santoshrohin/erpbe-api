using ErpBE.Domain.Auth;
using ErpBE.Domain.Common;
using ErpBE.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Unit tests for JwtTokenGenerator.
/// Verifies that access tokens contain all required claims and that
/// refresh tokens are sufficiently random and non-repeating.
/// </summary>
public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtTokenGeneratorTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"]                   = "TestSecretKeyThatIsLongEnoughForHS256AlgorithmRequirements",
                ["JwtSettings:Issuer"]                   = "TestIssuer",
                ["JwtSettings:Audience"]                 = "TestAudience",
                ["JwtSettings:AccessTokenExpiryMinutes"] = "60",
            })
            .Build();

        _generator = new JwtTokenGenerator(config);
    }

    private static LoginResult BuildUser(bool isAdmin = false, Dictionary<int, string>? permissions = null) => new()
    {
        UserCode          = 42,
        Username          = "testuser",
        DisplayName       = "Test User",
        UserEmail         = "test@example.com",
        IsAdmin           = isAdmin,
        CompanyId         = 1,
        FinancialYearCode = -2147483641,
        CompanyName       = "Test Company",
        CompanyEmail      = "company@example.com",
        OpeningDate       = "01/04/2024",
        ClosingDate       = "31/03/2025",
        Permissions       = permissions ?? new Dictionary<int, string>(),
    };

    // ─── Access token structure ──────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var token = _generator.GenerateAccessToken(BuildUser());
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_IsValidJwt()
    {
        var token = _generator.GenerateAccessToken(BuildUser());
        _handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserCodeClaim()
    {
        var user  = BuildUser();
        var token = _generator.GenerateAccessToken(user);
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "user_code" && c.Value == "42");
    }

    [Fact]
    public void GenerateAccessToken_ContainsUsernameClaim()
    {
        var user  = BuildUser();
        var token = _generator.GenerateAccessToken(user);
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName
                                      && c.Value == "testuser");
    }

    [Fact]
    public void GenerateAccessToken_ContainsCompanyIdClaim()
    {
        var user  = BuildUser();
        var token = _generator.GenerateAccessToken(user);
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "company_id" && c.Value == "1");
    }

    [Fact]
    public void GenerateAccessToken_ContainsFinancialYearCodeClaim()
    {
        var user  = BuildUser();
        var token = _generator.GenerateAccessToken(user);
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "financial_year_code" && c.Value == "-2147483641");
    }

    [Fact]
    public void GenerateAccessToken_ContainsIsAdminClaim()
    {
        var token = _generator.GenerateAccessToken(BuildUser(isAdmin: false));
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "is_admin" && c.Value == "false");
    }

    [Fact]
    public void GenerateAccessToken_Admin_IsAdminClaimIsTrue()
    {
        var token = _generator.GenerateAccessToken(BuildUser(isAdmin: true));
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "is_admin" && c.Value == "true");
    }

    [Fact]
    public void GenerateAccessToken_Admin_ContainsRoleAdminClaim()
    {
        var token = _generator.GenerateAccessToken(BuildUser(isAdmin: true));
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_NonAdmin_DoesNotContainRoleClaim()
    {
        var token = _generator.GenerateAccessToken(BuildUser(isAdmin: false));
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().NotContain(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_EmbedsBitmaskPermissionClaims()
    {
        var permissions = new Dictionary<int, string>
        {
            [75] = "1111000",
            [72] = "1010101",
        };
        var token = _generator.GenerateAccessToken(BuildUser(permissions: permissions));
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "perm_75" && c.Value == "1111000");
        jwt.Claims.Should().Contain(c => c.Type == "perm_72" && c.Value == "1010101");
    }

    [Fact]
    public void GenerateAccessToken_NoPermissions_HasNoPermClaims()
    {
        var token = _generator.GenerateAccessToken(BuildUser());
        var jwt   = _handler.ReadJwtToken(token);

        jwt.Claims.Should().NotContain(c => c.Type.StartsWith("perm_"));
    }

    [Fact]
    public void GenerateAccessToken_HasUniqueJtiPerCall()
    {
        var user = BuildUser();
        var t1   = _handler.ReadJwtToken(_generator.GenerateAccessToken(user));
        var t2   = _handler.ReadJwtToken(_generator.GenerateAccessToken(user));

        t1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value
            .Should().NotBe(
                t2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    // ─── Refresh token ──────────────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        _generator.GenerateRefreshToken().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64()
    {
        var token = _generator.GenerateRefreshToken();
        var act   = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ProducesUniqueValues()
    {
        var t1 = _generator.GenerateRefreshToken();
        var t2 = _generator.GenerateRefreshToken();
        t1.Should().NotBe(t2);
    }

    [Fact]
    public void GenerateRefreshToken_IsAtLeast64Bytes()
    {
        // 64 random bytes → 88-char base64 (without padding it's still >80 chars)
        _generator.GenerateRefreshToken().Length.Should().BeGreaterThanOrEqualTo(80);
    }
}
