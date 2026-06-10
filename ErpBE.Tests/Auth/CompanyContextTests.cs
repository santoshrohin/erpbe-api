using ErpBE.Domain.Common;
using ErpBE.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Unit tests for CompanyContext.
/// Verifies that every property and method correctly reads from JWT claims.
/// </summary>
public class CompanyContextTests
{
    // ─── helpers ────────────────────────────────────────────────────────────

    private static CompanyContext BuildContext(params Claim[] claims)
    {
        var identity    = new ClaimsIdentity(claims, "TestAuth");
        var principal   = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        return new CompanyContext(accessor);
    }

    private static CompanyContext BuildEmptyContext()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        return new CompanyContext(accessor);
    }

    // ─── Properties ─────────────────────────────────────────────────────────

    [Fact]
    public void UserCode_ReadsFromUserCodeClaim()
    {
        var ctx = BuildContext(new Claim("user_code", "42"));
        ctx.UserCode.Should().Be(42);
    }

    [Fact]
    public void Username_ReadsFromUniqueNameClaim()
    {
        var ctx = BuildContext(new Claim(JwtRegisteredClaimNames.UniqueName, "mohan"));
        ctx.Username.Should().Be("mohan");
    }

    [Fact]
    public void CompanyId_ReadsFromCompanyIdClaim()
    {
        var ctx = BuildContext(new Claim("company_id", "7"));
        ctx.CompanyId.Should().Be(7);
    }

    [Fact]
    public void FinancialYearCode_ReadsFromClaim()
    {
        var ctx = BuildContext(new Claim("financial_year_code", "-2147483641"));
        ctx.FinancialYearCode.Should().Be(-2147483641);
    }

    [Fact]
    public void OpeningDate_ReadsFromFyOpeningClaim()
    {
        var ctx = BuildContext(new Claim("fy_opening", "01/04/2024"));
        ctx.OpeningDate.Should().Be("01/04/2024");
    }

    [Fact]
    public void ClosingDate_ReadsFromFyClosingClaim()
    {
        var ctx = BuildContext(new Claim("fy_closing", "31/03/2025"));
        ctx.ClosingDate.Should().Be("31/03/2025");
    }

    [Fact]
    public void IsAdmin_WhenIsAdminClaimIsTrue_ReturnsTrue()
    {
        var ctx = BuildContext(new Claim("is_admin", "true"));
        ctx.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WhenIsAdminClaimIsFalse_ReturnsFalse()
    {
        var ctx = BuildContext(new Claim("is_admin", "false"));
        ctx.IsAdmin.Should().BeFalse();
    }

    // ─── Null / missing context ──────────────────────────────────────────────

    [Fact]
    public void AllIntProperties_ReturnZero_WhenHttpContextIsNull()
    {
        var ctx = BuildEmptyContext();
        ctx.UserCode.Should().Be(0);
        ctx.CompanyId.Should().Be(0);
        ctx.FinancialYearCode.Should().Be(0);
    }

    [Fact]
    public void StringProperties_ReturnEmpty_WhenHttpContextIsNull()
    {
        var ctx = BuildEmptyContext();
        ctx.Username.Should().BeEmpty();
        ctx.OpeningDate.Should().BeEmpty();
        ctx.ClosingDate.Should().BeEmpty();
    }

    [Fact]
    public void IsAdmin_ReturnsFalse_WhenHttpContextIsNull()
    {
        BuildEmptyContext().IsAdmin.Should().BeFalse();
    }

    // ─── GetPermissions ──────────────────────────────────────────────────────

    [Fact]
    public void GetPermissions_ReturnsCorrectBitmask()
    {
        var ctx = BuildContext(new Claim(PermissionClaimNames.For(75), "1111000"));
        ctx.GetPermissions(75).Should().Be("1111000");
    }

    [Fact]
    public void GetPermissions_MissingModule_ReturnsAllZeros()
    {
        var ctx = BuildContext(); // no perm claims
        ctx.GetPermissions(75).Should().Be("0000000");
    }

    // ─── HasPermission ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, "1000000", true)]
    [InlineData(1, "1100000", true)]
    [InlineData(3, "1111000", true)]
    [InlineData(4, "1111000", false)]    // bit 4 is '0' in "1111000"
    [InlineData(6, "1111111", true)]
    public void HasPermission_ReturnsCorrectResult(int bitPos, string mask, bool expected)
    {
        var ctx = BuildContext(new Claim(PermissionClaimNames.For(75), mask));
        ctx.HasPermission(75, bitPos).Should().Be(expected);
    }

    [Fact]
    public void HasPermission_OutOfRangeBit_ReturnsFalse()
    {
        var ctx = BuildContext(new Claim(PermissionClaimNames.For(75), "1111111"));
        ctx.HasPermission(75, 99).Should().BeFalse();
    }

    [Fact]
    public void HasPermission_MissingClaim_ReturnsFalse()
    {
        var ctx = BuildContext(); // no perm claim
        ctx.HasPermission(75, 0).Should().BeFalse();
    }
}
